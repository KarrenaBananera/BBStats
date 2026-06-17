using BBStats.Models.UI;
using BBStats.Services;
using BBStats.Data;
using BBStats.Data.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OutputCaching;

namespace BBStats.Pages.Player;

[OutputCache(Duration = 180)]
public class IndexModel : PageModel
{
	private readonly AppDbContext _dbContext;
	private readonly IPlayerProfileService _playerProfileService;

	public async Task<IActionResult> OnPostDeleteIgnoredPlayerAsync(Int64 id)
{
	if (User.Identity?.IsAuthenticated != true || !User.IsInRole("Admin"))
	{
		return Forbid();
	}

    var ignoredPlayer = await _dbContext.IgnoredPlayers
        .FirstOrDefaultAsync(x => x.PlayerId == id);

	if (ignoredPlayer == null)   
		return NotFound();

	_dbContext.IgnoredPlayers.Remove(ignoredPlayer);
	await _dbContext.SaveChangesAsync();


    return RedirectToPage(new
    {
        steamId = id,
        character = Character,
        matchPage = MatchPage
    });
}

    public async Task<IActionResult> OnPostIgnorePlayerAsync(Int64 id)
    {

		if (User.Identity?.IsAuthenticated != true || !User.IsInRole("Admin"))
		{
			return Forbid();
		}

        var player = await _dbContext.Players
            .FirstOrDefaultAsync(x => x.Id == id);

        if (player == null) 
            return NotFound();

        if (!await _dbContext.IgnoredPlayers.AnyAsync(x => x.PlayerId == player.Id))
        {
            _dbContext.IgnoredPlayers.Add(new IgnoredPlayer
            {
                PlayerId = player.Id,
                Reason = "Ignored by admin"
            });
            await _dbContext.SaveChangesAsync();
        }
        
        return RedirectToPage();
    }

	public IndexModel(IPlayerProfileService playerProfileService, AppDbContext dbContext)
	{
		_playerProfileService = playerProfileService;
		_dbContext = dbContext;
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

		var includeIgnored = true;

		var result = await _playerProfileService.GetProfileAsync(
			playerId,
			Character,
			page,
			includeIgnored,
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
		
		Profile.IsIgnored = await _dbContext.IgnoredPlayers
			.IgnoreQueryFilters()
			.AnyAsync(x => x.PlayerId == playerId, cancellationToken: cancellationToken);

		return Page();
	}
}
