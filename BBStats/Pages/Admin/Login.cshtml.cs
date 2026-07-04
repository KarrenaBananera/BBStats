using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

public class LoginModel : PageModel
{
    private readonly IConfiguration _configuration;

    [BindProperty]
    public string Username { get; set; } = string.Empty;
    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public LoginModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void OnGet()
    {
    }

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {  
        var adminUsername = _configuration["AdminSettings:Username"];
        var adminPassword = _configuration["AdminSettings:Password"];

        if (Username == adminUsername && Password == adminPassword)
        {
            // If valid, create the admin role and sign in the user using cookie authentication
                var claims = new List<Claim>
            {
                new(ClaimTypes.Name, Username),
                new(ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);
            
            return RedirectToPage("/Top/Index");
            }
        else
        {

            ErrorMessage = "Invalid username or password.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostLogoutAsync()
    {   // Sign out the user and clear the authentication cookie
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return Redirect("/");
    }

}