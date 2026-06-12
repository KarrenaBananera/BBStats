using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BBStats.Data;
using BBStats.Data.Entites;


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
    {   
        Players = await _dbContext.Players.ToListAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostIgnorePlayerAsync(Int64 id)
    {

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

}