using BBStats.Models.UI;
using BBStats.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OutputCaching;

namespace BBStats.Pages.Player;

[OutputCache(Duration = 86400)]
public class CharacterStatsModel : PageModel
{
	private readonly IPlayerCharacterStatsService _playerCharacterStatsService;

	public CharacterStatsModel(IPlayerCharacterStatsService playerCharacterStatsService)
	{
		_playerCharacterStatsService = playerCharacterStatsService;
	}

	public PlayerCharacterStatsViewModel Stats { get; private set; } = new();

	[BindProperty(SupportsGet = true)]
	public string SteamId { get; set; } = "";

	[BindProperty(SupportsGet = true)]
	public string Character { get; set; } = "";

	public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
	{
		if (!long.TryParse(SteamId, out var playerId))
		{
			return NotFound();
		}

		var stats = await _playerCharacterStatsService.GetStatsAsync(
			playerId,
			Character,
			cancellationToken);

		if (stats is null)
		{
			return NotFound();
		}

		Stats = stats;
		return Page();
	}
}
