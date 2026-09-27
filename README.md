# TechStore Cloud — Sistema de Cadastro de Produtos

Sistema de cadastro de produtos para loja de tecnologia, com API REST em .NET 8, frontend estático e infraestrutura na Microsoft Azure.

## Estrutura do Projeto

```
├── src/
│   ├── TechStore.Api/          → API REST (Minimal API, .NET 8)
│   │   └── Modules/
│   │       ├── Catalog/        → Módulo de produtos
│   │       └── Categories/     → Módulo de categorias
│   └── TechStore.Shared/       → Contratos e tipos compartilhados
├── frontend/                   → Site estático (HTML/CSS/JS puro)
├── tests/                      → Testes unitários (xUnit)
├── infra/                      → Scripts e configs de deploy
└── docs/                       → Documentação e diagramas
```

## Passo a Passo Completo

### 1. Rodar Localmente

**Pré-requisitos**: .NET 8 SDK instalado.

```bash
# Clonar e entrar no projeto
cd projeto4

# Restaurar dependências e compilar
dotnet build

# Rodar os testes
dotnet test

# Rodar a API (ela tentará conectar ao SQL Server local)
cd src/TechStore.Api
dotnet run
```

A API sobe na porta 5000. Acesse:
- Swagger: http://localhost:5000/swagger
- Health check: http://localhost:5000/health

> **Nota sobre o banco local**: Se você não tem SQL Server local, a API vai subir mas falhará ao conectar no banco. Para desenvolvimento local, crie um `appsettings.Development.json` (ele está no .gitignore) com sua connection string local.

**Frontend local**: Abra o `frontend/index.html` direto no navegador ou use uma extensão como Live Server no VS Code (porta 5500). O `config.js` já aponta para `http://localhost:5000`.

### 2. Criar Recursos na Azure

Siga os comandos em [`infra/azure-cli-commands.md`](infra/azure-cli-commands.md) na ordem:

1. Resource Group
2. Azure SQL Server + banco de dados
3. Virtual Machine (Ubuntu)
4. Storage Account com static website
5. Log Analytics + Application Insights
6. Regra de firewall do SQL para a VM

Anote os valores gerados — você vai precisar deles nos próximos passos.

### 3. Configurar a VM

```bash
# Conectar na VM
ssh azureuser@<IP-DA-VM>

# Copiar os scripts para a VM (de outra janela do terminal)
scp infra/deploy-vm.sh infra/techstore-api.service infra/nginx-techstore.conf azureuser@<IP-DA-VM>:/tmp/

# Na VM: rodar o script de setup
sudo bash /tmp/deploy-vm.sh
```

O script instala o .NET 8 Runtime, Nginx, configura o reverse proxy e o serviço systemd.

### 4. Publicar a API

```bash
# Na sua máquina local: gerar artefatos
chmod +x infra/publish.sh
./infra/publish.sh

# Copiar para a VM
scp -r artifacts/* azureuser@<IP-DA-VM>:/opt/techstore-api/

# Na VM: editar as variáveis de ambiente
sudo nano /etc/systemd/system/techstore-api.service
# Substitua os placeholders: connection string do SQL, Application Insights, AllowedOrigins

# Iniciar o serviço
sudo systemctl daemon-reload
sudo systemctl start techstore-api
sudo systemctl status techstore-api

# Verificar que está funcionando
curl http://localhost/health
```

### 5. Subir o Frontend

```bash
# Editar config.js com o IP da VM
# Trocar: const API_BASE_URL = 'http://localhost:5000';
# Para:   const API_BASE_URL = 'http://<IP-DA-VM>';

# Upload para o Blob Storage
az storage blob upload-batch \
  --account-name <SEU-STORAGE> \
  --destination '$web' \
  --source ./frontend \
  --overwrite
```

Acesse o site pelo endpoint estático: `https://<SEU-STORAGE>.z15.web.core.windows.net`

### 6. Validar o Monitoramento

1. Acesse o **Application Insights** no portal Azure
2. Faça algumas requisições no frontend ou Swagger
3. Vá em **Logs** e execute:

```kql
AppRequests
| where TimeGenerated > ago(1h)
| summarize count() by Name, ResultCode
| order by count_ desc
```

4. Verifique o **Live Metrics** para telemetria em tempo real
5. Confira os logs no **Log Analytics Workspace**

---

## Comandos de Migrations

Os dois módulos usam DbContexts separados com tabelas de migrations independentes:

```bash
cd src/TechStore.Api

# Categories
dotnet ef migrations add InitialCategories --context CategoriesDbContext --output-dir Modules/Categories/Migrations

# Catalog
dotnet ef migrations add InitialCatalog --context CatalogDbContext --output-dir Modules/Catalog/Migrations
```

> **Nota**: As migrations são aplicadas automaticamente no startup da API (`Database.Migrate()`). Em um cenário de produção real, isso deveria ser feito em um passo separado de CI/CD.

## Testes

```bash
dotnet test --verbosity normal
```

31 testes cobrindo:
- Validações de produtos e categorias (FluentValidation)
- Operações CRUD nos repositórios
- Soft delete (produto e categoria)
- Paginação e filtros
- Checagem de categoria inexistente via ICategoryLookup

## Documentação

- [`docs/arquitetura.md`](docs/arquitetura.md) — decisões técnicas e justificativa do monólito modular
- [`docs/diagrama.md`](docs/diagrama.md) — diagramas Mermaid da arquitetura
- [`docs/roteiro-evidencias.md`](docs/roteiro-evidencias.md) — checklist de prints para o PDF
