# Plano de Implementação — Azure Service Bus Consumer via MassTransit (AgroSolutions.Service)

## Objetivo
Implementar o consumo da fila `valida_dados_motor_sensor` no Azure Service Bus usando **MassTransit 8.x** no projeto `AgroSolutions.Service`. A configuração MassTransit fica em `AgroSolutions.Infra` (IoC extension), o consumer em `AgroSolutions.Service/Consumers/`, seguindo o padrão DTO → Model → `IDadosSensorService` já existente. OpenTelemetry é estendido para capturar métricas do MassTransit. `Worker.cs` (stub) é removido. **Nada no `TalhaoSensorApi` é alterado.**

---

## Checklist de Implementação

- [x] **0. plano.md** — Criado na raiz do projeto para rastrear progresso

- [x] **1. Domain — Contrato de mensagem**
  - Arquivo: `src/AgroSolutions.Domain/Messages/SensorMessage.cs`
  - Namespace: `AgroSolutions.Domain.Messages`
  - Campos: `TalhaoId (Guid)`, `Umidade (int)`, `DataAfericao (DateTime)`, `Temperatura (double)`, `IndiceUv (int)`, `VelocidadeVento (decimal)`
  - Motivo: `SensorCreateDto` tem namespace `AgroSolutions.TalhaoSensor.Api.DTOs` (namespace de API no projeto Domain — desvio já existente, não tocado). `SensorMessage` é o contrato limpo do barramento.

- [x] **2. Infra — NuGet packages**
  - `src/AgroSolutions.Infra/AgroSolutions.Infra.csproj`
    - `MassTransit` 8.x
    - `MassTransit.Azure.ServiceBus.Core` 8.x
  - `src/AgroSolutions.Service/AgroSolutions.Service.csproj`
    - `MassTransit` 8.x
    - `MassTransit.Azure.ServiceBus.Core` 8.x

- [x] **3. Infra — IoC Extension `MassTransitExtensions.cs`**
  - Arquivo: `src/AgroSolutions.Infra/Ioc/MassTransitExtensions.cs`
  - Método: `AddMassTransitQueue(IConfiguration, Action<IBusRegistrationConfigurator>)`
  - Guards (mesmo padrão de `DataAccessRepository` e `AzureServiceBusService`):
    - Lança `InvalidOperationException` se `ConnectionStrings:AzureServiceBus` estiver ausente
    - Lança `InvalidOperationException` se `MessageQueue:InputQueueName` estiver ausente
  - Configura `UsingAzureServiceBus` com a connection string do Key Vault
  - Consumer registrado via callback (Infra não depende de tipos do Service layer — Clean Architecture)

- [x] **4. Infra — OpenTelemetry update**
  - Arquivo: `src/AgroSolutions.Infra/Ioc/OpenTelemetryExtensions.cs`
  - Adiciona `.AddSource("MassTransit")` ao builder de tracing para expor métricas/traces do MassTransit no OTLP já configurado

- [x] **5. Service — Consumer**
  - Arquivo: `src/AgroSolutions.Service/Consumers/SensorConsumer.cs`
  - Implementa `IConsumer<SensorMessage>`
  - Injeta `IServiceScopeFactory` + `ILogger<SensorConsumer>`
  - Fluxo em `Consume`:
    1. Cria scope DI → resolve `Processamento`
    2. Mapeia `SensorMessage` → `SensorCreateDto`
    3. Chama `Processamento.ProcessaDados(dto)`
    4. Re-throw em exceção → MassTransit trata abandon/dead-letter (3 tentativas + dead-letter automático)
  - Método interno `ProcessMessageAsync(SensorMessage, CancellationToken)` exposto como `internal` para testabilidade

- [x] **6. Service — Remover `Worker.cs`**
  - `src/AgroSolutions.Service/Worker.cs` deletado
  - MassTransit registra seu próprio `IHostedService` (`IBusControl`)

- [x] **7. Service — `Program.cs` atualizado**
  - Adiciona `AddMassTransitQueue(configuration, cfg => cfg.AddConsumer<SensorConsumer>())` na cadeia de IoC
  - Remove `AddHostedService<Worker>()`

- [x] **8. Service — `appsettings.json` atualizado**
  - `KeyVault:Url` → `https://vaultkeys-grupo-fase05.vault.azure.net/`
  - `MessageQueue:Provider` → `"AzureServiceBus"`
  - `MessageQueue:InputQueueName` → `"valida_dados_motor_sensor"`

- [x] **9. Tests — `SensorConsumerTests.cs`**
  - Testa caminho sucesso: `ProcessMessageAsync` → `ProcessaDados` chamado com dados corretos
  - Testa caminho falha: exceção do `Processamento` propaga corretamente
  - Padrão: fakes manuais (sem Moq), `internal sealed class` no mesmo arquivo

- [x] **10. Tests — `MassTransitExtensionsTests.cs`**
  - Testa guard: `InvalidOperationException` quando `ConnectionStrings:AzureServiceBus` ausente
  - Testa guard: `InvalidOperationException` quando `MessageQueue:InputQueueName` ausente

---

## Verificação

- [x] `dotnet build` — solução compila sem erros (0 erros)
- [x] `dotnet test` — todos os 36 testes passam (0 failures)
- [ ] Run local com Key Vault configurado → MassTransit loga conexão com Azure Service Bus no startup
- [ ] End-to-end: publicar mensagem na fila `valida_dados_motor_sensor` → verificar que `ProcessaDados` é invocado e o registro chega no banco

---

## Decisões de Arquitetura

| Decisão | Justificativa |
|---|---|
| `Action<IBusRegistrationConfigurator>` no extension | Infra não depende de tipos do Service layer — Clean Architecture preservada |
| `SensorMessage` dedicada em `Domain.Messages` | `SensorCreateDto` tem namespace de API (desvio existente não tocado); contrato limpo do barramento |
| `Worker.cs` removido | MassTransit assume o `IHostedService`; stub não agrega valor |
| Retry: padrão MassTransit | 3 tentativas imediatas + dead-letter automático, sem configuração adicional |
| `ProcessMessageAsync` interno | Testabilidade sem depender de `ConsumeContext<T>` complexo |
| `AddSource("MassTransit")` | MassTransit 8.x publica seu próprio `ActivitySource`; basta incluí-lo no OTLP existente |
