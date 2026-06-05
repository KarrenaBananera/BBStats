using BBStats.Data;
using BBStats.Models.UI;
using Microsoft.EntityFrameworkCore;

namespace BBStats.Services;

public class PlayerCharacterStatsService : IPlayerCharacterStatsService
{
	private const int MostPlayedLimit = 20;

	private readonly AppDbContext _dbContext;

	public PlayerCharacterStatsService(AppDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<PlayerCharacterStatsViewModel?> GetStatsAsync(
		long playerId,
		string characterSlug,
		CancellationToken cancellationToken = default)
	{
		var slug = characterSlug.Trim().ToLowerInvariant();
		if (!CharactersSeed.CharactersNames.TryGetValue(slug, out var characterId))
		{
			return null;
		}

		var hasCharacter = await _dbContext.PlayersCharactersStats
			.AsNoTracking()
			.AnyAsync(stat => stat.PlayerId == playerId && stat.CharacterId == characterId, cancellationToken);

		if (!hasCharacter)
		{
			return null;
		}

		var gameRows = await _dbContext.PlayersGames
			.AsNoTracking()
			.Include(pg => pg.Game)
			.Where(pg => pg.PlayerId == playerId && pg.CharacterId == characterId)
			.Select(pg => new OpponentGameRow(
				pg.Game.PlayerAId == playerId ? pg.Game.PlayerBId : pg.Game.PlayerAId,
				pg.Game.PlayerAId == playerId ? pg.Game.PlayerB : pg.Game.PlayerA,
				pg.Game.PlayerAId == playerId ? pg.Game.CharacterBId : pg.Game.CharacterAId,
				(pg.Game.PlayerAId == playerId && pg.Game.IsPlayerAWin) ||
				(pg.Game.PlayerBId == playerId && !pg.Game.IsPlayerAWin)))
			.ToListAsync(cancellationToken);

		if (gameRows.Count == 0)
		{
			return new PlayerCharacterStatsViewModel();
		}

		var characterNames = CharactersSeed.All.ToDictionary(c => c.Id, c => c.Name);

		var mostPlayed = gameRows
			.GroupBy(row => row.OpponentId)
			.Select(group =>
			{
				var games = group.Count();
				var wins = group.Count(row => row.Won);
				var winrate = (double)wins / games * 100;
				var topOpponentCharacterId = group
					.GroupBy(row => row.OpponentCharacterId)
					.OrderByDescending(characterGroup => characterGroup.Count())
					.First()
					.Key;

				return new MostPlayedOpponentRow(
					group.First().OpponentName,
					group.Key.ToString(),
					characterNames[topOpponentCharacterId].ToLowerInvariant(),
					games,
					winrate,
					GetWinrateCss(winrate));
			})
			.OrderByDescending(row => row.Games)
			.ThenBy(row => row.OpponentName, StringComparer.OrdinalIgnoreCase)
			.Take(MostPlayedLimit)
			.ToList();

		var matchups = gameRows
			.GroupBy(row => row.OpponentCharacterId)
			.Select(group =>
			{
				var games = group.Count();
				var wins = group.Count(row => row.Won);
				var winrate = (double)wins / games * 100;
				var characterName = characterNames[group.Key];

				return new PlayerCharacterMatchupRow(
					characterName,
					characterName.ToLowerInvariant(),
					games,
					winrate,
					GetWinrateCss(winrate));
			})
			.OrderByDescending(row => row.Games)
			.ThenBy(row => row.CharacterName, StringComparer.OrdinalIgnoreCase)
			.ToList();

		return new PlayerCharacterStatsViewModel
		{
			MostPlayedOpponents = mostPlayed,
			CharacterMatchups = matchups
		};
	}

	private static string GetWinrateCss(double winrate) => winrate switch
	{
		< 45 => "text-danger",
		> 55 => "text-success",
		_ => "text-warning"
	};

	private sealed record OpponentGameRow(
		long OpponentId,
		string OpponentName,
		int OpponentCharacterId,
		bool Won);
}
