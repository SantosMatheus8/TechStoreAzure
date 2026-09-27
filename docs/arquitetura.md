# Arquitetura — TechStore Cloud

## Visão Geral

O TechStore Cloud é um sistema de cadastro de produtos para uma loja de tecnologia, composto por:

- **Frontend estático** (HTML/CSS/JS) hospedado no Azure Blob Storage (Static Website)
- **API REST** em .NET 8 (Minimal API) hospedada em uma VM Ubuntu no Azure
- **Banco de dados** Azure SQL Database
- **Monitoramento** com Azure Monitor, Log Analytics e Application Insights

---

## Decisão Arquitetural: Monólito Modular

### O que são microsserviços

Microsserviços são um estilo de arquitetura onde cada funcionalidade de negócio é implementada como um serviço independente, com seu próprio processo, banco de dados e ciclo de deploy. Os serviços se comunicam por rede (HTTP, mensageria) e podem ser escritos em tecnologias diferentes.

### Por que microsserviços NÃO se justificam neste MVP

A decisão de usar um monólito modular em vez de microsserviços foi consciente, baseada na análise de custos operacionais:

1. **Deploy e infraestrutura**: Cada microsserviço precisa de sua própria VM ou container, pipeline de CI/CD, monitoramento e escalabilidade independente. Para 2 módulos com baixo tráfego, isso multiplica custos de Azure e complexidade operacional sem benefício.

2. **Comunicação de rede**: Chamadas entre serviços via HTTP adicionam latência, exigem tratamento de falhas de rede (retry, circuit breaker, timeout), serialização/deserialização JSON e versionamento de contratos. Uma chamada in-process é instantânea e type-safe.

3. **Consistência de dados**: Com bancos separados em serviços diferentes, operações que envolvem ambos os módulos (ex: validar se uma categoria existe ao criar um produto) exigem padrões de consistência eventual (Saga, outbox pattern). No monólito modular, os dois DbContexts apontam para o mesmo banco, e a consistência é garantida pelo SQL Server.

4. **Observabilidade**: Rastrear uma requisição que cruza múltiplos serviços exige tracing distribuído (OpenTelemetry, Jaeger), correlation IDs e dashboards dedicados. No monólito, o Serilog com um único Application Insights já cobre tudo.

5. **Equipe**: Microsserviços fazem sentido quando equipes diferentes precisam deployar independentemente. Um MVP acadêmico com um desenvolvedor não tem essa necessidade.

### O que o monólito modular oferece

A separação interna simula as fronteiras de microsserviços sem os custos operacionais:

- **Módulos isolados**: `Catalog` e `Categories` são pastas autocontidas com entidades, DTOs, validadores, repositórios e endpoints próprios
- **DbContexts separados**: Cada módulo tem seu próprio DbContext com schema dedicado (`catalog.*`, `categories.*`) e tabela de migrations independente
- **Comunicação por contrato**: O módulo Catalog nunca acessa o DbContext de Categories. Ele usa a interface `ICategoryLookup` definida em `Shared`, cuja implementação é registrada na DI pelo módulo Categories
- **Registro modular**: Cada módulo expõe `AddXxxModule()` e `MapXxxEndpoints()`, tornando o `Program.cs` um orquestrador leve
- **Nenhuma query cross-module**: Não existem `Include()` ou JOINs cruzando a fronteira de módulos

### Como extrair o módulo Catalog para um serviço próprio

Se no futuro o volume justificar, estes são os passos concretos:

1. **Criar um novo projeto** `TechStore.Catalog.Api` com seu próprio `Program.cs`
2. **Mover a pasta** `Modules/Catalog` para o novo projeto (entidades, DTOs, repositório, endpoints, DbContext — tudo já é autocontido)
3. **Trocar a implementação de `ICategoryLookup`**: em vez de `CategoryLookupService` (que acessa o DbContext local), registrar uma nova implementação `CategoryLookupHttpClient` que faz chamada HTTP para o serviço de Categories
4. **Separar o banco** (opcional): Como o CatalogDbContext já usa o schema `catalog.*` e tabela de migrations própria, ele pode apontar para outro servidor SQL sem conflito
5. **Configurar DNS/Load Balancer** para rotear `/api/produtos` para o novo serviço
6. **Adicionar resiliência**: retry com Polly, circuit breaker, health checks apontando para o serviço de Categories

O ponto-chave é que **nenhum código de negócio precisa ser reescrito** — a mudança está na camada de infraestrutura (DI registration e implementação de contrato).

---

## Stack Tecnológica

| Camada | Tecnologia |
|--------|-----------|
| Frontend | HTML5, CSS3, JavaScript (vanilla) |
| API | .NET 8, Minimal API, Entity Framework Core |
| Validação | FluentValidation |
| Documentação | Swagger/OpenAPI (Swashbuckle) |
| Banco de dados | Azure SQL Database |
| Logging | Serilog (Console + Application Insights) |
| Monitoramento | Azure Monitor, Log Analytics, Application Insights |
| Hospedagem API | Azure VM (Ubuntu 22.04) + Nginx |
| Hospedagem Frontend | Azure Blob Storage (Static Website) |

## Estrutura de Módulos

```
TechStore.Api
├── Program.cs              ← host: DI, middlewares, pipeline
├── Middleware/              ← tratamento global de exceções
└── Modules/
    ├── Catalog/             ← módulo de produtos
    │   ├── CatalogModule.cs      (AddCatalogModule / MapCatalogEndpoints)
    │   ├── CatalogDbContext.cs    (schema: catalog)
    │   ├── Entities/Product.cs
    │   ├── Dtos/
    │   ├── Validators/
    │   ├── Endpoints/
    │   └── Repositories/
    └── Categories/          ← módulo de categorias
        ├── CategoriesModule.cs    (AddCategoriesModule / MapCategoriesEndpoints)
        ├── CategoriesDbContext.cs  (schema: categories)
        ├── Entities/Category.cs
        ├── Dtos/
        ├── Validators/
        ├── Endpoints/
        ├── Repositories/
        └── Services/
            └── CategoryLookupService.cs  ← implementa ICategoryLookup

TechStore.Shared
├── Contracts/ICategoryLookup.cs  ← interface de contrato inter-módulo
└── Models/
    ├── PagedResult.cs
    └── ValidationProblemFactory.cs
```

## Comunicação Inter-Módulo

```
Catalog Endpoint (POST /api/produtos)
  │
  ├─ FluentValidation (campos do DTO)
  │
  └─ ICategoryLookup.ExistsAsync(categoriaId)   ← contrato em Shared
       │
       └─ CategoryLookupService (in-process)     ← implementação em Categories
            │
            └─ CategoriesDbContext.Categories.AnyAsync(...)
```

A interface `ICategoryLookup` é o ponto de desacoplamento. Hoje ela é resolvida in-process. Para extrair em microsserviço, basta trocar o registro na DI por uma implementação HTTP — o código do Catalog não muda.
