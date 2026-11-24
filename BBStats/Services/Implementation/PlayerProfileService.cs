using BBStats.Configuration;
using BBStats.Data;
using BBStats.Data.Entites;
using BBStats.Models.UI;
using BBStats.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BBStats.Services.Implementation;

public class PlayerProfileService : IPlayerProfileService
{
	private const int SetsPerPage = 10;
	private static readonly TimeSpan MaxSetGap = TimeSpan.FromMinutes(20);

	private readonly AppDbContext _dbContext;
	private readonly string _urlForFront;

	public PlayerProfileService(AppDbContext dbContext, IOptions<GamesFetcherOptions> gamesFetcherOptions)
	{
		_dbContext = dbContext;
		_urlForFront = gamesFetcherOptions.Value.UrlForFront;
	}

	public async Task<PlayerProfileResult> GetProfileAsync(
		long playerId,
		string characterSlug,
		int pageNumber,
		bool includeIgnored = false,
		MatchSeriesFilter? seriesFilter = null,
		CancellationToken cancellationToken = default)
	{
		pageNumber = Math.Max(1, pageNumber);
		var slug = characterSlug.Trim().ToLowerInvariant();
		var filter = (seriesFilter ?? MatchSeriesFilter.None).Normalize();

		var isIgnored = await _dbContext.IgnoredPlayers
			.AnyAsync(x => x.PlayerId == playerId, cancellationToken);

		
		var player = includeIgnored
			? await _dbContext.Players.IgnoreQueryFilters()
				.AsNoTracking()
				.FirstOrDefaultAsync(p => p.Id == playerId, cancellationToken)
			: await _dbContext.Players
				.AsNoTracking()
				.FirstOrDefaultAsync(p => p.Id == playerId, cancellationToken);

		if (player is null)
		{
			return PlayerProfileResult.NotFound();
		}

		var characterStats = includeIgnored
			? await _dbContext.PlayersCharactersStats.IgnoreQueryFilters()
				.AsNoTracking()
				.Include(stat => stat.Character)
				.Where(stat => stat.PlayerId == playerId)
				.OrderByDescending(stat => stat.Wins + stat.Losses)
				.ThenBy(stat => stat.Character.Name)
				.ToListAsync(cancellationToken)
			: await _dbContext.PlayersCharactersStats
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
				EmptyMessage = "No recorded matches for this player yet.",
				SeriesFilter = filter
			});
		}

		if (!CharactersSeed.CharactersNames.TryGetValue(slug, out var characterId) ||
		    characterStats.All(stat => stat.CharacterId != characterId))
		{
			return PlayerProfileResult.RedirectToCharacter(
				characterStats[0].Character.Name.ToLowerInvariant());
		}

		var activeStat = characterStats.First(stat => stat.CharacterId == characterId);
		var allSets = await BuildMatchSeriesAsync(playerId, characterId, filter, cancellationToken);
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
				(int)Math.Round(stat.PlayerRating.RatingDeviation) * 2,
				stat.CharacterId == characterId))
			.ToList();

		var characterRank = await GetCharacterRankAsync(characterId, activeStat, cancellationToken);
		var overallRank = await GetOverallRankAsync(activeStat, cancellationToken);
		var (winratePercent, winrateCss) = FormatWinrate(activeStat.Wins, activeStat.Losses);

		return PlayerProfileResult.Found(new PlayerProfilePageViewModel
		{
			PlayerName = player.Name,
			SteamId = playerId.ToString(),
			CharacterSlug = slug,
			CharacterDisplayName = activeStat.Character.Name,
			OverallRank = isIgnored || activeStat.PlayerRating.RatingDeviation > 130 ? 0 : overallRank,
			CharacterRank = isIgnored || activeStat.PlayerRating.RatingDeviation > 130 ? 0 : characterRank,
			Rating = isIgnored ? 0 : (int)Math.Round(activeStat.PlayerRating.CurrentRating),
			RatingDeviation = isIgnored ? 0 : (int)Math.Round(activeStat.PlayerRating.RatingDeviation) * 2,
			Wins = activeStat.Wins,
			Losses = activeStat.Losses,
			WinratePercent = winratePercent,
			WinrateCss = winrateCss,
			Characters = sidebar,
			Series = pageSets,
			CurrentPage = pageNumber,
			TotalPages = totalPages,
			EmptyMessage = allSets.Count == 0
				? filter.IsActive
					? "No series match these filters."
					: "No recorded series for this character."
				: null,
			SeriesFilter = filter
		});
	}

	private async Task<List<MatchSeriesViewModel>> BuildMatchSeriesAsync(
		long playerId,
		int characterId,
		MatchSeriesFilter filter,
		CancellationToken cancellationToken)
	{
		var isIgnored = await _dbContext.IgnoredPlayers
			.AnyAsync(x => x.PlayerId == playerId, cancellationToken);

		int? opponentCharacterId = null;
		if (filter.HasOpponentCharacter)
		{
			if (!CharactersSeed.CharactersNames.TryGetValue(filter.OpponentCharacterSlug!, out var parsedCharacterId))
			{
				return [];
			}

			opponentCharacterId = parsedCharacterId;
		}

		var characterNames = CharactersSeed.All.ToDictionary(c => c.Id, c => c.Name);
		var contexts = await LoadGameContextsAsync(
			playerId,
			characterId,
			filter,
			opponentCharacterId,
			characterNames,
			cancellationToken);

		if (contexts.Count == 0)
		{
			return [];
		}

		var groupedSets = GroupIntoSets(contexts);
		return groupedSets
			.Select(set => BuildSeriesViewModel(set, isIgnored))
			.OrderByDescending(data => data.SortKey)
			.Select(data => data.Series)
			.ToList();
	}

	private async Task<List<PlayerGameContext>> LoadGameContextsAsync(
		long playerId,
		int characterId,
		MatchSeriesFilter filter,
		int? opponentCharacterId,
		IReadOnlyDictionary<int, string> characterNames,
		CancellationToken cancellationToken)
	{
		var query = _dbContext.PlayersGames
			.IgnoreQueryFilters()
			.AsNoTracking()
			.Where(pg => pg.PlayerId == playerId && pg.CharacterId == characterId);

		query = ApplyCharacterFilter(query, playerId, opponentCharacterId);

		if (filter.HasOpponentName)
		{
			var opponentIds = await FindPlayerIdsByCurrentNameAsync(filter.OpponentName!, cancellationToken);
			if (opponentIds.Count == 0)
			{
				return [];
			}

			query = query.Where(pg =>
				(pg.Game.PlayerAId == playerId && opponentIds.Contains(pg.Game.PlayerBId)) ||
				(pg.Game.PlayerBId == playerId && opponentIds.Contains(pg.Game.PlayerAId)));
		}

		query = ApplyCurrentEloFilter(query, playerId, filter);

		var playerGames = await query
			.Include(pg => pg.Game)
			.OrderBy(pg => pg.Game.PlayedAt)
			.ThenBy(pg => pg.GameId)
			.ToListAsync(cancellationToken);

		return playerGames
			.Select(pg => MapGameContext(playerId, pg, pg.Game, characterNames))
			.ToList();
	}

	private async Task<List<long>> FindPlayerIdsByCurrentNameAsync(
		string opponentName,
		CancellationToken cancellationToken)
	{
		var pattern = "%" + EscapeLike(opponentName) + "%";
		return await _dbContext.Players
			.IgnoreQueryFilters()
			.AsNoTracking()
			.Where(player => EF.Functions.Like(player.Name, pattern))
			.Select(player => player.Id)
			.ToListAsync(cancellationToken);
	}

	private static IQueryable<PlayerGame> ApplyCharacterFilter(
		IQueryable<PlayerGame> query,
		long playerId,
		int? opponentCharacterId)
	{
		if (opponentCharacterId is not int characterFilterId)
		{
			return query;
		}

		return query.Where(pg =>
			(pg.Game.PlayerAId == playerId && pg.Game.CharacterBId == characterFilterId) ||
			(pg.Game.PlayerBId == playerId && pg.Game.CharacterAId == characterFilterId));
	}

	private IQueryable<PlayerGame> ApplyCurrentEloFilter(
		IQueryable<PlayerGame> query,
		long playerId,
		MatchSeriesFilter filter)
	{
		if (!filter.HasEloBounds)
		{
			return query;
		}

		var minElo = filter.MinOpponentElo ?? int.MinValue;
		var maxElo = filter.MaxOpponentElo ?? int.MaxValue;
		var stats = _dbContext.PlayersCharactersStats.IgnoreQueryFilters().AsNoTracking();

		return query.Where(pg =>
			stats.Any(stat =>
				((pg.Game.PlayerAId == playerId &&
				  stat.PlayerId == pg.Game.PlayerBId &&
				  stat.CharacterId == pg.Game.CharacterBId) ||
				 (pg.Game.PlayerBId == playerId &&
				  stat.PlayerId == pg.Game.PlayerAId &&
				  stat.CharacterId == pg.Game.CharacterAId)) &&
				(int)Math.Round(stat.PlayerRating.CurrentRating) >= minElo &&
				(int)Math.Round(stat.PlayerRating.CurrentRating) <= maxElo));
	}

	private static string EscapeLike(string value) =>
		value.Replace("[", "[[]", StringComparison.Ordinal)
			.Replace("%", "[%]", StringComparison.Ordinal)
			.Replace("_", "[_]", StringComparison.Ordinal);

	private static PlayerGameContext MapGameContext(
		long playerId,
		PlayerGame playerGame,
		Game game,
		IReadOnlyDictionary<int, string> characterNames)
	{
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

	private static bool BelongsToSameSet(PlayerGameContext previous, PlayerGameContext current)
	{
		if (current.OpponentId != previous.OpponentId ||
		    current.OpponentCharacterId != previous.OpponentCharacterId)
		{
			return false;
		}

		var gap = current.PlayedAt - previous.PlayedAt;
		return gap >= TimeSpan.Zero && gap < MaxSetGap;
	}

	private static List<List<PlayerGameContext>> GroupIntoSets(IReadOnlyList<PlayerGameContext> gamesInOrder)
	{
		var sets = new List<List<PlayerGameContext>>();
		var currentSet = new List<PlayerGameContext> { gamesInOrder[0] };

		for (var i = 1; i < gamesInOrder.Count; i++)
		{
			var previous = gamesInOrder[i - 1];
			var current = gamesInOrder[i];

			if (BelongsToSameSet(previous, current))
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

	private SeriesViewModelData BuildSeriesViewModel(List<PlayerGameContext> setGames, bool isIgnored)
	{
		var firstGame = setGames[0];
		var wins = setGames.Count(g => g.Won);
		var losses = setGames.Count - wins;
		var totalRatingDelta = setGames.Sum(g => g.RatingDelta);
		var (ratingDeltaText, ratingDeltaCss) = FormatRatingDelta(totalRatingDelta, isIgnored);
		var scoreCss = wins > losses
			? "text-success"
			: wins < losses
				? "text-danger"
				: "text-warning";

		var gameRows = setGames
			.Select((game, index) => MapGameRow(game, index + 1, isIgnored))
			.ToList();

		var downloadUrls = gameRows
			.Select(row => row.ReplayDownloadUrl)
			.Where(url => !string.IsNullOrWhiteSpace(url))
			.Cast<string>()
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
				gameRows,
				downloadUrls));
	}

	private GameResultRow MapGameRow(PlayerGameContext game, int number, bool isIgnored)
	{
		var (openUrl, downloadUrl) = BBStats.Services.Implementation.GameReplayLink.Build(game.Game.ReplayId, _urlForFront);
		var (ratingDeltaText, ratingDeltaCss) = FormatRatingDelta(game.RatingDelta, isIgnored);

		return new GameResultRow(
			number,
			game.Won ? "Win" : "Loss",
			game.Won ? "text-success" : "text-danger",
			AsUtc(game.PlayedAt),
			ratingDeltaText,
			ratingDeltaCss,
			openUrl,
			downloadUrl);
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
		var query = _dbContext.PlayersCharactersStats.AsNoTracking()
			.Where(stat => stat.PlayerRating.RatingDeviation <= 130);

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

	private static (double WinratePercent, string WinrateCss) FormatWinrate(int wins, int losses)
	{
		var total = wins + losses;
		if (total == 0)
		{
			return (0, "text-muted");
		}

		var winrate = (double)wins / total * 100;
		return (winrate, GetWinrateCss(winrate));
	}

	private static string GetWinrateCss(double winrate) => winrate switch
	{
		< 45 => "text-danger",
		> 55 => "text-success",
		_ => "text-warning"
	};

	private static (string Text, string Css) FormatRatingDelta(double delta, bool hideRating)
	{
		if (hideRating)
		{
			return ("", "");
		}

		var rounded = (int)Math.Round(delta, MidpointRounding.AwayFromZero);
		if (rounded > 0)
		{
			return ($"+{rounded}", "text-success");
		}

		if (rounded < 0)
		{
			return ($"−{Math.Abs(rounded)}", "text-danger");
		}

		return ("0", "text-white");
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
