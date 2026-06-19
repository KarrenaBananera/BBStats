using BBStats.Data;
using BBStats.Models.UI;
using Microsoft.EntityFrameworkCore;

namespace BBStats.Services;

public class PlayerSearchService : IPlayerSearchService
{
	private const int MaxResults = 100;
	private readonly AppDbContext _dbContext;

	public PlayerSearchService(AppDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public Task<bool> PlayerExistsAsync(long steamId, CancellationToken cancellationToken = default) =>
		_dbContext.Players.IgnoreQueryFilters().AsNoTracking().AnyAsync(p => p.Id == steamId, cancellationToken);

	public async Task<string> GetDefaultCharacterSlugAsync(
		long steamId,
		CancellationToken cancellationToken = default)
	{
		var topCharacter = await _dbContext.PlayersCharactersStats
			.IgnoreQueryFilters()
			.AsNoTracking()
			.Include(stat => stat.Character)
			.Where(stat => stat.PlayerId == steamId)
			.OrderByDescending(stat => stat.Wins + stat.Losses)
			.ThenBy(stat => stat.CharacterId)
			.Select(stat => stat.Character.Name)
			.FirstOrDefaultAsync(cancellationToken);

		return topCharacter?.ToLowerInvariant() ?? "ragna";
	}

	public async Task<IReadOnlyList<PlayerSearchResultItem>> SearchByNameAsync(
		string query,
		bool includeIgnored, 
		CancellationToken cancellationToken = default)
	{
		var trimmedQuery = query.Trim();
		if (string.IsNullOrWhiteSpace(trimmedQuery))
		{
			return [];
		}
		
		var players = includeIgnored
			? await _dbContext.Players.IgnoreQueryFilters()
				.AsNoTracking()
				.Where(player => EF.Functions.Like(player.Name, $"%{trimmedQuery}%"))
				.OrderBy(player => player.Name)
				.Take(MaxResults)
				.ToListAsync(cancellationToken)
			: await _dbContext.Players
				.AsNoTracking()
				.Where(player => EF.Functions.Like(player.Name, $"%{trimmedQuery}%"))
				.OrderBy(player => player.Name)
				.Take(MaxResults)
				.ToListAsync(cancellationToken);

		if (players.Count == 0)
		{
			return [];
		}

		var playerIds = players.Select(player => player.Id).ToList();
		var stats = await _dbContext.PlayersCharactersStats
			.AsNoTracking()
			.Include(stat => stat.Character)
			.Where(stat => playerIds.Contains(stat.PlayerId))
			.ToListAsync(cancellationToken);

		var characterSlugByPlayerId = stats
			.GroupBy(stat => stat.PlayerId)
			.ToDictionary(
				group => group.Key,
				group => group
					.OrderByDescending(stat => stat.Wins + stat.Losses)
					.ThenBy(stat => stat.CharacterId)
					.First()
					.Character.Name.ToLowerInvariant());

		return players
			.Select(player => new PlayerSearchResultItem(
				player.Id.ToString(),
				player.Name,
				characterSlugByPlayerId.GetValueOrDefault(player.Id, "ragna")))
			.ToList();
	}
}
