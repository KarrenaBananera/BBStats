using BBStats.Data;
using BBStats.Models.UI;
using Microsoft.EntityFrameworkCore;

namespace BBStats.Services;

public class CharacterStatisticsService : ICharacterStatisticsService
{
	private readonly AppDbContext _dbContext;

	public CharacterStatisticsService(AppDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<CharacterStatisticsViewModel?> GetStatisticsAsync(
		int characterId,
		CancellationToken cancellationToken = default)
	{
		var character = CharactersSeed.All.FirstOrDefault(c => c.Id == characterId);
		if (character is null)
		{
			return null;
		}

		var characterNames = CharactersSeed.All.ToDictionary(c => c.Id, c => c.Name);

		var totalGamesCount = await _dbContext.Games.CountAsync(cancellationToken);
		var characterGames = _dbContext.Games.Where(g =>
			g.CharacterAId == characterId || g.CharacterBId == characterId);

		var totalMatches = await characterGames.CountAsync(cancellationToken);
		var wins = await characterGames.CountAsync(g =>
			(g.CharacterAId == characterId && g.IsPlayerAWin) ||
			(g.CharacterBId == characterId && !g.IsPlayerAWin),
			cancellationToken);

		var usagePercent = totalGamesCount > 0
			? (double)totalMatches / totalGamesCount * 100
			: 0;
		var winratePercent = totalMatches > 0
			? (double)wins / totalMatches * 100
			: 0;

		var matchupsAsA = await _dbContext.Matchups
			.AsNoTracking()
			.Where(m => m.CharacterAId == characterId)
			.ToListAsync(cancellationToken);

		var matchupsAsB = await _dbContext.Matchups
			.AsNoTracking()
			.Where(m => m.CharacterBId == characterId)
			.ToListAsync(cancellationToken);

		var matchups = new List<MatchupStatViewModel>(matchupsAsA.Count + matchupsAsB.Count);

		foreach (var matchup in matchupsAsA)
		{
			if (!characterNames.TryGetValue(matchup.CharacterBId, out var opponentName))
			{
				continue;
			}

			matchups.Add(CreateMatchupStat(
				opponentName,
				matchup.TotalGames,
				matchup.WinsA,
				totalMatches));
		}

		foreach (var matchup in matchupsAsB)
		{
			if (!characterNames.TryGetValue(matchup.CharacterAId, out var opponentName))
			{
				continue;
			}

			matchups.Add(CreateMatchupStat(
				opponentName,
				matchup.TotalGames,
				matchup.WinsB,
				totalMatches));
		}

		return new CharacterStatisticsViewModel
		{
			CharacterSlug = character.Name.ToLowerInvariant(),
			DisplayName = character.Name,
			PortraitImageUrl = CharacterImages.GetPortraitUrl(character.Name),
			UsagePercent = usagePercent,
			WinratePercent = winratePercent,
			TotalMatches = totalMatches,
			Matchups = matchups
				.OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase)
				.ToList()
		};
	}

	private MatchupStatViewModel CreateMatchupStat(
		string opponentName,
		int matchupGames,
		int characterWins,
		int characterTotalMatches)
	{
		var winrate = matchupGames > 0
			? (double)characterWins / matchupGames * 100
			: 0;
		var frequency = characterTotalMatches > 0
			? (double)matchupGames / characterTotalMatches * 100
			: 0;

		return new MatchupStatViewModel(
			opponentName,
			CharacterImages.GetIconUrl(opponentName),
			frequency,
			matchupGames,
			winrate,
			GetWinrateCss(winrate));
	}

	private static string GetWinrateCss(double winrate) => winrate switch
	{
		< 45 => "text-danger",
		> 55 => "text-success",
		_ => "text-warning"
	};
}
