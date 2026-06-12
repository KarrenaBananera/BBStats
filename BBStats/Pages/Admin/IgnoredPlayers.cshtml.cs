using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BBStats.Data;
using BBStats.Data.Entites;


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
    {   
        IgnoredPlayers = await _dbContext.IgnoredPlayers.ToListAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteIgnoredPlayerAsync(Int64 id)
    {
        
        var ignoredPlayer = await _dbContext.IgnoredPlayers
            .FirstOrDefaultAsync(x => x.PlayerId == id);

        if (ignoredPlayer == null)   
            return NotFound();

        _dbContext.IgnoredPlayers.Remove(ignoredPlayer);
        await _dbContext.SaveChangesAsync();
        return RedirectToPage();
    }

}