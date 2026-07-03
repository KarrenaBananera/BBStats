using System.Text;
using BBStats.Configuration;
using Microsoft.Extensions.Options;

namespace BBStats.Services.Implementation;

public class HistoricalGamesFetcherClient
{
	private readonly HttpClient _httpClient;
	private readonly IOptionsMonitor<HistoricalGamesFetcherOptions> _options;
	private readonly string _requestTemplate;

	public HistoricalGamesFetcherClient(HttpClient httpClient, IOptionsMonitor<HistoricalGamesFetcherOptions> options)
	{
		_requestTemplate = File.ReadAllText("Services/Implementation/HistoricalRequestString.txt");
		_httpClient = httpClient;
		_options = options;
	}

	public async Task<string> GetGames(int page, string startDate, string endDate)
	{
		var baseUrl = _options.CurrentValue.BaseUrl;
		if (string.IsNullOrWhiteSpace(baseUrl))
		{
			throw new InvalidOperationException("HistoricalGamesFetcher:BaseUrl is not configured.");
		}

		var requestString = _requestTemplate
			.Replace("{PAGE}", page.ToString())
			.Replace("{START_DATE}", startDate)
			.Replace("{END_DATE}", endDate);

		var postContent = new StringContent(requestString, Encoding.UTF8, "application/json");

		var baseUri = new Uri(baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/");
		var requestUri = new Uri(baseUri, "_dash-update-component");
		var response = await _httpClient.PostAsync(requestUri, postContent);
		response.EnsureSuccessStatusCode();

		return await response.Content.ReadAsStringAsync();
	}
}
