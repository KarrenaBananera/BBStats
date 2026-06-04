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

	[BindProperty(SupportsGet = true, Name = "page")]
	public int PageNumber { get; set; } = 1;

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

		if (PageNumber < 1)
		{
			return RedirectToPage(new { character = slug, page = 1 });
		}

		Top = await _topPlayersService.GetPageAsync(PageNumber, characterId, cancellationToken);

		if (PageNumber > Top.TotalPages && Top.TotalPages > 0)
		{
			return RedirectToPage(new { character = slug, page = Top.TotalPages });
		}

		return Page();
	}

	public string? GetPageUrl(int page) =>
		page <= 1
			? Url.Page("/Characters/Index", new { character = Character })
			: Url.Page("/Characters/Index", new { character = Character, page });
}
