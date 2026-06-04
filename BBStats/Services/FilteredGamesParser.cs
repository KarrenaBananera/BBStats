namespace BBStats.Services;

public class FilteredGamesParser : IGamesParser
{
	private const int MaxSeenGames = 250;

	private readonly GamesParser _parser = new();
	private readonly object _sync = new();
	private readonly Queue<string> _seenOrder = new();
	private readonly HashSet<string> _seenUrls = new(StringComparer.Ordinal);

	public List<GameDTO> Parse(string data)
	{
		var result = new List<GameDTO>();
		var games = _parser.Parse(data);

		foreach (var game in games)
		{
			if (TryRegister(game.GameUrl))
			{
				result.Add(game);
			}
		}

		return result;
	}

	private bool TryRegister(string gameUrl)
	{
		lock (_sync)
		{
			if (!_seenUrls.Add(gameUrl))
			{
				return false;
			}

			_seenOrder.Enqueue(gameUrl);

			while (_seenOrder.Count > MaxSeenGames)
			{
				var oldest = _seenOrder.Dequeue();
				_seenUrls.Remove(oldest);
			}

			return true;
		}
	}
}
