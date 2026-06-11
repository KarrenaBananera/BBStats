using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BBStats.Data;
using BBStats.Data.Entites;

// Check if the user is an admin, if not redirect to login page
[Authorize(Roles = "Admin")]
public class IgnoredPlayersModel : PageModel
{
    private readonly AppDbContext _dbContext;

    public List<IgnoredPlayer> IgnoredPlayers { get; set; } = [];

    public IgnoredPlayersModel(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> OnGetAsync()
    {   // Get all ignored players from the database and display them on the page
        IgnoredPlayers = await _dbContext.IgnoredPlayers.ToListAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteIgnoredPlayerAsync(Int64 id)
    {
        // Remove the ignored player from the database
        var ignoredPlayer = await _dbContext.IgnoredPlayers.FirstOrDefaultAsync(x => x.PlayerId == id);

        if (ignoredPlayer == null) 
        {
            return NotFound();
        }
        
        _dbContext.IgnoredPlayers.Remove(ignoredPlayer);
        await _dbContext.SaveChangesAsync();
        return RedirectToPage();
    }

}