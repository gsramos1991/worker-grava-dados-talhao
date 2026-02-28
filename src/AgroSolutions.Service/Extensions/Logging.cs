using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Events;
using OpenTelemetry.Exporter;


namespace AgroSolutions.Service.Extensions
{
    public static class Logging
    {
        public static void ConfigureSerilog()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}"
                )
                .CreateLogger();
        }

        public static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder)
        {
            var serviceName = "AgroSolutions.Service.Ingestor.Dados";

            var serviceVersion = builder.Configuration["OpenTelemetry:ServiceVersion"]
                ?? "1.0.0";

            var environment =
                builder.Configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";

            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithCorrelationId()
                .Enrich.WithProperty("ServiceName", serviceName)
                .Enrich.WithProperty("ServiceVersion", serviceVersion)
                .Enrich.WithProperty("Environment", environment)
                .WriteTo.Console(
                    outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] CorrelationId:{CorrelationId} {Message}{NewLine}{Exception}"
                )
                .WriteTo.OpenTelemetry(options =>
                {
                    options.ResourceAttributes = new Dictionary<string, object>
                    {
                        ["service.name"] = serviceName,
                        ["service.version"] = serviceVersion,
                        ["deployment.environment"] = environment,
                    };
                });

            Log.Logger = loggerConfig.CreateLogger();
            builder.Host.UseSerilog(Log.Logger);

            return builder;
        }
    }
}
