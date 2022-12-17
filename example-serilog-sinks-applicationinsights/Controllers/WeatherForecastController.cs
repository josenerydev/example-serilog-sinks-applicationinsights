using example_serilog_sinks_applicationinsights.Services;
using Microsoft.AspNetCore.Mvc;

namespace example_serilog_sinks_applicationinsights.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IWeatherForecastService _weatherForecastService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IWeatherForecastService weatherForecastService)
        {
            _logger = logger;
            _weatherForecastService = weatherForecastService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["UserId"] = "svrooij",
                ["OperationType"] = "update"
            }))
            {
                return _weatherForecastService.GetWeatherForecast();
            }
        }
    }
}