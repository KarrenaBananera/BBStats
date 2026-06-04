using BBStats.Configuration;
using BBStats.Data;
using Microsoft.Extensions.Options;

namespace BBStats.Services;

public class GamesProcessingService : BackgroundService
{
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly IOptionsMonitor<GamesFetcherOptions> _options;
	private readonly ILogger<GamesProcessingService> _logger;

	public GamesProcessingService(
		IServiceScopeFactory scopeFactory,
		IOptionsMonitor<GamesFetcherOptions> options,
		ILogger<GamesProcessingService> logger)
	{
		_scopeFactory = scopeFactory;
		_options = options;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			var options = _options.CurrentValue;
			var interval = TimeSpan.FromSeconds(Math.Max(1, options.FetchIntervalSeconds));

			if (!options.Enabled)
			{
				_logger.LogInformation(
					"Game fetching is disabled. Next check in {IntervalSeconds}s.",
					interval.TotalSeconds);
				await Task.Delay(interval, stoppingToken);
				continue;
			}

			try
			{
				using var scope = _scopeFactory.CreateScope();
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
			catch (Exception ex)
			{
				_logger.LogError(ex, "Fetching games error");
			}

			await Task.Delay(interval, stoppingToken);
		}
	}
}
