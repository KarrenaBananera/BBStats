using BBStats.Data.Entites;
using BBStats.Services;
using Microsoft.EntityFrameworkCore;

namespace BBStats.Data;

public class GameRepository : IGamesRepository
{
	private const Int64 MIN_STEAM_ID = 76561197960265729;
	private const Int64 MAX_STEAM_ID = 76561202255233023;
	private readonly AppDbContext _dbContext;

	public GameRepository(AppDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<bool> AddGameAsync(GameDTO game)
	{

		if (!IsGameValid(game))
			return false;
			
		await _dbContext.Database.BeginTransactionAsync();

		var gameInDb = await AddGameFromDtoAsync(game);
		await UpdateMatchupAsync(game);

		var playerA =  await UpdateOrCreatePlayerAsync(game.PlayerAId, game.PlayerA);
		var playerB =  await UpdateOrCreatePlayerAsync(game.PlayerBId, game.PlayerB);


		var characterStatA =  await GetOrCreatePlayerCharacterStat(game.PlayerAId, game.CharacterAId);
		var characterStatB =  await GetOrCreatePlayerCharacterStat(game.PlayerBId, game.CharacterBId);

		await RecordPlayerGame( characterStatA, characterStatB, gameInDb);
		await _dbContext.SaveChangesAsync();
		await _dbContext.Database.CommitTransactionAsync();

		return true;
	}

	private async Task<Player> UpdateOrCreatePlayerAsync(Int64 userId, string playerName)
	{
		var player = await _dbContext.Players.FirstOrDefaultAsync(x => x.Id == userId);

		if (player == null)
		{
			player = new Player { Id = userId, Name = playerName };
			await _dbContext.Players.AddAsync(player);
			return player;
		}

		if (!player.Name.Equals(playerName))
		{
			player.Name = playerName;
			_dbContext.Players.Update(player);
		}

		return player;
	}
	private async Task<PlayerCharacterStat> GetOrCreatePlayerCharacterStat(Int64 playerId, int characterId)
	{
		var playerCharacterStat = await _dbContext.PlayersCharactersStats.
			FirstOrDefaultAsync(x => x.PlayerId == playerId && x.CharacterId == characterId);

		if (playerCharacterStat == null)
		{
			playerCharacterStat = new PlayerCharacterStat
			{
				PlayerId = playerId,
				CharacterId = characterId,
				PlayerRating = Glicko2Calculator.GetDefaultRating(),
				MaxRating = Glicko2Calculator.GetDefaultRating().CurrentRating,
			};

			await _dbContext.AddAsync(playerCharacterStat);
		}

		return playerCharacterStat;
	}
	private async Task UpdateMatchupAsync(GameDTO game)
	{
		if (game.CharacterAId > game.CharacterBId)
		{
			game = game with
			{
				CharacterAId = game.CharacterBId,
				CharacterBId = game.CharacterAId,
				IsPlayerAWin = !game.IsPlayerAWin
			};
		}

		var matchupDb = _dbContext.Matchups.FirstOrDefault(x => x.CharacterAId == game.CharacterAId &&
		x.CharacterBId == game.CharacterBId);

		if (matchupDb == null)
		{
			matchupDb = new Matchup();
			matchupDb.CharacterAId = game.CharacterAId;
			matchupDb.CharacterBId = game.CharacterBId;
			await _dbContext.Matchups.AddAsync(matchupDb);
			await _dbContext.SaveChangesAsync();
		}

		matchupDb.TotalGames++;

		if (game.IsPlayerAWin)
			matchupDb.WinsA++;
		else
			matchupDb.WinsB++;
	}
	
	private static bool IsGameValid(GameDTO game)
	{
		if (game.PlayerAId < MIN_STEAM_ID || game.PlayerAId > MAX_STEAM_ID)
			return false;
		if (game.PlayerBId < MIN_STEAM_ID || game.PlayerBId > MAX_STEAM_ID)
			return false;

		if (String.IsNullOrWhiteSpace(game.PlayerA) || String.IsNullOrWhiteSpace(game.PlayerB))
			return false;

		if (String.IsNullOrWhiteSpace(game.ReplayId))
			return false;

		return true;
	}

	private async Task RecordPlayerGame(PlayerCharacterStat characterStatA, 
		PlayerCharacterStat characterStatB,
		Game game)
	{
		if (game.IsPlayerAWin)
		{
			characterStatA.Wins++;
			characterStatB.Losses++;
		}
		else
		{
			characterStatB.Wins++;
			characterStatA.Losses++;
		}

		var oldRatingA = characterStatA.PlayerRating;
		var oldRatingB = characterStatB.PlayerRating;

		var ratingChange = Glicko2Calculator.CalculateRating(
			characterStatA.PlayerRating, characterStatB.PlayerRating, game.IsPlayerAWin);

		characterStatA.PlayerRating = ratingChange.playerA;
		characterStatB.PlayerRating = ratingChange.playerB;

		characterStatA.MaxRating = Math.Max(characterStatA.MaxRating,
			characterStatA.PlayerRating.CurrentRating);
		characterStatB.MaxRating = Math.Max(characterStatB.MaxRating,
			characterStatB.PlayerRating.CurrentRating);

		var playerAGame = new PlayerGame()
		{
			PlayerId = characterStatA.PlayerId,
			GameId = game.Id,
			CharacterId = characterStatA.CharacterId,
			EloBefore = oldRatingA.CurrentRating,
			EloAfter = characterStatA.PlayerRating.CurrentRating,
			Game = game
		};

		var playerBGame = new PlayerGame()
		{
			PlayerId = characterStatB.PlayerId,
			GameId = game.Id,
			CharacterId = characterStatB.CharacterId,
			EloBefore = oldRatingB.CurrentRating,
			EloAfter = characterStatB.PlayerRating.CurrentRating,
			Game = game
		};

		await _dbContext.PlayersGames.AddAsync(playerBGame);
		await _dbContext.PlayersGames.AddAsync(playerAGame);

	}

	private async Task<Game> AddGameFromDtoAsync(GameDTO game)
	{
		var gameToAdd = new Game()
		{
			CharacterAId = game.CharacterAId,
			CharacterBId = game.CharacterBId,
			ReplayId = game.ReplayId,
			IsPlayerAWin = game.IsPlayerAWin,
			PlayerAId = game.PlayerAId,
			PlayerBId = game.PlayerBId,
			PlayedAt = game.PlayedAt,
			PlayerA = game.PlayerA,
			PlayerB = game.PlayerB,
		};

		await _dbContext.Games.AddAsync(gameToAdd);
		return gameToAdd;
	}
}
