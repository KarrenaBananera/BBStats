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

	[BindProperty(SupportsGet = true, Name = "page")]
	public int PageNumber { get; set; } = 1;

	public TopPageViewModel Top { get; private set; } = new();

	public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
	{
		if (PageNumber < 1)
		{
			return RedirectToPage(new { page = 1 });
		}

		Top = await _topPlayersService.GetPageAsync(PageNumber, cancellationToken: cancellationToken);

		if (PageNumber > Top.TotalPages && Top.TotalPages > 0)
		{
			return RedirectToPage(new { page = Top.TotalPages });
		}

		return Page();
	}

	public string? GetPageUrl(int page) =>
		page <= 1 ? Url.Page("/Top/Index") : Url.Page("/Top/Index", new { page = page });
}
