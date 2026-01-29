
using System.Collections.Concurrent;

namespace BBStats.Services;

public class FilteredGamesParser : IGamesParser
{
	private readonly GamesParser _parser = new ();
	private readonly ConcurrentDictionary<string, DateTime> _seenGames = new();
	private readonly TimeSpan _ttl = TimeSpan.FromHours(12);
	public List<GameDTO> Parse(string data)
	{
		var result = new List<GameDTO>();
		var games = _parser.Parse(data);
		
		foreach (var game in games)
		{
			if (_seenGames.TryAdd(game.GameUrl, DateTime.UtcNow))
				result.Add(game);
		}

		return result;
	}

	private FilteredGamesParser()
	{
		var timer = new System.Timers.Timer(50000);
		timer.Elapsed += (_,_) => Cleanup();
		timer.AutoReset = true;
		timer.Enabled = true;
	}

	public void Cleanup()
	{
		var threshold = DateTime.UtcNow - _ttl;

		foreach (var pair in _seenGames)
		{
			if (pair.Value < threshold)
				_seenGames.TryRemove(pair.Key, out _);
		}
	}
}
