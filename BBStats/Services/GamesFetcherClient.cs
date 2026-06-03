using System.Text;
using BBStats.Configuration;
using Microsoft.Extensions.Options;

namespace BBStats.Services;

public class GamesFetcherClient
{
	private readonly HttpClient _httpClient;
	private readonly IOptionsMonitor<GamesFetcherOptions> _options;
	private readonly string _requestString;
	private readonly HttpContent _postContent;

	public GamesFetcherClient(HttpClient httpClient, IOptionsMonitor<GamesFetcherOptions> options)
	{
		_requestString = File.ReadAllText("Services/RequestString.txt");
		_httpClient = httpClient;
		_options = options;
		_postContent = new StringContent(_requestString, Encoding.UTF8, "application/json");
	}

	public async Task<string> GetGames()
	{
		var baseUrl = _options.CurrentValue.BaseUrl;
		if (string.IsNullOrWhiteSpace(baseUrl))
		{
			throw new InvalidOperationException("GamesFetcher:BaseUrl is not configured in games-fetcher.json.");
		}

		var baseUri = new Uri(baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/");
		var requestUri = new Uri(baseUri, "_dash-update-component");
		var response = await _httpClient.PostAsync(requestUri, _postContent);
		response.EnsureSuccessStatusCode();

		return await response.Content.ReadAsStringAsync();
	}
}

