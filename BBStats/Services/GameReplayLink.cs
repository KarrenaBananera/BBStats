namespace BBStats.Services;

public static class GameReplayLink
{
	private const string SteamAppOpenUrl = "steam://run/586140/?load-replay=";
	private const string LoadReplayPrefix = "load-replay=";

	public static string? ExtractReplayId(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return null;
		}

		var trimmed = value.Trim();

		if (!trimmed.Contains(':') && !trimmed.Contains('/'))
		{
			return trimmed.EndsWith(".dat", StringComparison.OrdinalIgnoreCase)
				? trimmed[..^4]
				: trimmed;
		}

		var downloadUrl = trimmed.StartsWith("steam://", StringComparison.OrdinalIgnoreCase)
			? ExtractLoadReplayUrl(trimmed)
			: trimmed;

		if (string.IsNullOrWhiteSpace(downloadUrl) ||
		    !Uri.TryCreate(downloadUrl, UriKind.Absolute, out var uri))
		{
			return null;
		}

		var fileName = Path.GetFileName(uri.LocalPath);
		if (string.IsNullOrWhiteSpace(fileName))
		{
			return null;
		}

		return fileName.EndsWith(".dat", StringComparison.OrdinalIgnoreCase)
			? fileName[..^4]
			: fileName;
	}

	public static string BuildDownloadUrl(string replayId, string urlForFront)
	{
		var host = NormalizeUrlForFront(urlForFront);
		return $"http://{host}/download/{replayId}.dat";
	}

	public static string BuildOpenUrl(string replayId, string urlForFront) =>
		SteamAppOpenUrl + BuildDownloadUrl(replayId, urlForFront);

	public static (string? OpenUrl, string? DownloadUrl) Build(string replayId, string urlForFront)
	{
		if (string.IsNullOrWhiteSpace(replayId))
		{
			return (null, null);
		}

		var downloadUrl = BuildDownloadUrl(replayId, urlForFront);
		return (BuildOpenUrl(replayId, urlForFront), downloadUrl);
	}

	private static string NormalizeUrlForFront(string urlForFront)
	{
		var host = urlForFront.Trim();

		if (host.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
		{
			host = host[7..];
		}
		else if (host.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
		{
			host = host[8..];
		}

		return host.TrimEnd('/');
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
