using BBStats.Data;
using BBStats.Models.UI;
using Microsoft.EntityFrameworkCore;

namespace BBStats.Services;

public class TopPlayersService : ITopPlayersService
{
	private readonly AppDbContext _dbContext;

	public TopPlayersService(AppDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<TopPageViewModel> GetPageAsync(
		int pageNumber,
		int? characterId = null,
		CancellationToken cancellationToken = default)
	{
		pageNumber = Math.Max(1, pageNumber);

		var query = _dbContext.PlayersCharactersStats
			.AsNoTracking()
			.Include(stat => stat.Player)
			.Include(stat => stat.Character)
			.AsQueryable();

		if (characterId.HasValue)
		{
			query = query.Where(stat => stat.CharacterId == characterId.Value);
		}

		query = query
			.OrderByDescending(stat => stat.PlayerRating.CurrentRating)
			.ThenBy(stat => stat.PlayerId)
			.ThenBy(stat => stat.CharacterId);

		var totalEntries = await query.CountAsync(cancellationToken);
		var totalPages = totalEntries == 0
			? 1
			: (int)Math.Ceiling(totalEntries / (double)TopPageViewModel.PageSize);

		if (pageNumber > totalPages)
		{
			pageNumber = totalPages;
		}

		var stats = await query
			.Skip((pageNumber - 1) * TopPageViewModel.PageSize)
			.Take(TopPageViewModel.PageSize)
			.ToListAsync(cancellationToken);

		var startRank = (pageNumber - 1) * TopPageViewModel.PageSize + 1;
		var rows = stats
			.Select((stat, index) => new TopPlayerRowViewModel(
				startRank + index,
				stat.Player.Name,
				stat.PlayerId.ToString(),
				stat.Character.Name,
				stat.Character.Name.ToLowerInvariant(),
				(int)Math.Round(stat.PlayerRating.CurrentRating)))
			.ToList();

		return new TopPageViewModel
		{
			PageNumber = pageNumber,
			TotalPages = totalPages,
			TotalEntries = totalEntries,
			Rows = rows
		};
	}
}
