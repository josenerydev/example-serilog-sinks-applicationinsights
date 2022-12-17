namespace example_serilog_sinks_applicationinsights.Services
{
    public interface IWeatherForecastService
    {
        IEnumerable<WeatherForecast> GetWeatherForecast();
    }
}