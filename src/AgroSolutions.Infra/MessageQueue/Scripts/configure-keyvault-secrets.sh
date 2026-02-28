#!/bin/bash

# Script para Configurar Secrets no Azure Key Vault
# Para Filas de Mensagens (Azure Service Bus e RabbitMQ)

set -e

# Cores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Parâmetros
KEY_VAULT_NAME="${1:-talhaosensorvault}"
PROVIDER="${2:-RabbitMQ}"
CONNECTION_STRING="${3}"

if [ -z "$CONNECTION_STRING" ]; then
    echo -e "${RED}Erro: Connection string não fornecida${NC}"
    echo ""
    echo "Uso: $0 [KeyVaultName] [Provider] [ConnectionString]"
    echo ""
    echo "Exemplos:"
    echo "  $0 talhaosensorvault RabbitMQ 'amqp://guest:guest@localhost:5672/'"
    echo "  $0 talhaosensorvault AzureServiceBus 'Endpoint=sb://...'"
    exit 1
fi

echo -e "${CYAN}===============================================${NC}"
echo -e "${CYAN}Configurando Secrets no Azure Key Vault${NC}"
echo -e "${CYAN}===============================================${NC}"
echo ""

# Determinar o nome do secret baseado no provider
SECRET_NAME=""
if [ "$PROVIDER" = "AzureServiceBus" ] || [ "$PROVIDER" = "Azure" ]; then
    SECRET_NAME="ConnectionStrings--AzureServiceBus"
    echo -e "${GREEN}Provider: Azure Service Bus${NC}"
elif [ "$PROVIDER" = "RabbitMQ" ] || [ "$PROVIDER" = "Rabbit" ]; then
    SECRET_NAME="ConnectionStrings--RabbitMQ"
    echo -e "${GREEN}Provider: RabbitMQ${NC}"
else
    echo -e "${RED}Provider '$PROVIDER' não suportado. Use 'AzureServiceBus' ou 'RabbitMQ'.${NC}"
    exit 1
fi

echo -e "${YELLOW}Key Vault: $KEY_VAULT_NAME${NC}"
echo -e "${YELLOW}Nome do Secret: $SECRET_NAME${NC}"
echo ""

# Verificar se está logado no Azure
echo -e "${CYAN}Verificando autenticação no Azure...${NC}"
if ! az account show &> /dev/null; then
    echo -e "${YELLOW}Você não está autenticado no Azure. Executando 'az login'...${NC}"
    az login
fi
echo -e "${GREEN}? Autenticado no Azure${NC}"
echo ""

# Verificar se o Key Vault existe
echo -e "${CYAN}Verificando se o Key Vault '$KEY_VAULT_NAME' existe...${NC}"
if ! az keyvault show --name "$KEY_VAULT_NAME" &> /dev/null; then
    echo -e "${RED}Key Vault '$KEY_VAULT_NAME' não encontrado.${NC}"
    echo -e "${RED}Certifique-se de que o Key Vault existe e você tem permissões.${NC}"
    exit 1
fi
echo -e "${GREEN}? Key Vault encontrado${NC}"
echo ""

# Criar ou atualizar o secret
echo -e "${CYAN}Criando/atualizando secret '$SECRET_NAME'...${NC}"
if az keyvault secret set \
    --vault-name "$KEY_VAULT_NAME" \
    --name "$SECRET_NAME" \
    --value "$CONNECTION_STRING" \
    --output none; then
    
    echo -e "${GREEN}? Secret criado/atualizado com sucesso!${NC}"
    echo ""
    
    # Mostrar informações do secret (sem revelar o valor)
    echo -e "${CYAN}Detalhes do Secret:${NC}"
    echo "  Nome: $SECRET_NAME"
    echo "  Key Vault: $KEY_VAULT_NAME"
    echo ""
    
    # Verificar o secret
    echo -e "${CYAN}Verificando secret...${NC}"
    SECRET_INFO=$(az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name "$SECRET_NAME" --output json)
    SECRET_ID=$(echo "$SECRET_INFO" | jq -r '.id')
    SECRET_ENABLED=$(echo "$SECRET_INFO" | jq -r '.attributes.enabled')
    SECRET_CREATED=$(echo "$SECRET_INFO" | jq -r '.attributes.created')
    
    echo "  ID: $SECRET_ID"
    echo "  Habilitado: $SECRET_ENABLED"
    echo "  Criado em: $SECRET_CREATED"
    echo ""
    
    echo -e "${GREEN}===============================================${NC}"
    echo -e "${GREEN}Configuração concluída com sucesso!${NC}"
    echo -e "${GREEN}===============================================${NC}"
    echo ""
    echo -e "${CYAN}Próximos passos:${NC}"
    echo "1. Configure o appsettings.json com:"
    echo "   {"
    echo "     \"KeyVault\": {"
    echo "       \"Url\": \"https://$KEY_VAULT_NAME.vault.azure.net/\""
    echo "     },"
    echo "     \"MessageQueue\": {"
    echo "       \"Provider\": \"$PROVIDER\""
    echo "     }"
    echo "   }"
    echo ""
    echo "2. Certifique-se de que a aplicação tem permissões no Key Vault"
    echo "3. Execute a aplicação"
else
    echo -e "${RED}? Erro ao criar/atualizar secret${NC}"
    exit 1
fi
