using BBStats.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BBStats.Services;

public class FilteredGamesParser : IGamesParser, IHostedService
{
	private const int MaxSeenGames = 250;

	private readonly GamesParser _parser = new();
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly object _sync = new();
	private readonly Queue<string> _seenOrder = new();
	private readonly HashSet<string> _seenReplayIds = new(StringComparer.Ordinal);

	public FilteredGamesParser(IServiceScopeFactory scopeFactory)
	{
		_scopeFactory = scopeFactory;
	}

	public List<GameDTO> Parse(string data)
	{
		var result = new List<GameDTO>();
		var games = _parser.Parse(data);

		foreach (var game in games)
		{
			if (TryRegister(game.ReplayId))
			{
				result.Add(game);
			}
		}

		return result;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		using var scope = _scopeFactory.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

		var replayIds = await dbContext.Games
			.AsNoTracking()
			.OrderByDescending(g => g.Id)
			.Select(g => g.ReplayId)
			.Take(MaxSeenGames)
			.ToListAsync(cancellationToken);

		lock (_sync)
		{
			foreach (var replayId in replayIds)
			{
				if (string.IsNullOrWhiteSpace(replayId) || !_seenReplayIds.Add(replayId))
				{
					continue;
				}

				_seenOrder.Enqueue(replayId);
			}
		}
	}

	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

	private bool TryRegister(string replayId)
	{
		lock (_sync)
		{
			if (!_seenReplayIds.Add(replayId))
			{
				return false;
			}

			_seenOrder.Enqueue(replayId);

			while (_seenOrder.Count > MaxSeenGames)
			{
				var oldest = _seenOrder.Dequeue();
				_seenReplayIds.Remove(oldest);
			}

			return true;
		}
	}
}
