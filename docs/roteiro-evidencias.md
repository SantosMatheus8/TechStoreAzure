# Roteiro de Evidências — TechStore Cloud

Lista dos prints que devem ser capturados para o PDF de evidências do trabalho.

## Recursos no Portal Azure

1. **Resource Group** — visão geral mostrando todos os recursos criados
2. **Azure SQL Server** — painel principal com nome do servidor e localização
3. **Azure SQL Database** — painel do banco `techstoredb` mostrando tier e status
4. **Regras de firewall do SQL** — tela de Networking mostrando as regras criadas
5. **Virtual Machine** — painel principal com IP público, tamanho e status "Running"
6. **NSG (Network Security Group)** — regras de entrada mostrando portas 22 e 80 abertas
7. **Storage Account** — painel principal
8. **Static Website** — configuração mostrando endpoint primário e `index.html` configurado
9. **Log Analytics Workspace** — painel principal
10. **Application Insights** — painel principal com connection string (parcialmente visível)

## API Funcionando

11. **Health check** — resposta do `http://<IP-VM>/health` no navegador mostrando status Healthy
12. **Swagger UI** — página do Swagger com os endpoints agrupados por tag (Produtos e Categorias)
13. **Swagger - GET /api/categorias** — execução mostrando as 4 categorias de seed
14. **Swagger - GET /api/produtos** — execução mostrando os 10 produtos de seed
15. **Swagger - POST /api/produtos** — criação de um produto novo (request + response 201)
16. **Swagger - POST com categoria inválida** — response 422 com mensagem de erro
17. **Swagger - DELETE /api/produtos/{id}** — soft delete retornando 204

## Frontend Funcionando

18. **Listagem de produtos** — tela principal com produtos carregados, mostrando nome, categoria, preço e status
19. **Busca por nome** — campo de busca com filtro ativo e resultado parcial
20. **Filtro por categoria** — select de categoria aplicado com produtos filtrados
21. **Formulário de criação** — tela de novo produto com campos preenchidos
22. **Formulário de edição** — tela de edição com dados carregados
23. **Modal de exclusão** — modal de confirmação aberto
24. **Tela de categorias** — listagem de categorias com formulário de criação

## Monitoramento

25. **Application Insights - Visão geral** — dashboard com métricas de requisições
26. **Application Insights - Live Metrics** — telemetria em tempo real (se disponível)
27. **Log Analytics - Query** — execução de uma query KQL mostrando logs da API, por exemplo:
    ```kql
    AppRequests
    | where TimeGenerated > ago(1h)
    | summarize count() by Name, ResultCode
    | order by count_ desc
    ```
28. **Log Analytics - Resultado** — tabela de resultados da query acima

## Terminal / Desenvolvimento

29. **Testes passando** — saída do `dotnet test` mostrando todos os 31 testes aprovados
30. **Build com sucesso** — saída do `dotnet build` sem erros
31. **Serviço systemd ativo** — saída do `systemctl status techstore-api` na VM

---

**Total: 31 evidências**

Dica: organize o PDF na mesma ordem desta lista, com um título descritivo acima de cada print.
