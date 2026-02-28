# Script para Configurar Secrets no Azure Key Vault
# Para Filas de Mensagens (Azure Service Bus e RabbitMQ)

param(
    [Parameter(Mandatory=$false)]
    [string]$KeyVaultName = "talhaosensorvault",
    
    [Parameter(Mandatory=$false)]
    [string]$Provider = "RabbitMQ", # ou "AzureServiceBus"
    
    [Parameter(Mandatory=$true)]
    [string]$ConnectionString
)

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "Configurando Secrets no Azure Key Vault" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""

# Determinar o nome do secret baseado no provider
$secretName = ""
if ($Provider -eq "AzureServiceBus" -or $Provider -eq "Azure") {
    $secretName = "ConnectionStrings--AzureServiceBus"
    Write-Host "Provider: Azure Service Bus" -ForegroundColor Green
}
elseif ($Provider -eq "RabbitMQ" -or $Provider -eq "Rabbit") {
    $secretName = "ConnectionStrings--RabbitMQ"
    Write-Host "Provider: RabbitMQ" -ForegroundColor Green
}
else {
    Write-Host "Provider '$Provider' não suportado. Use 'AzureServiceBus' ou 'RabbitMQ'." -ForegroundColor Red
    exit 1
}

Write-Host "Key Vault: $KeyVaultName" -ForegroundColor Yellow
Write-Host "Nome do Secret: $secretName" -ForegroundColor Yellow
Write-Host ""

# Verificar se está logado no Azure
Write-Host "Verificando autenticação no Azure..." -ForegroundColor Cyan
$account = az account show 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Você não está autenticado no Azure. Executando 'az login'..." -ForegroundColor Yellow
    az login
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Falha ao autenticar no Azure." -ForegroundColor Red
        exit 1
    }
}
Write-Host "? Autenticado no Azure" -ForegroundColor Green
Write-Host ""

# Verificar se o Key Vault existe
Write-Host "Verificando se o Key Vault '$KeyVaultName' existe..." -ForegroundColor Cyan
$kvExists = az keyvault show --name $KeyVaultName 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Key Vault '$KeyVaultName' não encontrado." -ForegroundColor Red
    Write-Host "Certifique-se de que o Key Vault existe e você tem permissões." -ForegroundColor Red
    exit 1
}
Write-Host "? Key Vault encontrado" -ForegroundColor Green
Write-Host ""

# Criar ou atualizar o secret
Write-Host "Criando/atualizando secret '$secretName'..." -ForegroundColor Cyan
$secureString = $ConnectionString
$result = az keyvault secret set `
    --vault-name $KeyVaultName `
    --name $secretName `
    --value $secureString `
    --output json 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "? Secret criado/atualizado com sucesso!" -ForegroundColor Green
    Write-Host ""
    
    # Mostrar informações do secret (sem revelar o valor)
    Write-Host "Detalhes do Secret:" -ForegroundColor Cyan
    Write-Host "  Nome: $secretName" -ForegroundColor White
    Write-Host "  Key Vault: $KeyVaultName" -ForegroundColor White
    Write-Host ""
    
    # Verificar o secret
    Write-Host "Verificando secret..." -ForegroundColor Cyan
    $secret = az keyvault secret show --vault-name $KeyVaultName --name $secretName --output json | ConvertFrom-Json
    Write-Host "  ID: $($secret.id)" -ForegroundColor White
    Write-Host "  Habilitado: $($secret.attributes.enabled)" -ForegroundColor White
    Write-Host "  Criado em: $($secret.attributes.created)" -ForegroundColor White
    Write-Host ""
    
    Write-Host "===============================================" -ForegroundColor Green
    Write-Host "Configuração concluída com sucesso!" -ForegroundColor Green
    Write-Host "===============================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Próximos passos:" -ForegroundColor Cyan
    Write-Host "1. Configure o appsettings.json com:" -ForegroundColor White
    Write-Host "   {" -ForegroundColor Gray
    Write-Host "     ""KeyVault"": {" -ForegroundColor Gray
    Write-Host "       ""Url"": ""https://$KeyVaultName.vault.azure.net/""" -ForegroundColor Gray
    Write-Host "     }," -ForegroundColor Gray
    Write-Host "     ""MessageQueue"": {" -ForegroundColor Gray
    Write-Host "       ""Provider"": ""$Provider""" -ForegroundColor Gray
    Write-Host "     }" -ForegroundColor Gray
    Write-Host "   }" -ForegroundColor Gray
    Write-Host ""
    Write-Host "2. Certifique-se de que a aplicação tem permissões no Key Vault" -ForegroundColor White
    Write-Host "3. Execute a aplicação" -ForegroundColor White
}
else {
    Write-Host "? Erro ao criar/atualizar secret" -ForegroundColor Red
    Write-Host $result -ForegroundColor Red
    exit 1
}
