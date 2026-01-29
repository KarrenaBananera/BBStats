using BBStats.Data;
using BBStats.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
	if (builder.Environment.IsDevelopment())
	{
	}
	options.UseSqlite(connectionString);

});

builder.Services.AddHttpClient<GamesFetcherClient>(client =>
{
	client.BaseAddress = new Uri("http://50.118.225.175:2000/");
	client.Timeout = TimeSpan.FromSeconds(120);
});

builder.Services.AddHostedService<GamesProcessingService>();
var app = builder.Build();  

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

