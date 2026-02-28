# Scripts de Configuração do Azure Key Vault

Este diretório contém scripts para facilitar a configuração dos secrets no Azure Key Vault.

## Scripts Disponíveis

### PowerShell (Windows)

**Arquivo**: `ConfigureKeyVaultSecrets.ps1`

**Uso**:
```powershell
.\ConfigureKeyVaultSecrets.ps1 -Provider "RabbitMQ" -ConnectionString "amqp://guest:guest@localhost:5672/"
```

**Parâmetros**:
- `-KeyVaultName`: (Opcional) Nome do Key Vault. Padrão: `talhaosensorvault`
- `-Provider`: (Opcional) Provider a configurar (`RabbitMQ` ou `AzureServiceBus`). Padrão: `RabbitMQ`
- `-ConnectionString`: (Obrigatório) A connection string a ser armazenada

**Exemplos**:

```powershell
# Configurar RabbitMQ local
.\ConfigureKeyVaultSecrets.ps1 `
  -Provider "RabbitMQ" `
  -ConnectionString "amqp://guest:guest@localhost:5672/"

# Configurar RabbitMQ em produção
.\ConfigureKeyVaultSecrets.ps1 `
  -Provider "RabbitMQ" `
  -ConnectionString "amqp://user:password@rabbitmq.production.com:5672/prod"

# Configurar Azure Service Bus
.\ConfigureKeyVaultSecrets.ps1 `
  -Provider "AzureServiceBus" `
  -ConnectionString "Endpoint=sb://mynamespace.servicebus.windows.net/;SharedAccessKeyName=SendPolicy;SharedAccessKey=abc123..."

# Configurar em outro Key Vault
.\ConfigureKeyVaultSecrets.ps1 `
  -KeyVaultName "outro-keyvault" `
  -Provider "RabbitMQ" `
  -ConnectionString "amqp://user:pass@host:5672/"
```

### Bash (Linux/Mac)

**Arquivo**: `configure-keyvault-secrets.sh`

**Uso**:
```bash
chmod +x configure-keyvault-secrets.sh
./configure-keyvault-secrets.sh [KeyVaultName] [Provider] [ConnectionString]
```

**Parâmetros**:
1. `KeyVaultName`: (Opcional) Nome do Key Vault. Padrão: `talhaosensorvault`
2. `Provider`: (Opcional) Provider a configurar (`RabbitMQ` ou `AzureServiceBus`). Padrão: `RabbitMQ`
3. `ConnectionString`: (Obrigatório) A connection string a ser armazenada

**Exemplos**:

```bash
# Configurar RabbitMQ local
./configure-keyvault-secrets.sh talhaosensorvault RabbitMQ "amqp://guest:guest@localhost:5672/"

# Configurar Azure Service Bus
./configure-keyvault-secrets.sh talhaosensorvault AzureServiceBus "Endpoint=sb://mynamespace.servicebus.windows.net/;SharedAccessKeyName=SendPolicy;SharedAccessKey=abc123..."

# Configurar com valores padrões (exceto ConnectionString)
./configure-keyvault-secrets.sh "" "" "amqp://guest:guest@localhost:5672/"
```

## Pré-requisitos

### Para Ambos os Scripts

1. **Azure CLI** instalada e configurada
   - Windows: https://aka.ms/installazurecliwindows
   - Linux/Mac: https://docs.microsoft.com/cli/azure/install-azure-cli

2. **Autenticação no Azure**
   ```bash
   az login
   ```

3. **Permissões no Key Vault**
   - Você precisa ter permissões para criar/atualizar secrets
   - Role necessária: `Key Vault Secrets Officer` (RBAC) ou permissões de `Set` nas Access Policies

### Adicional para Bash (Linux/Mac)

4. **jq** instalado (para parsing de JSON)
   ```bash
   # Ubuntu/Debian
   sudo apt-get install jq
   
   # Mac
   brew install jq
   ```

## O que os Scripts Fazem

1. ? Verificam se você está autenticado no Azure
2. ? Verificam se o Key Vault existe
3. ? Criam ou atualizam o secret com o nome correto:
   - `ConnectionStrings--AzureServiceBus` para Azure Service Bus
   - `ConnectionStrings--RabbitMQ` para RabbitMQ
4. ? Exibem informações sobre o secret criado
5. ? Fornecem instruções sobre os próximos passos

## Secrets Criados

Os scripts criam secrets com a seguinte nomenclatura no Key Vault:

| Provider | Nome do Secret |
|----------|----------------|
| RabbitMQ | `ConnectionStrings--RabbitMQ` |
| Azure Service Bus | `ConnectionStrings--AzureServiceBus` |

**Nota**: O `--` (dois hífens) é importante! Ele é automaticamente convertido para `:` quando a aplicação lê do Key Vault via `IConfiguration`.

## Configuração Manual (Sem Scripts)

Se preferir configurar manualmente:

### Via Azure CLI

```bash
# RabbitMQ
az keyvault secret set \
  --vault-name talhaosensorvault \
  --name "ConnectionStrings--RabbitMQ" \
  --value "amqp://user:password@host:5672/"

# Azure Service Bus
az keyvault secret set \
  --vault-name talhaosensorvault \
  --name "ConnectionStrings--AzureServiceBus" \
  --value "Endpoint=sb://namespace.servicebus.windows.net/;SharedAccessKeyName=SendPolicy;SharedAccessKey=key"
```

### Via Azure Portal

1. Acesse https://portal.azure.com
2. Navegue até o Key Vault `talhaosensorvault`
3. Clique em **Secrets** no menu lateral
4. Clique em **+ Generate/Import**
5. Preencha:
   - **Name**: `ConnectionStrings--RabbitMQ` (ou `ConnectionStrings--AzureServiceBus`)
   - **Value**: Sua connection string
6. Clique em **Create**

## Verificação

Para verificar se o secret foi criado corretamente:

```bash
# Listar todos os secrets
az keyvault secret list --vault-name talhaosensorvault --output table

# Ver um secret específico (sem mostrar o valor)
az keyvault secret show --vault-name talhaosensorvault --name "ConnectionStrings--RabbitMQ"

# Ver o valor do secret (cuidado ao compartilhar)
az keyvault secret show --vault-name talhaosensorvault --name "ConnectionStrings--RabbitMQ" --query value -o tsv
```

## Troubleshooting

### Erro: "az: command not found"

**Solução**: Instale a Azure CLI
- Windows: https://aka.ms/installazurecliwindows
- Linux/Mac: https://docs.microsoft.com/cli/azure/install-azure-cli

### Erro: "Please run 'az login' to setup account"

**Solução**: Faça login no Azure
```bash
az login
```

### Erro: "The user, group or application '...' does not have secrets set permission"

**Solução**: Você precisa de permissões no Key Vault

**Opção 1 - RBAC (Recomendado)**:
```bash
# Substitua [YOUR_EMAIL] pelo seu email
az role assignment create \
  --role "Key Vault Secrets Officer" \
  --assignee [YOUR_EMAIL] \
  --scope /subscriptions/[SUBSCRIPTION_ID]/resourceGroups/[RG_NAME]/providers/Microsoft.KeyVault/vaults/talhaosensorvault
```

**Opção 2 - Access Policies**:
1. Acesse o Key Vault no portal
2. Vá em **Access policies**
3. Clique em **+ Add Access Policy**
4. Selecione permissões de Secret: **Get**, **List**, **Set**
5. Selecione seu usuário
6. Clique em **Add** e depois **Save**

### Erro: "jq: command not found" (apenas Bash)

**Solução**: Instale o jq
```bash
# Ubuntu/Debian
sudo apt-get install jq

# Mac
brew install jq
```

## Suporte

Para mais informações, consulte:
- [Documentação do Azure Key Vault](https://learn.microsoft.com/en-us/azure/key-vault/)
- [Azure CLI Reference](https://learn.microsoft.com/en-us/cli/azure/keyvault)
- [README principal](../README.md)
