namespace BBStats.Services;

public static class GameReplayLink
{
	private const string LoadReplayPrefix = "load-replay=";

	public static (string? OpenUrl, string? DownloadUrl) Parse(string? gameUrl)
	{
		if (string.IsNullOrWhiteSpace(gameUrl))
		{
			return (null, null);
		}

		if (gameUrl.StartsWith("steam://", StringComparison.OrdinalIgnoreCase))
		{
			var replayHttpUrl = ExtractLoadReplayUrl(gameUrl);
			return (gameUrl, replayHttpUrl);
		}

		if (gameUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
		    gameUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
		{
			return (null, gameUrl);
		}

		return (gameUrl, null);
	}

	private static string? ExtractLoadReplayUrl(string steamUrl)
	{
		var index = steamUrl.IndexOf(LoadReplayPrefix, StringComparison.OrdinalIgnoreCase);
		if (index < 0)
		{
			return null;
		}

		var value = steamUrl[(index + LoadReplayPrefix.Length)..];
		var ampersand = value.IndexOf('&');
		if (ampersand >= 0)
		{
			value = value[..ampersand];
		}

		return Uri.UnescapeDataString(value);
	}
}
