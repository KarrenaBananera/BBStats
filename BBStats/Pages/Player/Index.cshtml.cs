using BBStats.Models.UI;
using BBStats.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BBStats.Pages.Player;

[ResponseCache(Duration = 300)]
public class IndexModel : PageModel
{
	private readonly IPlayerProfileService _playerProfileService;

	public IndexModel(IPlayerProfileService playerProfileService)
	{
		_playerProfileService = playerProfileService;
	}

	public PlayerProfilePageViewModel Profile { get; private set; } = new();

	[BindProperty(SupportsGet = true)]
	public string SteamId { get; set; } = "";

	[BindProperty(SupportsGet = true)]
	public string Character { get; set; } = "";

	[BindProperty(SupportsGet = true)]
	public int? MatchPage { get; set; }

	public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
	{
		if (!long.TryParse(SteamId, out var playerId))
		{
			return NotFound();
		}

		var page = MatchPage is null or < 1 ? 1 : MatchPage.Value;
		var result = await _playerProfileService.GetProfileAsync(
			playerId,
			Character,
			page,
			cancellationToken);

		if (result.RedirectCharacterSlug is not null)
		{
			return RedirectToPage(new { steamId = SteamId, character = result.RedirectCharacterSlug });
		}

		if (result.Profile is null)
		{
			return NotFound();
		}

		if (page > result.Profile.TotalPages && result.Profile.TotalPages > 0)
		{
			return RedirectToPage(new
			{
				steamId = SteamId,
				character = result.Profile.CharacterSlug,
				matchPage = result.Profile.TotalPages
			});
		}

		Profile = result.Profile;
		return Page();
	}
}
