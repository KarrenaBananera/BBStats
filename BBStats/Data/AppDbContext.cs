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

		modelBuilder.Entity<Player>(entity =>
		{
			entity.Property(p => p.Id).ValueGeneratedNever();
		});

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

			//entity.HasMany(ps => ps.Games)
			//	  .WithOne(pg => pg.CharacterStat)  
			//	  .HasForeignKey(pg => new { pg.PlayerId, pg.CharacterId })
			//	  .HasPrincipalKey(ps => new { ps.PlayerId, ps.CharacterId });

			entity.OwnsOne(x => x.PlayerRating);
		});

		modelBuilder.Entity<Game>(entity =>
		{
			entity.HasOne(g => g.CharacterA)
				.WithMany()
				.HasForeignKey(g => g.CharacterAId)
				.OnDelete(DeleteBehavior.Restrict);

			entity.HasOne(g => g.CharacterB)
				.WithMany()
				.HasForeignKey(g => g.CharacterBId)
				.OnDelete(DeleteBehavior.Restrict);
		});

		modelBuilder.Entity<Matchup>(entity =>
		{
			entity.HasKey(u => new { u.CharacterAId, u.CharacterBId });
			entity.ToTable(t =>
				t.HasCheckConstraint("CharacterOrder", "[CharacterAId] <= [CharacterBId]"));

			entity.HasOne(m => m.CharacterA)
				.WithMany()
				.HasForeignKey(m => m.CharacterAId)
				.OnDelete(DeleteBehavior.Restrict);

			entity.HasOne(m => m.CharacterB)
				.WithMany()
				.HasForeignKey(m => m.CharacterBId)
				.OnDelete(DeleteBehavior.Restrict);
		});

		modelBuilder.Entity<PlayerGame>(entity =>
		{
			entity.HasKey(u => new { u.PlayerId, u.GameId, u.CharacterId });

			entity.HasOne(p => p.Game)
				  .WithMany()
				  .HasForeignKey(p => p.GameId);

			entity.HasOne(p => p.Character)
				  .WithMany()
				  .HasForeignKey(p => p.CharacterId)
				  .OnDelete(DeleteBehavior.Restrict);

			entity.HasOne(x => x.Player)
				  .WithMany()
				  .HasForeignKey(x => x.PlayerId);

			entity.HasOne<PlayerCharacterStat>()
				  .WithMany(ps => ps.Games)
				  .HasForeignKey(pg => new { pg.PlayerId, pg.CharacterId })
				  .OnDelete(DeleteBehavior.Restrict);

		});
	}
}	
