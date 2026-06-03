using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BBStats.Pages;

/// <summary>Legacy home page markup — not linked in navigation; returns 404.</summary>
public class IndexModel : PageModel
{
    public IActionResult OnGet() => NotFound();
}
