using AgroSolutions.Infra.Ioc;
using AgroSolutions.Service.Consumers;
using Azure.Identity;

var builder = Host.CreateApplicationBuilder(args);

var keyVaultUrl = builder.Configuration["KeyVault:Url"];

Console.WriteLine($"[STARTUP] KeyVault:Url = {(string.IsNullOrEmpty(keyVaultUrl) ? "NÃO CONFIGURADO" : keyVaultUrl)}");

if (!string.IsNullOrEmpty(keyVaultUrl))
{
    try
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUrl),
            new DefaultAzureCredential());
        Console.WriteLine("[STARTUP] Azure Key Vault conectado com sucesso.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[STARTUP] WARNING: Falha ao conectar ao Azure Key Vault: {ex.Message}");
        Console.WriteLine("[STARTUP] Continuando apenas com configuração local...");
    }
}

var asbCs = builder.Configuration["ConnectionStrings:AzureServiceBus"];
var inputQueue = builder.Configuration["MessageQueue:InputQueueName"];
Console.WriteLine($"[STARTUP] ConnectionStrings:AzureServiceBus = {(string.IsNullOrEmpty(asbCs) ? "NÃO ENCONTRADO" : "OK (" + asbCs[..Math.Min(40, asbCs.Length)] + "...)")}");
Console.WriteLine($"[STARTUP] MessageQueue:InputQueueName      = {(string.IsNullOrEmpty(inputQueue) ? "NÃO CONFIGURADO" : inputQueue)}");

builder.Services.AddAzureSecrets(builder.Configuration)
         .AddOpenTelemetryApp(builder.Configuration)
         .AddSerilogApp(builder.Configuration)
         .AddRegisterRepositories()
         .AddFilaQueue(builder.Configuration)
         .AddRegisterServices()
         .AddMassTransitQueue(builder.Configuration, cfg => cfg.AddConsumer<SensorConsumer>());

builder.Services.AddScoped<AgroSolutions.Service.Service.Processamento>();

var host = builder.Build();
host.Run();
