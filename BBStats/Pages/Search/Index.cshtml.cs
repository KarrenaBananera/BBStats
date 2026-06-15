using BBStats.Models.UI;
using BBStats.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BBStats.Pages.Search;

public class IndexModel : PageModel
{
	private readonly IPlayerSearchService _playerSearchService;

	public IndexModel(IPlayerSearchService playerSearchService)
	{
		_playerSearchService = playerSearchService;
	}

	[BindProperty(SupportsGet = true, Name = "q")]
	public string? Query { get; set; }

	public PlayerSearchPageViewModel Search { get; private set; } = new();

	public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
	{
		var query = Query?.Trim() ?? "";
		if (string.IsNullOrWhiteSpace(query))
		{
			Search = new PlayerSearchPageViewModel();
			return Page();
		}

		if (SteamId.TryParse(query, out var steamId))
		{
			if (!await _playerSearchService.PlayerExistsAsync(steamId, cancellationToken))
			{
				Search = new PlayerSearchPageViewModel
				{
					Query = query,
					HasSearched = true,
					Message = "Player with this Steam ID was not found."
				};
				return Page();
			}

			var characterSlug = await _playerSearchService.GetDefaultCharacterSlugAsync(
				steamId,
				cancellationToken);

			return RedirectToPage("/Player/Index", new
			{
				steamId = steamId.ToString(),
				character = characterSlug
			});
		}

		var includeIgnored = User.Identity?.IsAuthenticated == true && User.IsInRole("Admin");

		var results = await _playerSearchService.SearchByNameAsync(query, includeIgnored, cancellationToken);
		Search = new PlayerSearchPageViewModel
		{
			Query = query,
			Results = results,
			HasSearched = true,
			Message = results.Count == 0 ? "No players matched your search." : null
		};

		return Page();
	}
}
