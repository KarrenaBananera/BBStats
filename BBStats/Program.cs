using BBStats.Configuration;
using BBStats.Data;
using BBStats.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies; // For cookie authentication

var builder = WebApplication.CreateBuilder(args);

var adminUsername = builder.Configuration["AdminSettings:Username"];
var adminPassword = builder.Configuration["AdminSettings:Password"];

if (string.IsNullOrWhiteSpace(adminUsername) || string.IsNullOrWhiteSpace(adminPassword) ||
    adminUsername == "enter a username here" || adminPassword == "enter a password here")
{
    throw new InvalidOperationException(
        "AdminSettings:Username and AdminSettings:Password must be configured with non-default values and be not blank.");
}

builder.Configuration.AddJsonFile("games-fetcher.json", optional: false, reloadOnChange: true);

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Login";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });


builder.Services.AddOutputCache();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddPageRoute("/Top", "Top");
    options.Conventions.AddPageRoute("/Top", "Top/Index");
});
builder.Services.AddSingleton<FilteredGamesParser>();
builder.Services.AddSingleton<IGamesParser>(sp => sp.GetRequiredService<FilteredGamesParser>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<FilteredGamesParser>());
builder.Services.AddTransient<IGamesRepository,GameRepository>();
builder.Services.AddScoped<ICharacterStatisticsService, CharacterStatisticsService>();
builder.Services.AddScoped<ITopPlayersService, TopPlayersService>();
builder.Services.AddScoped<IPlayerProfileService, PlayerProfileService>();
builder.Services.AddScoped<IPlayerCharacterStatsService, PlayerCharacterStatsService>();
builder.Services.AddScoped<IPlayerSearchService, PlayerSearchService>();
builder.Services.AddAuthorization(); // just adding it explicity

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
	throw new InvalidOperationException(
		"Connection string 'DefaultConnection' is not configured. " +
		"Set environment variable ConnectionStrings__DefaultConnection (Production) " +
		"or ConnectionStrings:DefaultConnection in appsettings.Development.json.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
	options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
	options.UseSqlServer(connectionString);
});

builder.Services.Configure<GamesFetcherOptions>(
	builder.Configuration.GetSection(GamesFetcherOptions.SectionName));

var gamesFetcherOptions = builder.Configuration
	.GetSection(GamesFetcherOptions.SectionName)
	.Get<GamesFetcherOptions>() ?? new GamesFetcherOptions();

if (gamesFetcherOptions.Enabled && string.IsNullOrWhiteSpace(gamesFetcherOptions.BaseUrl))
{
	throw new InvalidOperationException(
		"GamesFetcher:BaseUrl is not configured. Set it in games-fetcher.json.");
}

if (string.IsNullOrWhiteSpace(gamesFetcherOptions.UrlForFront))
{
	throw new InvalidOperationException(
		"GamesFetcher:UrlForFront is not configured. Set it in games-fetcher.json.");
}

if (gamesFetcherOptions.FetchIntervalSeconds < 1)
{
	throw new InvalidOperationException(
		"GamesFetcher:FetchIntervalSeconds must be at least 1.");
}

builder.Services.AddHttpClient<GamesFetcherClient>(client =>
{
	client.Timeout = TimeSpan.FromSeconds(120);
});

builder.Services.AddHostedService<GamesProcessingService>();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = StaticFileCacheHeaders.ApplyLongTermImageCache
});
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();
app.MapRazorPages();

app.Run();

