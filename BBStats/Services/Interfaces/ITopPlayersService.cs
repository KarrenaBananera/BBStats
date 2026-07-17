using BBStats.Models.UI;

namespace BBStats.Services.Interfaces;

public interface ITopPlayersService
{
	Task<TopPageViewModel> GetPageAsync(
		int pageNumber,
		int? characterId = null,
		bool isAllTime = false,
		CancellationToken cancellationToken = default);
}
