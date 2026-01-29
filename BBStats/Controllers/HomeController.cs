using BBStats.Data;
using BBStats.Models;
using BBStats.Services;
using Microsoft.AspNetCore.Mvc;
    using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;

namespace BBStats.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
		private readonly AppDbContext dbContext;

		public HomeController(ILogger<HomeController> logger, AppDbContext dbContext)
        {
            _logger = logger;
			this.dbContext = dbContext;
		}

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Privacy()
        {
            var result = "";

			//         using var client = new HttpClient();
			//         string request = "{\"output\":\"..query-results.children...warning-text-latest.children..\",\"outputs\":[{\"id\":\"query-results\",\"property\":\"children\"},{\"id\":\"warning-text-latest\",\"property\":\"children\"}],\"inputs\":[{\"id\":\"query-button\",\"property\":\"n_clicks\",\"value\":0}],\"changedPropIds\":[],\"parsedChangedPropsIds\":[],\"state\":[{\"id\":\"date-range\",\"property\":\"start_date\"},{\"id\":\"date-range\",\"property\":\"end_date\"},{\"id\":\"p1-input\",\"property\":\"value\"},{\"id\":\"p1-steamid64-input\",\"property\":\"value\"},{\"id\":\"p1-toon\",\"property\":\"value\"}]}";
			//HttpContent content = new StringContent(request, Encoding.UTF8, "application/json");

			//try
			//{
			//	// GET-запрос
			//	var response = await client.PostAsync("http://50.118.225.175:2000/_dash-update-component", content);

			//	// Проверка успешности запроса
			//	response.EnsureSuccessStatusCode();

			//	// Чтение ответа
			//	string responseBody = await response.Content.ReadAsStringAsync();

			//	result = responseBody;
			//}
			//catch (HttpRequestException e)
			//{
			//	result = $"Ошибка: {e.Message}";
			//}
			var parser = new GamesParser();

			var games = parser.Parse(System.IO.File.ReadAllText("Services/DataExample.txt"));

			//foreach (var character in dbContext.Characters)
			//         {
			//             result += "<br>";
			//             result += character.Name;
			//         }

			foreach (var game in games)
			{
				result += "<br>";
				result += $"{game.PlayerA} Id - {game.PlayerAId} characher- {game.CharacterAId} date - {game.PlayedAt.ToUniversalTime()}";
				result += "<br>";
				result += $"{game.PlayerB} Id - {game.PlayerBId} characher- {game.CharacterBId}";
				result += "<br>";
			}
			ViewBag.message = result;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
