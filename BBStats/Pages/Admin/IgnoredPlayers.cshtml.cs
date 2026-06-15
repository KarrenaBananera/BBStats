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
    private const int PageSize = 10;

    public List<IgnoredPlayer> IgnoredPlayers { get; set; } = [];
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public int TotalPages { get; set; }

    public IgnoredPlayersModel(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task OnGetAsync()
    {
        if (PageNumber < 1)
        {
            PageNumber = 1;
        }

        var totalCount = await _dbContext.IgnoredPlayers.CountAsync();

        TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

        IgnoredPlayers = await _dbContext.IgnoredPlayers
            .OrderByDescending(x => x.CreatedAt)
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

    }

    public async Task<IActionResult> OnPostDeleteIgnoredPlayerAsync(Int64 id)
    {
        
        var ignoredPlayer = await _dbContext.IgnoredPlayers
            .FirstOrDefaultAsync(x => x.PlayerId == id);

        if (ignoredPlayer == null)   
            return NotFound();

        _dbContext.IgnoredPlayers.Remove(ignoredPlayer);
        await _dbContext.SaveChangesAsync();
        return RedirectToPage(new { pageNumber = PageNumber });
    }

}