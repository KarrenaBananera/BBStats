using System.Text;

namespace BBStats.Services;

public class GamesFetcherClient
{
	private readonly HttpClient _httpClient;
	private readonly string _requestString;
	private HttpContent _postContent;
	public GamesFetcherClient(HttpClient httpClient)
	{
		_requestString = File.ReadAllText("RequestString.txt");
		_httpClient = httpClient;
		_postContent = new StringContent(_requestString, Encoding.UTF8, "application/json");
	}

	public async Task<string> GetGames()
	{
		var response = await _httpClient.PostAsync("/_dash-update-component",_postContent);
		response.EnsureSuccessStatusCode();

		return await response.Content.ReadAsStringAsync();
	}
}

