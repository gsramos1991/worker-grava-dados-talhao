# Configuração de Fila de Mensagens

Este projeto suporta dois provedores de fila de mensagens:
- **Azure Service Bus** - Serviço de mensageria gerenciado da Azure
- **RabbitMQ** - Broker de mensagens open-source

## ?? Configuração

### 1. Escolha do Provider

No arquivo `appsettings.json`, configure o provider desejado:

```json
{
  "KeyVault": {
    "Url": "https://talhaosensorvault.vault.azure.net/"
  },
  "MessageQueue": {
    "Provider": "RabbitMQ"
  }
}
```

**Valores aceitos para Provider:**
- `"AzureServiceBus"` ou `"Azure"` - Para usar Azure Service Bus
- `"RabbitMQ"` ou `"Rabbit"` - Para usar RabbitMQ

?? **IMPORTANTE**: Mantenha apenas o endpoint do Key Vault no `appsettings.json`. As connection strings devem estar no Azure Key Vault.

### 2. Connection Strings no Azure Key Vault

As connection strings **NÃO** devem estar no `appsettings.json`. Elas devem ser armazenadas com segurança no **Azure Key Vault**.

#### Secrets Necessários no Azure Key Vault

| Provider | Nome do Secret no Key Vault | Exemplo de Valor |
|----------|----------------------------|------------------|
| Azure Service Bus | `ConnectionStrings--AzureServiceBus` | `Endpoint=sb://namespace.servicebus.windows.net/;SharedAccessKeyName=SendPolicy;SharedAccessKey=abc123...` |
| RabbitMQ | `ConnectionStrings--RabbitMQ` | `amqp://username:password@hostname:5672/` |

**Nota**: O `--` (dois hífens) no nome do secret é convertido automaticamente para `:` pelo Azure Key Vault Configuration Provider.

#### Como Criar Secrets no Azure Key Vault

##### Opção 1: Via Azure Portal

1. Acesse o [Azure Portal](https://portal.azure.com)
2. Navegue até o seu Key Vault: `talhaosensorvault`
3. No menu lateral, clique em **"Secrets"**
4. Clique em **"+ Generate/Import"**
5. Preencha os campos:
   - **Upload options**: Manual
   - **Name**: `ConnectionStrings--AzureServiceBus` (ou `ConnectionStrings--RabbitMQ`)
   - **Value**: Cole a connection string completa
   - **Content type**: (opcional) `text/plain`
   - **Enabled**: Yes
6. Clique em **"Create"**

##### Opção 2: Via Azure CLI

```bash
# Para Azure Service Bus
az keyvault secret set \
  --vault-name talhaosensorvault \
  --name "ConnectionStrings--AzureServiceBus" \
  --value "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=SendPolicy;SharedAccessKey=your-key-here"

# Para RabbitMQ
az keyvault secret set \
  --vault-name talhaosensorvault \
  --name "ConnectionStrings--RabbitMQ" \
  --value "amqp://username:password@hostname:5672/"
```

##### Opção 3: Via PowerShell

```powershell
# Para Azure Service Bus
$secret = ConvertTo-SecureString "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=SendPolicy;SharedAccessKey=your-key" -AsPlainText -Force
Set-AzKeyVaultSecret -VaultName "talhaosensorvault" -Name "ConnectionStrings--AzureServiceBus" -SecretValue $secret

# Para RabbitMQ
$secret = ConvertTo-SecureString "amqp://username:password@hostname:5672/" -AsPlainText -Force
Set-AzKeyVaultSecret -VaultName "talhaosensorvault" -Name "ConnectionStrings--RabbitMQ" -SecretValue $secret
```

### 3. Como Obter as Connection Strings

#### Azure Service Bus

1. No Azure Portal, navegue até o seu **Service Bus Namespace**
2. No menu lateral, clique em **"Shared access policies"**
3. Selecione uma policy existente (ex: `RootManageSharedAccessKey`) ou crie uma nova com permissões de **"Send"**
4. Copie a **"Primary Connection String"**
5. Adicione esse valor como secret no Key Vault

**Criar uma fila no Azure Service Bus:**
1. No Service Bus Namespace, clique em **"Queues"**
2. Clique em **"+ Queue"**
3. Digite o nome (ex: `sensor-data-queue`)
4. Configure as opções (deixe os padrões ou ajuste conforme necessário)
5. Clique em **"Create"**

#### RabbitMQ

**Desenvolvimento Local (Docker):**
```bash
docker run -d --name rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  rabbitmq:3-management
```

- Connection String: `amqp://guest:guest@localhost:5672/`
- Painel de Gerenciamento: http://localhost:15672 (usuário: `guest`, senha: `guest`)

**CloudAMQP (Managed RabbitMQ):**
1. Acesse https://www.cloudamqp.com/
2. Crie uma instância (tem plano free)
3. Na dashboard, copie a **AMQP URL**
4. Exemplo: `amqp://user:password@hostname.cloudamqp.com/vhost`

**RabbitMQ em Servidor:**
- Formato: `amqp://username:password@hostname:port/vhost`
- Exemplo: `amqp://admin:secret@rabbitmq.meudominio.com:5672/production`

### 4. Permissões no Azure Key Vault

A aplicação precisa ter permissões para ler os secrets do Key Vault.

#### Opção A: Access Policies (Método Clássico)

1. No Key Vault, vá em **"Access policies"**
2. Clique em **"+ Add Access Policy"**
3. Em **Secret permissions**, selecione:
   - ? **Get**
   - ? **List**
4. Em **Select principal**, escolha a identidade da aplicação
5. Clique em **"Add"** e depois **"Save"**

#### Opção B: RBAC (Recomendado)

1. No Key Vault, vá em **"Access control (IAM)"**
2. Clique em **"+ Add"** > **"Add role assignment"**
3. Selecione a role **"Key Vault Secrets User"**
4. Na aba **Members**, clique em **"+ Select members"**
5. Pesquise e selecione a **Managed Identity** da sua aplicação
6. Clique em **"Review + assign"**

**Para desenvolvimento local:**
- Use `DefaultAzureCredential` que tenta várias formas de autenticação
- Faça login na Azure CLI: `az login`
- Ou use Visual Studio/VS Code com conta da Azure conectada

### 5. Registro no Program.cs

O código já está configurado para buscar secrets do Key Vault:

```csharp
var builder = WebApplication.CreateBuilder(args);
var keyVaultUrl = builder.Configuration["KeyVault:Url"];

if (!string.IsNullOrEmpty(keyVaultUrl))
{
    // Adiciona Azure Key Vault como provider de configuração
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl),
        new DefaultAzureCredential());
}

// Registra os serviços de fila
builder.Services.AddFilaQueue(builder.Configuration);
```

Os serviços irão automaticamente buscar as connection strings do Key Vault através do `IConfiguration`.

## ?? Uso

### Injeção de Dependência

Injete `IMessageQueueService` em suas classes:

```csharp
public class DadosSensorService
{
    private readonly IMessageQueueService _messageQueue;
    private readonly ILogger<DadosSensorService> _logger;

    public DadosSensorService(
        IMessageQueueService messageQueue,
        ILogger<DadosSensorService> logger)
    {
        _messageQueue = messageQueue;
        _logger = logger;
    }

    public async Task ProcessarDadosSensor(SensorCreateDto sensor)
    {
        // Salvar no banco de dados
        await _repository.SaveAsync(sensor);

        // Enviar para fila para processamento assíncrono
        await _messageQueue.SendMessageAsync(sensor, "sensor-processing-queue");
        
        _logger.LogInformation("Dados enviados para processamento assíncrono");
    }
}
```

### Exemplos de Envio

#### 1. Enviar Objeto Tipado

```csharp
var sensor = new SensorCreateDto
{
    TalhaoId = Guid.NewGuid(),
    DataHora = DateTime.UtcNow,
    TemperaturaValor = 25.5,
    UmidadeValor = 70,
    VelocidadeVento = 15.3
};

await _messageQueue.SendMessageAsync(sensor, "sensor-data-queue");
```

#### 2. Enviar String Simples

```csharp
var mensagem = "Processamento concluído com sucesso";
await _messageQueue.SendMessageAsync(mensagem, "notification-queue");
```

### Nomes de Filas Recomendados

- `sensor-data-queue` - Dados brutos dos sensores
- `sensor-processing-queue` - Sensores para processamento
- `alert-queue` - Alertas gerados
- `notification-queue` - Notificações
- `dead-letter-queue` - Mensagens com erro (DLQ)

## ?? Pacotes NuGet

Os pacotes já estão instalados no projeto `AgroSolutions.Infra`:

| Pacote | Versão | Finalidade |
|--------|--------|------------|
| `Azure.Messaging.ServiceBus` | 7.20.1 | Cliente para Azure Service Bus |
| `RabbitMQ.Client` | 7.2.0 | Cliente para RabbitMQ |
| `Azure.Extensions.AspNetCore.Configuration.Secrets` | - | Integração com Azure Key Vault |
| `Azure.Identity` | - | Autenticação com Azure |

## ?? Observações Importantes

### Comportamento
- ? Este serviço está configurado apenas para **ENVIO** de mensagens
- ? Não há implementação de **CONSUMO** de mensagens
- ?? As mensagens são enviadas de forma **durável** (persistente)
- ?? Logs são gerados para todas as operações de envio
- ?? Em caso de erro, exceções são propagadas para tratamento na aplicação

### Azure Service Bus
- Mensagens são enviadas com `ContentType` adequado (JSON ou text/plain)
- Cada mensagem recebe um `MessageId` único (GUID)
- Conexão é mantida durante toda a vida da aplicação (Singleton)
- Implementa `IAsyncDisposable` para liberação adequada de recursos
- Connection string é lida do Azure Key Vault

### RabbitMQ
- Filas são criadas automaticamente se não existirem (com `durable: true`)
- Mensagens são marcadas como persistentes
- Conexão possui recuperação automática em caso de falha
- Implementa `IDisposable` para liberação adequada de recursos
- API assíncrona (RabbitMQ.Client 7.x)
- Connection string é lida do Azure Key Vault

### Segurança
- ?? Connection strings **nunca** devem estar em código-fonte ou `appsettings.json`
- ?? Sempre use Azure Key Vault para armazenar secrets
- ?? No Key Vault, use nomes de secret com `--` que serão convertidos para `:` na configuração
- ?? Configure permissões adequadas (princípio do menor privilégio)

## ?? Tratamento de Erros

Sempre trate exceções ao enviar mensagens:

```csharp
try
{
    await _messageQueue.SendMessageAsync(sensor, "sensor-queue");
    _logger.LogInformation("Mensagem enviada com sucesso");
}
catch (ArgumentNullException ex)
{
    _logger.LogError(ex, "Connection string não encontrada no Key Vault");
    // Verificar se o secret existe no Key Vault
}
catch (Exception ex)
{
    _logger.LogError(ex, "Falha ao enviar mensagem para a fila");
    // Implementar lógica de retry ou fallback
}
```

## ?? Testando

### Verificar se o Secret está no Key Vault

```bash
# Listar todos os secrets
az keyvault secret list --vault-name talhaosensorvault --output table

# Ver um secret específico
az keyvault secret show --vault-name talhaosensorvault --name "ConnectionStrings--RabbitMQ"
```

### Teste Local com RabbitMQ

1. **Inicie o RabbitMQ via Docker:**
```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

2. **Adicione o secret no Key Vault:**
```bash
az keyvault secret set \
  --vault-name talhaosensorvault \
  --name "ConnectionStrings--RabbitMQ" \
  --value "amqp://guest:guest@localhost:5672/"
```

3. **Configure o appsettings.json:**
```json
{
  "KeyVault": {
    "Url": "https://talhaosensorvault.vault.azure.net/"
  },
  "MessageQueue": {
    "Provider": "RabbitMQ"
  }
}
```

4. **Execute a aplicação** e verifique os logs
5. **Acesse o painel do RabbitMQ**: http://localhost:15672
6. Verifique as filas criadas e mensagens enviadas

### Teste com Azure Service Bus

1. **Crie um Service Bus Namespace no Azure**
2. **Crie uma fila** dentro do namespace
3. **Copie a connection string** da policy com permissão "Send"
4. **Adicione o secret no Key Vault:**
```bash
az keyvault secret set \
  --vault-name talhaosensorvault \
  --name "ConnectionStrings--AzureServiceBus" \
  --value "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=SendPolicy;SharedAccessKey=your-key"
```
5. **Configure o appsettings.json:**
```json
{
  "MessageQueue": {
    "Provider": "AzureServiceBus"
  }
}
```
6. **Execute a aplicação** e verifique as mensagens no Azure Portal

## ?? Troubleshooting

### Erro: "ConnectionString não encontrada"

**Causa**: O secret não existe no Key Vault ou o nome está incorreto.

**Solução**:
1. Verifique se o secret existe: `az keyvault secret list --vault-name talhaosensorvault`
2. Certifique-se do nome correto: `ConnectionStrings--AzureServiceBus` ou `ConnectionStrings--RabbitMQ`
3. Verifique se a aplicação tem permissões para ler o Key Vault

### Erro: "Access denied" ou "403 Forbidden"

**Causa**: A aplicação não tem permissões no Key Vault.

**Solução**:
1. Adicione a identidade da aplicação nas Access Policies do Key Vault
2. Ou atribua a role "Key Vault Secrets User" via RBAC
3. Para desenvolvimento local, faça login: `az login`

### Erro ao conectar ao RabbitMQ ou Azure Service Bus

**Causa**: Connection string incorreta ou serviço não acessível.

**Solução**:
1. Verifique se o valor do secret está correto
2. Teste a conectividade com o serviço
3. Verifique regras de firewall e rede

## ?? Documentação Adicional

- [Azure Key Vault Documentation](https://learn.microsoft.com/en-us/azure/key-vault/)
- [Azure Service Bus Documentation](https://learn.microsoft.com/en-us/azure/service-bus-messaging/)
- [RabbitMQ Documentation](https://www.rabbitmq.com/documentation.html)
- [RabbitMQ .NET Client Guide](https://www.rabbitmq.com/dotnet-api-guide.html)
- [DefaultAzureCredential](https://learn.microsoft.com/en-us/dotnet/api/azure.identity.defaultazurecredential)

## ?? Suporte

Para dúvidas ou problemas:
1. Verifique os logs da aplicação
2. Verifique se os secrets existem no Key Vault
3. Verifique as permissões no Key Vault
4. Consulte a documentação oficial dos provedores
5. Verifique a conectividade com o serviço de mensageria
