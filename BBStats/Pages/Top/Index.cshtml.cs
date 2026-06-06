using BBStats.Models.UI;
using BBStats.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OutputCaching;

namespace BBStats.Pages.Top;

[OutputCache(Duration = 600, VaryByQueryKeys = ["page"])]
public class IndexModel : PageModel
{
	private readonly ITopPlayersService _topPlayersService;

	public IndexModel(ITopPlayersService topPlayersService)
	{
		_topPlayersService = topPlayersService;
	}

	public TopPageViewModel Top { get; private set; } = new();

	public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
	{
		var pageNumber = PaginationQuery.ReadPageNumber(Request);

		if (pageNumber < 1)
		{
			return Redirect(PageQueryUrl.WithQueryPage(Url, "/Top/Index", 1)!);
		}

		Top = await _topPlayersService.GetPageAsync(pageNumber, cancellationToken: cancellationToken);

		if (pageNumber > Top.TotalPages && Top.TotalPages > 0)
		{
			return Redirect(PageQueryUrl.WithQueryPage(Url, "/Top/Index", Top.TotalPages)!);
		}

		return Page();
	}

	public string? GetPageUrl(int page) =>
		PageQueryUrl.WithQueryPage(Url, "/Top/Index", page);
}
