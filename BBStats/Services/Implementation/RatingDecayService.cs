using BBStats.Data;
using Microsoft.EntityFrameworkCore;

namespace BBStats.Services.Implementation;

public class RatingDecayService : BackgroundService
{
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly ILogger<RatingDecayService> _logger;

	public RatingDecayService(
		IServiceScopeFactory scopeFactory,
		ILogger<RatingDecayService> logger)
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
				var today = DateOnly.FromDateTime(DateTime.UtcNow);
				var yesterday = today.AddDays(-1);

				using var scope = _scopeFactory.CreateScope();
				var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

				var statsToDecay = await dbContext.PlayersCharactersStats
					.Where(s => s.LastPlayedAt < yesterday && 
								(s.LastDecayAppliedAt == null || s.LastDecayAppliedAt < today) &&
								s.PlayerRating.RatingDeviation < 175)
					.ToListAsync(stoppingToken);

				if (statsToDecay.Count > 0)
				{
					foreach (var stat in statsToDecay)
					{
						stat.PlayerRating = Glicko2Calculator.ApplyDecay(stat.PlayerRating);
						stat.LastDecayAppliedAt = today;
					}

					await dbContext.SaveChangesAsync(stoppingToken);
					_logger.LogInformation("Applied decay to {Count} player character stats.", statsToDecay.Count);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while applying rating decay.");
			}

			// Run once a day to check for new day
			await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
		}
	}
}
