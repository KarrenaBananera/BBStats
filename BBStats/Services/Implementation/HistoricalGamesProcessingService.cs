using BBStats.Configuration;
using BBStats.Data;
using BBStats.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace BBStats.Services.Implementation;

public class HistoricalGamesProcessingService : BackgroundService
{
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly IOptionsMonitor<HistoricalGamesFetcherOptions> _options;
	private readonly ILogger<HistoricalGamesProcessingService> _logger;

	public HistoricalGamesProcessingService(
		IServiceScopeFactory scopeFactory,
		IOptionsMonitor<HistoricalGamesFetcherOptions> options,
		ILogger<HistoricalGamesProcessingService> logger)
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
					"Historical game fetching is disabled. Next check in {IntervalSeconds}s.",
					interval.TotalSeconds);
				await Task.Delay(interval, stoppingToken);
				continue;
			}

			try
			{
				using var scope = _scopeFactory.CreateScope();
				var client = scope.ServiceProvider.GetRequiredService<HistoricalGamesFetcherClient>();
				var parser = scope.ServiceProvider.GetRequiredService<IGamesParser>();
				var repository = scope.ServiceProvider.GetRequiredService<IGamesRepository>();

				var startDate = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");
				var endDate = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");

				int page = 0;
				bool hasMorePages = true;
				int totalFetched = 0;

				_logger.LogInformation("Starting historical games fetch from {StartDate} to {EndDate}", startDate, endDate);

				while (hasMorePages && !stoppingToken.IsCancellationRequested)
				{
					_logger.LogInformation("Fetching historical games page {Page}", page);
					var unparsedGames = await client.GetGames(page, startDate, endDate);
					
					// Stop if there are no matches (the API returns children: null)
					if (unparsedGames.Contains("\"children\": null") && unparsedGames.Contains("\"query-results\""))
					{
						_logger.LogInformation("No games found for the day.");
						break;
					}

					try
					{
						var root = Newtonsoft.Json.Linq.JObject.Parse(unparsedGames);
						var returnedPage = (int?)root["response"]?["page-num"]?["data"];
						
						// If the server returns a page less than requested, it means we requested a page beyond the end.
						if (returnedPage != null && returnedPage.Value < page)
						{
							_logger.LogInformation("Reached end of pages. Requested {Page}, server returned {ReturnedPage}.", page, returnedPage.Value);
							break;
						}
					}
					catch (Exception ex)
					{
						_logger.LogWarning("Failed to parse page-num from response: {Message}", ex.Message);
					}

					var games = parser.Parse(unparsedGames).OrderBy(x => x.PlayedAt).ToList();

					if (games.Count > 0)
					{
						foreach (var game in games)
						{
							await repository.AddGameAsync(game);
						}
						totalFetched += games.Count;
						_logger.LogInformation("Saved {Count} historical games from page {Page}.", games.Count, page);
					}

					page++;
				}

				_logger.LogInformation("Finished fetching historical games. Total parsed and saved: {Total}", totalFetched);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error fetching historical games");
			}

			await Task.Delay(interval, stoppingToken);
		}
	}
}
