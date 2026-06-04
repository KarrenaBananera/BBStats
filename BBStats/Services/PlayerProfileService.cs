using BBStats.Data;
using BBStats.Data.Entites;
using BBStats.Models.UI;
using Microsoft.EntityFrameworkCore;

namespace BBStats.Services;

public class PlayerProfileService : IPlayerProfileService
{
	private const int SetsPerPage = 10;

	private readonly AppDbContext _dbContext;

	public PlayerProfileService(AppDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<PlayerProfileResult> GetProfileAsync(
		long playerId,
		string characterSlug,
		int pageNumber,
		CancellationToken cancellationToken = default)
	{
		pageNumber = Math.Max(1, pageNumber);
		var slug = characterSlug.Trim().ToLowerInvariant();

		var player = await _dbContext.Players
			.AsNoTracking()
			.FirstOrDefaultAsync(p => p.Id == playerId, cancellationToken);

		if (player is null)
		{
			return PlayerProfileResult.NotFound();
		}

		var characterStats = await _dbContext.PlayersCharactersStats
			.AsNoTracking()
			.Include(stat => stat.Character)
			.Where(stat => stat.PlayerId == playerId)
			.OrderByDescending(stat => stat.Wins + stat.Losses)
			.ThenBy(stat => stat.Character.Name)
			.ToListAsync(cancellationToken);

		if (characterStats.Count == 0)
		{
			return PlayerProfileResult.Found(new PlayerProfilePageViewModel
			{
				PlayerName = player.Name,
				SteamId = playerId.ToString(),
				CharacterSlug = slug,
				EmptyMessage = "No recorded matches for this player yet."
			});
		}

		if (!CharactersSeed.CharactersNames.TryGetValue(slug, out var characterId) ||
		    characterStats.All(stat => stat.CharacterId != characterId))
		{
			return PlayerProfileResult.RedirectToCharacter(
				characterStats[0].Character.Name.ToLowerInvariant());
		}

		var activeStat = characterStats.First(stat => stat.CharacterId == characterId);
		var allSets = await BuildMatchSeriesAsync(playerId, characterId, cancellationToken);
		var totalPages = allSets.Count == 0
			? 1
			: (int)Math.Ceiling(allSets.Count / (double)SetsPerPage);

		if (pageNumber > totalPages)
		{
			pageNumber = totalPages;
		}

		var pageSets = allSets
			.Skip((pageNumber - 1) * SetsPerPage)
			.Take(SetsPerPage)
			.ToList();

		var sidebar = characterStats
			.Select(stat => new CharacterSidebarItem(
				stat.Character.Name.ToLowerInvariant(),
				stat.Character.Name,
				stat.Wins + stat.Losses,
				(int)Math.Round(stat.PlayerRating.CurrentRating),
				stat.CharacterId == characterId))
			.ToList();

		var characterRank = await GetCharacterRankAsync(characterId, activeStat, cancellationToken);
		var overallRank = await GetOverallRankAsync(activeStat, cancellationToken);

		return PlayerProfileResult.Found(new PlayerProfilePageViewModel
		{
			PlayerName = player.Name,
			SteamId = playerId.ToString(),
			CharacterSlug = slug,
			CharacterDisplayName = activeStat.Character.Name,
			OverallRank = overallRank,
			CharacterRank = characterRank,
			Rating = (int)Math.Round(activeStat.PlayerRating.CurrentRating),
			RatingDeviation = (int)Math.Round(activeStat.PlayerRating.RatingDeviation),
			Characters = sidebar,
			Series = pageSets,
			CurrentPage = pageNumber,
			TotalPages = totalPages,
			EmptyMessage = allSets.Count == 0 ? "No recorded series for this character." : null
		});
	}

	private async Task<List<MatchSeriesViewModel>> BuildMatchSeriesAsync(
		long playerId,
		int characterId,
		CancellationToken cancellationToken)
	{
		var playerGames = await _dbContext.PlayersGames
			.AsNoTracking()
			.Include(pg => pg.Game)
			.Where(pg => pg.PlayerId == playerId && pg.CharacterId == characterId)
			.OrderBy(pg => pg.Game.PlayedAt)
			.ThenBy(pg => pg.GameId)
			.ToListAsync(cancellationToken);

		if (playerGames.Count == 0)
		{
			return [];
		}

		var characterNames = CharactersSeed.All.ToDictionary(c => c.Id, c => c.Name);
		var contexts = playerGames
			.Select(pg => MapGameContext(playerId, characterId, pg, characterNames))
			.ToList();

		var groupedSets = GroupIntoSets(contexts);
		return groupedSets
			.Select(BuildSeriesViewModel)
			.OrderByDescending(data => data.SortKey)
			.Select(data => data.Series)
			.ToList();
	}

	private static PlayerGameContext MapGameContext(
		long playerId,
		int characterId,
		PlayerGame playerGame,
		IReadOnlyDictionary<int, string> characterNames)
	{
		var game = playerGame.Game;
		var isPlayerA = game.PlayerAId == playerId;
		var opponentId = isPlayerA ? game.PlayerBId : game.PlayerAId;
		var opponentCharacterId = isPlayerA ? game.CharacterBId : game.CharacterAId;
		var opponentName = isPlayerA ? game.PlayerB : game.PlayerA;
		var won = isPlayerA ? game.IsPlayerAWin : !game.IsPlayerAWin;

		if (!characterNames.TryGetValue(opponentCharacterId, out var opponentCharacterName))
		{
			opponentCharacterName = "Unknown";
		}

		return new PlayerGameContext(
			playerGame,
			game,
			opponentId,
			opponentName,
			opponentCharacterId,
			opponentCharacterName,
			won,
			playerGame.EloAfter - playerGame.EloBefore,
			game.PlayedAt);
	}

	private static List<List<PlayerGameContext>> GroupIntoSets(IReadOnlyList<PlayerGameContext> gamesInOrder)
	{
		var sets = new List<List<PlayerGameContext>>();
		var currentSet = new List<PlayerGameContext> { gamesInOrder[0] };

		for (var i = 1; i < gamesInOrder.Count; i++)
		{
			var previous = gamesInOrder[i - 1];
			var current = gamesInOrder[i];

			if (current.OpponentId == previous.OpponentId &&
			    current.OpponentCharacterId == previous.OpponentCharacterId)
			{
				currentSet.Add(current);
				continue;
			}

			sets.Add(currentSet);
			currentSet = [current];
		}

		sets.Add(currentSet);
		return sets;
	}

	private static SeriesViewModelData BuildSeriesViewModel(List<PlayerGameContext> setGames)
	{
		var firstGame = setGames[0];
		var wins = setGames.Count(g => g.Won);
		var losses = setGames.Count - wins;
		var totalRatingDelta = setGames.Sum(g => g.RatingDelta);
		var (ratingDeltaText, ratingDeltaCss) = FormatRatingDelta(totalRatingDelta);
		var scoreCss = wins > losses
			? "text-success"
			: wins < losses
				? "text-danger"
				: "text-warning";

		var gameRows = setGames
			.Select((game, index) => new GameResultRow(
				index + 1,
				game.Won ? "Win" : "Loss",
				game.Won ? "text-success" : "text-danger",
				AsUtc(game.PlayedAt)))
			.ToList();

		return new SeriesViewModelData(
			AsUtc(firstGame.PlayedAt),
			new MatchSeriesViewModel(
				AsUtc(firstGame.PlayedAt),
				firstGame.OpponentName,
				firstGame.OpponentId.ToString(),
				firstGame.OpponentCharacterName.ToLowerInvariant(),
				firstGame.OpponentCharacterName,
				$"{wins} : {losses}",
				scoreCss,
				ratingDeltaText,
				ratingDeltaCss,
				gameRows));
	}

	private async Task<int> GetCharacterRankAsync(
		int characterId,
		PlayerCharacterStat activeStat,
		CancellationToken cancellationToken) =>
		await GetRankAsync(activeStat, characterId, cancellationToken);

	private async Task<int> GetOverallRankAsync(
		PlayerCharacterStat activeStat,
		CancellationToken cancellationToken) =>
		await GetRankAsync(activeStat, null, cancellationToken);

	private async Task<int> GetRankAsync(
		PlayerCharacterStat activeStat,
		int? characterId,
		CancellationToken cancellationToken)
	{
		var rating = activeStat.PlayerRating.CurrentRating;
		var query = _dbContext.PlayersCharactersStats.AsNoTracking();

		if (characterId.HasValue)
		{
			query = query.Where(stat => stat.CharacterId == characterId.Value);
		}

		var higherRanked = await query.CountAsync(stat =>
			stat.PlayerRating.CurrentRating > rating ||
			(stat.PlayerRating.CurrentRating == rating && stat.PlayerId < activeStat.PlayerId) ||
			(stat.PlayerRating.CurrentRating == rating &&
			 stat.PlayerId == activeStat.PlayerId &&
			 stat.CharacterId < activeStat.CharacterId),
			cancellationToken);

		return higherRanked + 1;
	}

	private static (string Text, string Css) FormatRatingDelta(double delta)
	{
		var rounded = (int)Math.Round(delta, MidpointRounding.AwayFromZero);
		if (rounded > 0)
		{
			return ($"+{rounded}", "text-success");
		}

		if (rounded < 0)
		{
			return ($"−{Math.Abs(rounded)}", "text-danger");
		}

		return ("0", "text-muted");
	}

	private static DateTime AsUtc(DateTime dateTime) =>
		dateTime.Kind == DateTimeKind.Utc
			? dateTime
			: DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

	private sealed record PlayerGameContext(
		PlayerGame PlayerGame,
		Game Game,
		long OpponentId,
		string OpponentName,
		int OpponentCharacterId,
		string OpponentCharacterName,
		bool Won,
		double RatingDelta,
		DateTime PlayedAt);

	private sealed record SeriesViewModelData(DateTime SortKey, MatchSeriesViewModel Series);
}
