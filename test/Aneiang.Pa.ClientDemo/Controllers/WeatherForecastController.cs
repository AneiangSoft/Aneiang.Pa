using Aneiang.Pa.Core.News.Models;
using Aneiang.Pa.Models;
using Aneiang.Pa.News;
using Microsoft.AspNetCore.Mvc;

namespace Aneiang.Pa.ClientDemo.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly INewsScraperFactory _newsScraperFactory;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, INewsScraperFactory newsScraperFactory)
        {
            _logger = logger;
            _newsScraperFactory = newsScraperFactory;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet(Name = "GetNews")]
        public async Task<NewsResult> GetNews()
        {
            var newsScraper = _newsScraperFactory.GetScraper(ScraperSource.WeiBo);
            var result = await newsScraper.GetNewsAsync();
            return result;
        }
    }
}
