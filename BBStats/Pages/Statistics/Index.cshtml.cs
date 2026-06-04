using BBStats.Data;
using BBStats.Models.UI;
using BBStats.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OutputCaching;

namespace BBStats.Pages.Statistics;

[OutputCache(Duration = 3600)]
public class IndexModel : PageModel
{
	private readonly ICharacterStatisticsService _characterStatisticsService;

	public IndexModel(ICharacterStatisticsService characterStatisticsService)
	{
		_characterStatisticsService = characterStatisticsService;
	}

	[BindProperty(SupportsGet = true)]
	public string? Character { get; set; }

	public CharacterStatisticsViewModel Stats { get; private set; } = new();

	public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
	{
		var slug = Character?.Trim().ToLowerInvariant();
		if (string.IsNullOrWhiteSpace(slug) || !CharactersSeed.CharactersNames.TryGetValue(slug, out var characterId))
		{
			return RedirectToPage(new { character = "ragna" });
		}

		var statistics = await _characterStatisticsService.GetStatisticsAsync(characterId, cancellationToken);
		if (statistics is null)
		{
			return NotFound();
		}

		Stats = statistics;
		ViewData["Title"] = $"{Stats.DisplayName} Statistics";
		return Page();
	}
}
