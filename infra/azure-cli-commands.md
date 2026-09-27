# Comandos Azure CLI — TechStore Cloud

Documentação dos recursos criados na infraestrutura Azure.

## Variáveis utilizadas

```bash
RESOURCE_GROUP="rg-techstore-mvp"
LOCATION="spaincentral"
SQL_SERVER_NAME="sql-techstore-matheus"
SQL_ADMIN_USER="<SQL_ADMIN_USER>"
SQL_DB_NAME="techstoredb"
VM_NAME="vm-techstore"
VM_ADMIN_USER="azureuser"
STORAGE_ACCOUNT="sttechstore31459"
LOG_WORKSPACE="law-techstore"
APP_INSIGHTS="ai-techstore"
KEY_VAULT="kv-techstore-matheus"
```

> **Nota:** Assinatura Azure for Students — política de regiões permitidas:
> francecentral, canadacentral, belgiumcentral, spaincentral, italynorth.

## 1. Resource Group

```bash
az group create \
  --name $RESOURCE_GROUP \
  --location $LOCATION
```

## 2. Azure SQL Server + Banco de Dados

```bash
# Criar SQL Server (tier Basic em spaincentral)
az sql server create \
  --resource-group $RESOURCE_GROUP \
  --name $SQL_SERVER_NAME \
  --admin-user $SQL_ADMIN_USER \
  --admin-password "<SENHA>" \
  --location $LOCATION

# Criar banco de dados (Basic, 5 DTUs)
az sql db create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name $SQL_DB_NAME \
  --edition Basic \
  --capacity 5

# Regra de firewall: IP da VM
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name AllowVM \
  --start-ip-address <VM_PUBLIC_IP> \
  --end-ip-address <VM_PUBLIC_IP>

# Regra de firewall: IP do admin (para migrations e testes)
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name AllowAdminIP \
  --start-ip-address <ADMIN_IP> \
  --end-ip-address <ADMIN_IP>
```

> **Sem** regra AllowAzureServices (0.0.0.0) — apenas IPs explícitos.

## 3. Virtual Machine (Ubuntu 22.04)

```bash
# VM Standard_B2ats_v2 (B1s não disponível na assinatura Azure for Students)
az vm create \
  --resource-group $RESOURCE_GROUP \
  --name $VM_NAME \
  --image Ubuntu2204 \
  --size Standard_B2ats_v2 \
  --admin-username $VM_ADMIN_USER \
  --generate-ssh-keys \
  --public-ip-sku Standard \
  --location $LOCATION

# Habilitar identidade gerenciada (system-assigned)
az vm identity assign \
  --resource-group $RESOURCE_GROUP \
  --name $VM_NAME
```

### NSG (Network Security Group)

- Porta 80 (HTTP) — aberta
- Porta 443 (HTTPS) — aberta
- Porta 22 (SSH) — restrita ao IP do admin

```bash
# Abrir portas 80 e 443
az vm open-port --resource-group $RESOURCE_GROUP --name $VM_NAME --port 80 --priority 1001
az vm open-port --resource-group $RESOURCE_GROUP --name $VM_NAME --port 443 --priority 1002

# SSH restrito ao IP do admin
az network nsg rule update \
  --resource-group $RESOURCE_GROUP \
  --nsg-name ${VM_NAME}NSG \
  --name default-allow-ssh \
  --source-address-prefixes <ADMIN_IP>
```

### DNS público

```bash
az network public-ip update \
  --resource-group $RESOURCE_GROUP \
  --name ${VM_NAME}PublicIP \
  --dns-name techstore-matheus
```

DNS: `techstore-matheus.spaincentral.cloudapp.azure.com`

## 4. Storage Account (Frontend estático)

```bash
# Criar Storage Account (em canadacentral)
az storage account create \
  --resource-group $RESOURCE_GROUP \
  --name $STORAGE_ACCOUNT \
  --location canadacentral \
  --sku Standard_LRS \
  --kind StorageV2

# Habilitar site estático
az storage blob service-properties update \
  --account-name $STORAGE_ACCOUNT \
  --static-website \
  --index-document index.html \
  --404-document index.html

# Upload do frontend
az storage blob upload-batch \
  --account-name $STORAGE_ACCOUNT \
  --destination '$web' \
  --source ./frontend
```

URL do frontend: `https://sttechstore31459.z9.web.core.windows.net`

## 5. Log Analytics + Application Insights

```bash
# Log Analytics Workspace (em canadacentral)
az monitor log-analytics workspace create \
  --resource-group $RESOURCE_GROUP \
  --workspace-name $LOG_WORKSPACE \
  --location canadacentral

WORKSPACE_ID=$(az monitor log-analytics workspace show \
  --resource-group $RESOURCE_GROUP \
  --workspace-name $LOG_WORKSPACE \
  --query id --output tsv)

# Application Insights (em canadacentral)
az monitor app-insights component create \
  --resource-group $RESOURCE_GROUP \
  --app $APP_INSIGHTS \
  --location canadacentral \
  --workspace $WORKSPACE_ID \
  --kind web \
  --application-type web
```

## 6. Key Vault (com RBAC)

```bash
# Criar Key Vault
az keyvault create \
  --resource-group $RESOURCE_GROUP \
  --name $KEY_VAULT \
  --location $LOCATION \
  --enable-rbac-authorization true

# Atribuir papel "Key Vault Secrets User" à identidade gerenciada da VM
VM_PRINCIPAL_ID=$(az vm identity show \
  --resource-group $RESOURCE_GROUP \
  --name $VM_NAME \
  --query principalId --output tsv)

KV_RESOURCE_ID=$(az keyvault show \
  --resource-group $RESOURCE_GROUP \
  --name $KEY_VAULT \
  --query id --output tsv)

az role assignment create \
  --role "Key Vault Secrets User" \
  --assignee-object-id $VM_PRINCIPAL_ID \
  --assignee-principal-type ServicePrincipal \
  --scope $KV_RESOURCE_ID

# Criar segredos (execute com um usuário que tenha papel "Key Vault Secrets Officer")
az keyvault secret set --vault-name $KEY_VAULT \
  --name "ConnectionStrings--DefaultConnection" \
  --value "<CONNECTION_STRING>"

az keyvault secret set --vault-name $KEY_VAULT \
  --name "ApplicationInsights--ConnectionString" \
  --value "<APP_INSIGHTS_CONNECTION_STRING>"
```

Segredos no Key Vault:
- `ConnectionStrings--DefaultConnection` → o provider converte `--` em `:` automaticamente
- `ApplicationInsights--ConnectionString`

## Resumo dos recursos

| Recurso | Nome | Região |
|---------|------|--------|
| Resource Group | rg-techstore-mvp | spaincentral |
| SQL Server | sql-techstore-matheus | spaincentral |
| SQL Database | techstoredb | spaincentral |
| VM | vm-techstore (Standard_B2ats_v2) | spaincentral |
| Storage Account | sttechstore31459 | canadacentral |
| Log Analytics | law-techstore | canadacentral |
| Application Insights | ai-techstore | canadacentral |
| Key Vault | kv-techstore-matheus | spaincentral |

| DNS / URL | Valor |
|-----------|-------|
| API (VM) | techstore-matheus.spaincentral.cloudapp.azure.com |
| Frontend | https://sttechstore31459.z9.web.core.windows.net |
| Key Vault | https://kv-techstore-matheus.vault.azure.net/ |
