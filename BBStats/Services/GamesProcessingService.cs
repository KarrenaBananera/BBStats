using BBStats.Data;
using System.Linq.Expressions;

namespace BBStats.Services;

public class GamesProcessingService : BackgroundService
{
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly ILogger<GamesProcessingService> _logger;

	public GamesProcessingService(IServiceScopeFactory scopeFactory, ILogger<GamesProcessingService> logger)
	{
		_scopeFactory = scopeFactory;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
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
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Fetching games error");
			}
			await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
		}
	}
}