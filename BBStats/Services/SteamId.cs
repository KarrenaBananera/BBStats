namespace BBStats.Services;

public static class SteamId
{
	public const long MinValue = 76561197960265729;
	public const long MaxValue = 76561202255233023;

	public static bool TryParse(string? value, out long steamId)
	{
		steamId = 0;
		if (string.IsNullOrWhiteSpace(value))
		{
			return false;
		}

		if (!long.TryParse(value.Trim(), out steamId))
		{
			return false;
		}

		return steamId is >= MinValue and <= MaxValue;
	}
}
