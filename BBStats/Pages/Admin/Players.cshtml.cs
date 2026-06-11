using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BBStats.Data;
using BBStats.Data.Entites;

// Check if the user is an admin, if not redirect to login page
[Authorize(Roles = "Admin")]
public class PlayersModel : PageModel
{
    private readonly AppDbContext _dbContext;

    public List<Player> Players { get; set; } = [];

    public PlayersModel(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> OnGetAsync()
    {   // Get all players from the database and display them on the page
        Players = await _dbContext.Players.ToListAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeletePlayerAsync(Int64 id)
    {

        // Remove the player and all their games from the database
        var player = await _dbContext.Players.FirstOrDefaultAsync(x => x.Id == id);

        if (player == null) 
        {
            return NotFound();
        }

        if (!await _dbContext.IgnoredPlayers.AnyAsync(x => x.PlayerId == player.Id))
        {
            _dbContext.IgnoredPlayers.Add(new IgnoredPlayer
            {
                PlayerId = player.Id,
                Reason = "Deleted by admin"
            });
        }
        
        _dbContext.Players.Remove(player);
        await _dbContext.SaveChangesAsync();
        return RedirectToPage();
    }

}