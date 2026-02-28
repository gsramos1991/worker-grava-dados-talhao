using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.OpenTelemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroSolutions.Infra.Ioc
{
    public static class SerilogExtensions
    {
        public static IServiceCollection AddSerilogApp(this IServiceCollection services, IConfiguration configuration)
        {
            try
            {
                var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
                // Implementation for adding Serilog
                var loggerConfiguration = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .Enrich.WithProperty("Application", "AgroSolutions.GravaDados.Sensor")
                    .Enrich.WithCorrelationId()
                    .Enrich.WithCorrelationIdHeader()
                    .Enrich.WithProperty("Enviroment", environment)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(

                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] CorrelationId:{CorrelationId} {Message}{NewLine}{Exception}"
                    )
                    .WriteTo.OpenTelemetry(options =>
                    {
                        options.Endpoint = "http://localhost:4317";
                        options.Protocol = OtlpProtocol.Grpc;
                        options.ResourceAttributes = new Dictionary<string, object>
                        {
                            ["service.name"] = "AgroSolutions.GravaDados.Sensor",
                            ["service.version"] = "1.0.0",
                            ["deployment.environment"] = environment
                        };
                    });

                if (environment != "Development")
                {
                    loggerConfiguration.MinimumLevel.Information();
                }


                Log.Logger = loggerConfiguration.CreateLogger();

                return services;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Aviso: Falha ao configurar OpenTelemetry: {ex.Message}");
            }
            return services;

        }
    }
}
