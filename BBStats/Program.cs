using BBStats.Configuration;
using BBStats.Data;
using BBStats.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("games-fetcher.json", optional: false, reloadOnChange: true);

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
app.UseOutputCache();
app.UseAuthorization();
app.MapRazorPages();

app.Run();

