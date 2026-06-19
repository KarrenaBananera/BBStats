using BBStats.Data;
using BBStats.Models.UI;
using BBStats.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OutputCaching;

namespace BBStats.Pages.Characters;

[OutputCache(Duration = 600, VaryByQueryKeys = ["page"])]
public class IndexModel : PageModel
{
	private readonly ITopPlayersService _topPlayersService;

	public IndexModel(ITopPlayersService topPlayersService)
	{
		_topPlayersService = topPlayersService;
	}

	[BindProperty(SupportsGet = true)]
	public string? Character { get; set; }

	public string CharacterDisplayName { get; private set; } = "";

	public TopPageViewModel Top { get; private set; } = new();

	public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
	{
		var slug = Character?.Trim().ToLowerInvariant();
		if (string.IsNullOrWhiteSpace(slug) || !CharactersSeed.CharactersNames.TryGetValue(slug, out var characterId))
		{
			return RedirectToPage(new { character = "ragna" });
		}

		CharacterDisplayName = CharactersSeed.All.First(c => c.Id == characterId).Name;

		var pageNumber = PaginationQuery.ReadPageNumber(Request);

		if (pageNumber < 1)
		{
			return Redirect(PageQueryUrl.WithQueryPage(Url, "/Characters/Index", 1, new { character = slug })!);
		}

		Top = await _topPlayersService.GetPageAsync(pageNumber, characterId, cancellationToken);

		if (pageNumber > Top.TotalPages && Top.TotalPages > 0)
		{
			return Redirect(PageQueryUrl.WithQueryPage(Url, "/Characters/Index", Top.TotalPages, new { character = slug })!);
		}

		return Page();
	}

	public string? GetPageUrl(int page) =>
		PageQueryUrl.WithQueryPage(Url, "/Characters/Index", page, new { character = Character });
}
