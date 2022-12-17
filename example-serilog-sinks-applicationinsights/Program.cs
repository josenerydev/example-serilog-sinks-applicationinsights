using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using System.Reflection;

var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
        .AddUserSecrets(Assembly.GetExecutingAssembly(), false)
        .AddEnvironmentVariables()
        .Build();

var guid = Guid.NewGuid().ToString().Substring(0, 4);

var telemetryConfiguration = new TelemetryConfiguration
{
    ConnectionString = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]
};

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application {0}", guid);

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces));

    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddApplicationInsightsTelemetry();

    var app = builder.Build();

    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("CorrelationId", guid);
        };
    });

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    //throw new Exception($"Exception for test {guid}");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly {0}", guid);
}
finally
{
    Log.CloseAndFlush();
}
