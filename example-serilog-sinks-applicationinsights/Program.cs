using example_serilog_sinks_applicationinsights.Extensions.Hosting;
using example_serilog_sinks_applicationinsights.Services;
using Serilog;

var configuration = PreconfigureSerilogHostBuilderExtensions.CreatePreconfigureConfigurationBuilder();

var guid = Guid.NewGuid().ToString().Substring(0, 4);

Log.Logger = PreconfigureSerilogHostBuilderExtensions.CreatePreconfigureBootstrapLogger(configuration);

try
{
    Log.Information("Starting web application {0}", guid);

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UsePreconfigureSerilog(configuration);

    // Add services to the container.
    builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    //builder.Services.AddApplicationInsightsTelemetry();

    var app = builder.Build();

    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
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

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}