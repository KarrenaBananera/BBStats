using BBStats.Data;

namespace BBStats.Services;

public class GamesProcessingService : BackgroundService
{
	private readonly IServiceScopeFactory _scopeFactory;

	public GamesProcessingService(IServiceScopeFactory scopeFactory)
	{
		_scopeFactory = scopeFactory;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			using (var scope = _scopeFactory.CreateScope())
			{
				var client = scope.ServiceProvider.GetRequiredService<GamesFetcherClient>();
				var parser = scope.ServiceProvider.GetRequiredService<IGamesParser>();
				var repositroy = scope.ServiceProvider.GetRequiredService<IGamesRepository>();
				var unparsedGames = await client.GetGames();

				var games = parser.Parse(unparsedGames).OrderBy(x => x.PlayedAt);

				foreach (var game in games)
				{
					await repositroy.AddGameAsync(game);
				}
			}
			await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
		}
	}
}
