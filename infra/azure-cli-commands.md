# Comandos Azure CLI — TechStore Cloud

Execute estes comandos na ordem, substituindo os valores entre `< >`.

## Variáveis (defina antes de executar)

```bash
RESOURCE_GROUP="rg-techstore"
LOCATION="brazilsouth"
SQL_SERVER_NAME="<SEU-SQL-SERVER>"        # deve ser globalmente único
SQL_ADMIN_USER="<SEU-ADMIN>"
SQL_ADMIN_PASSWORD="<SUA-SENHA-FORTE>"    # mínimo 8 chars, maiúscula, minúscula, número
SQL_DB_NAME="techstoredb"
VM_NAME="vm-techstore"
VM_ADMIN_USER="azureuser"
STORAGE_ACCOUNT="<SEU-STORAGE>"           # deve ser globalmente único, só minúsculas e números
LOG_WORKSPACE="law-techstore"
APP_INSIGHTS="ai-techstore"
```

## 1. Resource Group

```bash
az group create \
  --name $RESOURCE_GROUP \
  --location $LOCATION
```

## 2. Azure SQL Server + Banco de Dados

```bash
# Criar SQL Server
az sql server create \
  --resource-group $RESOURCE_GROUP \
  --name $SQL_SERVER_NAME \
  --admin-user $SQL_ADMIN_USER \
  --admin-password $SQL_ADMIN_PASSWORD \
  --location $LOCATION

# Criar banco de dados (tier Basic para MVP)
az sql db create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name $SQL_DB_NAME \
  --edition Basic \
  --capacity 5

# Regra de firewall: permitir serviços Azure
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Regra de firewall: permitir seu IP local (para testar migrations)
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name AllowMyIP \
  --start-ip-address <SEU-IP-PUBLICO> \
  --end-ip-address <SEU-IP-PUBLICO>
```

## 3. Virtual Machine (Ubuntu)

```bash
# Criar VM
az vm create \
  --resource-group $RESOURCE_GROUP \
  --name $VM_NAME \
  --image Ubuntu2204 \
  --size Standard_B1s \
  --admin-username $VM_ADMIN_USER \
  --generate-ssh-keys \
  --public-ip-sku Standard \
  --location $LOCATION

# Abrir porta 80 (HTTP) e 22 (SSH)
az vm open-port \
  --resource-group $RESOURCE_GROUP \
  --name $VM_NAME \
  --port 80 \
  --priority 1001

az vm open-port \
  --resource-group $RESOURCE_GROUP \
  --name $VM_NAME \
  --port 22 \
  --priority 1002

# Obter IP público da VM
az vm show \
  --resource-group $RESOURCE_GROUP \
  --name $VM_NAME \
  --show-details \
  --query publicIps \
  --output tsv
```

## 4. Storage Account (Frontend estático)

```bash
# Criar Storage Account
az storage account create \
  --resource-group $RESOURCE_GROUP \
  --name $STORAGE_ACCOUNT \
  --location $LOCATION \
  --sku Standard_LRS \
  --kind StorageV2

# Habilitar site estático
az storage blob service-properties update \
  --account-name $STORAGE_ACCOUNT \
  --static-website \
  --index-document index.html \
  --404-document index.html

# Obter URL do site estático
az storage account show \
  --resource-group $RESOURCE_GROUP \
  --name $STORAGE_ACCOUNT \
  --query primaryEndpoints.web \
  --output tsv

# Upload do frontend (execute da pasta /frontend)
az storage blob upload-batch \
  --account-name $STORAGE_ACCOUNT \
  --destination '$web' \
  --source ./frontend
```

## 5. Log Analytics + Application Insights

```bash
# Criar Log Analytics Workspace
az monitor log-analytics workspace create \
  --resource-group $RESOURCE_GROUP \
  --workspace-name $LOG_WORKSPACE \
  --location $LOCATION

# Obter ID do workspace
WORKSPACE_ID=$(az monitor log-analytics workspace show \
  --resource-group $RESOURCE_GROUP \
  --workspace-name $LOG_WORKSPACE \
  --query id \
  --output tsv)

# Criar Application Insights
az monitor app-insights component create \
  --resource-group $RESOURCE_GROUP \
  --app $APP_INSIGHTS \
  --location $LOCATION \
  --workspace $WORKSPACE_ID \
  --kind web \
  --application-type web

# Obter connection string do Application Insights
az monitor app-insights component show \
  --resource-group $RESOURCE_GROUP \
  --app $APP_INSIGHTS \
  --query connectionString \
  --output tsv
```

## 6. Firewall do SQL para a VM

```bash
# Obter IP público da VM
VM_IP=$(az vm show --resource-group $RESOURCE_GROUP --name $VM_NAME --show-details --query publicIps --output tsv)

# Adicionar regra de firewall para a VM
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name AllowVM \
  --start-ip-address $VM_IP \
  --end-ip-address $VM_IP
```

## Resumo dos valores que você vai precisar

| O quê | Onde usar |
|-------|-----------|
| Connection string do SQL | `techstore-api.service` → variável `ConnectionStrings__DefaultConnection` |
| Connection string do App Insights | `techstore-api.service` → variável `ApplicationInsights__ConnectionString` |
| URL do site estático | `techstore-api.service` → variável `AllowedOrigins` |
| IP público da VM | `frontend/config.js` → `API_BASE_URL` (ex: `http://<IP-DA-VM>`) |
