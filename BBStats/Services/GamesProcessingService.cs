using BBStats.Data;

namespace BBStats.Services;

public class GamesProcessingService : BackgroundService
{
	private readonly IGamesParser _parser;
	private readonly GamesFetcherClient _client;
	private readonly IGamesRepository _repository;

	public GamesProcessingService(IGamesParser parser, 
		GamesFetcherClient client, IGamesRepository repository)
	{
		_parser = parser;
		_client = client;
		_repository = repository;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			var unparsedGames = await _client.GetGames();

			var games = _parser.Parse(unparsedGames).OrderBy(x => x.PlayedAt);

			foreach (var game in games)
			{
				_repository.AddGame(game);
			}

			await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
		}
	}
}
