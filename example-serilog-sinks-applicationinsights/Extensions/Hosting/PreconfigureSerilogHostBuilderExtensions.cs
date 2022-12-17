﻿using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using Serilog.Extensions.Hosting;
using System.Reflection;

namespace example_serilog_sinks_applicationinsights.Extensions.Hosting
{
    public static class PreconfigureSerilogHostBuilderExtensions
    {
        public static IConfigurationRoot CreatePreconfigureConfigurationBuilder()
        {
            return new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json")
                 .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
                 .AddUserSecrets(Assembly.GetExecutingAssembly(), false)
                 .AddEnvironmentVariables()
                 .Build();
        }

        public static IHostBuilder UsePreconfigureSerilog(this IHostBuilder builder, IConfiguration configuration)
        {
            return builder.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .DefaultLoggerConfiguration(configuration));
        }

        public static ReloadableLogger CreatePreconfigureBootstrapLogger(IConfiguration configuration)
        {
            return new LoggerConfiguration()
                .PreconfigureLogger(configuration)
                .CreateBootstrapLogger();
        }

        public static LoggerConfiguration PreconfigureLogger(this LoggerConfiguration loggerConfiguration, IConfiguration configuration)
        {
            VerifyApplicationName(configuration);

            return loggerConfiguration
                .ReadFrom.Configuration(configuration)
                .DefaultLoggerConfiguration(configuration);
        }

        private static LoggerConfiguration DefaultLoggerConfiguration(this LoggerConfiguration loggerConfiguration, IConfiguration configuration)
        {
            VerifyApplicationName(configuration);

            return loggerConfiguration
                .Enrich.FromLogContext()
                .Enrich.WithCorrelationIdHeader()
                .WriteTo.Async(wt => wt.Console())
                .WriteTo.ApplicationInsights(GetTelemetryConfiguration(configuration), TelemetryConverter.Traces);
        }

        private static TelemetryConfiguration GetTelemetryConfiguration(IConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
                throw new ArgumentException("APPLICATIONINSIGHTS_CONNECTION_STRING can't to be null or empty");

            var telemetryConfiguration = new TelemetryConfiguration
            {
                ConnectionString = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]
            };

            return telemetryConfiguration;
        }

        private static void VerifyApplicationName(IConfiguration configuration)
        {
            var application = configuration.GetValue<string>("Serilog:Properties:Application");
            if (string.IsNullOrWhiteSpace(application))
                throw new ArgumentException("Serilog:Properties:Application can't to be null or empty");
        }
    }
}