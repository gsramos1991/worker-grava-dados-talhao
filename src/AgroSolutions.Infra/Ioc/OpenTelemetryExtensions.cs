using AgroSolutions.Domain.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry;

namespace AgroSolutions.Infra.Ioc
{
    public static class OpenTelemetryExtensions
    {
        public static IServiceCollection AddOpenTelemetryApp(this IServiceCollection services, IConfiguration configuration)
        {
            var serviceName = "AgroSolutions.Service.Ingestor.Dados";
            var serviceVersion = "1.0.0";

            var resource = ResourceBuilder.CreateDefault()
                .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["enviroment"] = configuration["ASPNETCORE_ENVIROMENT"] ?? "Production",
                    ["deployment.environment"] = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production",
                    ["job"] = "agrosolutions"
                });

            services.AddOpenTelemetry()
                .WithTracing(builder =>
                {
                    builder
                        .SetResourceBuilder(resource)
                        .AddSource("MassTransit")
                        .AddSource(AgroSolutionsDiagnostics.SourceName)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation();
                        
                })
                .WithMetrics(metrics =>
                {
                    metrics
                        .SetResourceBuilder(resource)
                        .AddMeter("MassTransit");
                        
                }).UseOtlpExporter();

            return services;
        }
    }
}
