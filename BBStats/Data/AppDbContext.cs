using BBStats.Data.Entites;
using Microsoft.EntityFrameworkCore;

namespace BBStats.Data;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options)
		   : base(options)
	{
	}
	public DbSet<Character> Characters => Set<Character>();
	public DbSet<PlayerGame> PlayersGames => Set<PlayerGame>();
	public DbSet<Game> Games => Set<Game>();
	public DbSet<Matchup> Matchups => Set<Matchup>();
	public DbSet<Player> Players => Set<Player>();
	public DbSet<PlayerCharacterStat> PlayersCharactersStats => Set<PlayerCharacterStat>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Character>().HasData(CharactersSeed.All);

		modelBuilder.Entity<PlayerCharacterStat>(entity =>
		{
			entity.HasKey(u => new { u.PlayerId, u.CharacterId });

			entity.Property(u => u.MaxRating).HasDefaultValue(1000);

			entity.HasOne(u => u.Player)
				  .WithMany(p => p.CharactersStat)
				  .HasForeignKey(u => u.PlayerId)
				  .OnDelete(DeleteBehavior.Cascade);

			entity.HasOne(u => u.Character)
					.WithMany()
					.HasForeignKey(u => u.CharacterId)
					.OnDelete(DeleteBehavior.Cascade);

			entity.OwnsOne(x => x.PlayerRating);
		});

		modelBuilder.Entity<Matchup>(entity =>
		{
			entity.HasKey(u => new { u.CharacterAId, u.CharacterBId });
			entity.ToTable(t =>
			t.HasCheckConstraint("CharacterOrder",
				"\"CharacterAId\" <= \"CharacterBId\""));
		});

		modelBuilder.Entity<PlayerGame>(entity =>
		{
			entity.HasKey(u => new { u.PlayerId, u.GameId, u.CharacterId });

			entity.HasOne(p => p.Game)
			.WithMany()
			.HasForeignKey(p => p.GameId);

			entity.HasOne(p => p.Character)
			.WithMany()
			.HasForeignKey(p => p.GameId);

			entity.HasOne(x => x.Player)
			.WithMany()
			.HasForeignKey(x => x.PlayerId);

			entity.HasOne(p => p.PlayerCharacterStat)
			.WithMany()
			.HasForeignKey(x => new { x.PlayerId, x.CharacterId });
		});
	}
}	
