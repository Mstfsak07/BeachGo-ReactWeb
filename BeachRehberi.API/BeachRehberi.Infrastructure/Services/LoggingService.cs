using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace BeachRehberi.Infrastructure.Services;

/// <summary>
/// Logging service configuration
/// </summary>
public static class LoggingService
{
    public static void ConfigureLogging(IHostBuilder hostBuilder, IConfiguration configuration)
    {
        hostBuilder.UseSerilog((context, services, logConfig) =>
        {
            logConfig
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Error)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "BeachRehberi.API")
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/beachrehberi-.log",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(configuration["Elasticsearch:Uri"] ?? "http://localhost:9200"))
                {
                    AutoRegisterTemplate = true,
                    IndexFormat = "beachrehberi-api-{0:yyyy.MM.dd}",
                    TypeName = null
                })
                .WriteTo.Seq(configuration["Seq:Uri"] ?? "http://localhost:5341");
        });
    }
}