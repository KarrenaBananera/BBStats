using BBStats.Configuration;
using BBStats.Data;
using BBStats.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("games-fetcher.json", optional: false, reloadOnChange: true);

builder.Services.AddResponseCaching();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddPageRoute("/Top/Index", "Top");
    options.Conventions.AddPageRoute("/Top/Index", "Top/Index");
});
builder.Services.AddSingleton<IGamesParser, FilteredGamesParser>();
builder.Services.AddTransient<IGamesRepository,GameRepository>();
builder.Services.AddScoped<ICharacterStatisticsService, CharacterStatisticsService>();
builder.Services.AddScoped<ITopPlayersService, TopPlayersService>();
builder.Services.AddScoped<IPlayerProfileService, PlayerProfileService>();
builder.Services.AddScoped<IPlayerSearchService, PlayerSearchService>();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
	options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
	options.UseSqlServer(connectionString);
});

builder.Services.Configure<GamesFetcherOptions>(
	builder.Configuration.GetSection(GamesFetcherOptions.SectionName));

if (string.IsNullOrWhiteSpace(builder.Configuration[$"{GamesFetcherOptions.SectionName}:BaseUrl"]))
{
	throw new InvalidOperationException(
		"GamesFetcher:BaseUrl is not configured. Set it in games-fetcher.json.");
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseResponseCaching();
app.UseAuthorization();
app.MapRazorPages();

app.Run();

