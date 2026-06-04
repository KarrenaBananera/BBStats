using System.Globalization;
using BBStats.Data;
using Newtonsoft.Json.Linq;

namespace BBStats.Services;

public class GamesParser
{
	private static readonly TimeSpan SourceTimeZoneOffset = TimeSpan.FromHours(-4);

	public List<GameDTO> Parse(string data)
	{
		var result = new List<GameDTO>();

		var root = JObject.Parse(data);

		var tableChildren = root["response"]?["query-results"]?["children"]?["props"]?["children"] as JArray;
		if (tableChildren == null || tableChildren.Count < 2)
			return result;

		var headerRow = tableChildren[0]?["props"]?["children"]?["props"]?["children"] as JArray;
		if (headerRow == null)
			return result;

		var headers = headerRow
			.Select(h => h?["props"]?["children"]?.ToString())
			.ToList();

		var rows = tableChildren[1]?["props"]?["children"] as JArray;
		if (rows == null)
			return result;

		foreach (var row in rows)
		{
			var cells = row?["props"]?["children"] as JArray;
			if (cells == null || cells.Count != headers.Count)
				continue;

			var rowDict = new Dictionary<string, JToken>();

			for (int i = 0; i < headers.Count; i++)
				rowDict[headers[i]] = cells[i]?["props"]?["children"];

			var playerAName = rowDict["p1"]?.ToString();
			var playerBName = rowDict["p2"]?.ToString();

			var characterAName = rowDict["p1_toon"]?.ToString();
			var characterBName = rowDict["p2_toon"]?.ToString();

			var winner = rowDict["winner"]?.Value<int>() ?? 0;

			var playedAt = ParsePlayedAtUtc(rowDict["upload_datetime_"]!.ToString());

			var replayUrl = rowDict["open"]?["props"]?["href"]?.ToString();

			result.Add(new GameDTO
			{
				PlayerA = playerAName!,
				PlayerB = playerBName!,
				PlayerAId = rowDict["p1_steamid64"]!.Value<long>(),
				PlayerBId = rowDict["p2_steamid64"]!.Value<long>(),
				GameUrl = replayUrl!,
				PlayedAt = playedAt,
				IsPlayerAWin = winner == 0,
				CharacterAId = CharactersSeed.CharactersNames[characterAName!.ToLower()],
				CharacterBId = CharactersSeed.CharactersNames[characterBName!.ToLower()]
			});
		}

		return result;
	}

	private static DateTime ParsePlayedAtUtc(string uploadDateTime)
	{
		var localTime = DateTime.Parse(
			uploadDateTime,
			CultureInfo.InvariantCulture,
			DateTimeStyles.None);

		return DateTime.SpecifyKind(
			new DateTimeOffset(localTime, SourceTimeZoneOffset).UtcDateTime,
			DateTimeKind.Utc);
	}
}
