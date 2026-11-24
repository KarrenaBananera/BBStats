using BBStats.Models.UI;
using BBStats.Data;
using BBStats.Data.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OutputCaching;
using BBStats.Services.Interfaces;

namespace BBStats.Pages.Player;

[OutputCache(
	Duration = 180,
	VaryByRouteValueNames = ["steamId", "character", "matchPage"],
	VaryByQueryKeys = ["handler", "minElo", "maxElo", "opponent", "opponentCharacter"])]
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


    return RedirectToPage(ProfileRoute(Character, CurrentMatchPage));
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
        
        return RedirectToPage(ProfileRoute(Character, CurrentMatchPage));
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

	[BindProperty(SupportsGet = true)]
	public int? MinElo { get; set; }

	[BindProperty(SupportsGet = true)]
	public int? MaxElo { get; set; }

	[BindProperty(SupportsGet = true)]
	public string? Opponent { get; set; }

	[BindProperty(SupportsGet = true)]
	public string? OpponentCharacter { get; set; }

	private int? CurrentMatchPage => MatchPage is null or < 1 ? null : MatchPage;

	private MatchSeriesFilter CurrentSeriesFilter() => new()
	{
		MinOpponentElo = MinElo,
		MaxOpponentElo = MaxElo,
		OpponentName = Opponent,
		OpponentCharacterSlug = OpponentCharacter
	};

	private object ProfileRoute(string? character, int? matchPage)
	{
		var filter = CurrentSeriesFilter().Normalize();
		return new
		{
			steamId = SteamId,
			character,
			matchPage,
			minElo = filter.MinOpponentElo,
			maxElo = filter.MaxOpponentElo,
			opponent = filter.OpponentName,
			opponentCharacter = filter.OpponentCharacterSlug
		};
	}

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
			CurrentSeriesFilter(),
			cancellationToken);

		if (result.RedirectCharacterSlug is not null)
		{
			return RedirectToPage(ProfileRoute(result.RedirectCharacterSlug, null));
		}

		if (result.Profile is null)
		{
			return NotFound();
		}

		if (page > result.Profile.TotalPages && result.Profile.TotalPages > 0)
		{
			return RedirectToPage(ProfileRoute(
				result.Profile.CharacterSlug,
				result.Profile.TotalPages <= 1 ? null : result.Profile.TotalPages));
		}

		Profile = result.Profile;
		
		Profile.IsIgnored = await _dbContext.IgnoredPlayers
			.IgnoreQueryFilters()
			.AnyAsync(x => x.PlayerId == playerId, cancellationToken: cancellationToken);

		return Page();
	}

	public async Task<IActionResult> OnGetMatchesAsync(CancellationToken cancellationToken)
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
			true,
			CurrentSeriesFilter(),
			cancellationToken);

		if (result.Profile is null)
		{
			return NotFound();
		}

		Response.Headers["X-Robots-Tag"] = "noindex";
		return Partial("Profile/_MatchesSection", result.Profile);
	}
}
