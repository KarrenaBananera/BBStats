using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBStats.Migrations
{
    /// <inheritdoc />
    public partial class SyncPlayerGameModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayersGames_PlayersCharactersStats_PlayerCharacterStatPlayerId_PlayerCharacterStatCharacterId",
                table: "PlayersGames");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlayersGames",
                table: "PlayersGames");

            migrationBuilder.DropIndex(
                name: "IX_PlayersGames_PlayerCharacterStatPlayerId_PlayerCharacterStatCharacterId",
                table: "PlayersGames");

            migrationBuilder.DropColumn(
                name: "PlayerCharacterStatCharacterId",
                table: "PlayersGames");

            migrationBuilder.DropColumn(
                name: "PlayerCharacterStatPlayerId",
                table: "PlayersGames");

            migrationBuilder.AddColumn<int>(
                name: "CharacterId",
                table: "PlayersGames",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlayersGames",
                table: "PlayersGames",
                columns: new[] { "PlayerId", "GameId", "CharacterId" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayersGames_CharacterId",
                table: "PlayersGames",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayersGames_PlayerId_CharacterId",
                table: "PlayersGames",
                columns: new[] { "PlayerId", "CharacterId" });

            migrationBuilder.AddForeignKey(
                name: "FK_PlayersGames_Characters_CharacterId",
                table: "PlayersGames",
                column: "CharacterId",
                principalTable: "Characters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayersGames_PlayersCharactersStats_PlayerId_CharacterId",
                table: "PlayersGames",
                columns: new[] { "PlayerId", "CharacterId" },
                principalTable: "PlayersCharactersStats",
                principalColumns: new[] { "PlayerId", "CharacterId" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayersGames_Characters_CharacterId",
                table: "PlayersGames");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayersGames_PlayersCharactersStats_PlayerId_CharacterId",
                table: "PlayersGames");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlayersGames",
                table: "PlayersGames");

            migrationBuilder.DropIndex(
                name: "IX_PlayersGames_CharacterId",
                table: "PlayersGames");

            migrationBuilder.DropIndex(
                name: "IX_PlayersGames_PlayerId_CharacterId",
                table: "PlayersGames");

            migrationBuilder.DropColumn(
                name: "CharacterId",
                table: "PlayersGames");

            migrationBuilder.AddColumn<int>(
                name: "PlayerCharacterStatCharacterId",
                table: "PlayersGames",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PlayerCharacterStatPlayerId",
                table: "PlayersGames",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlayersGames",
                table: "PlayersGames",
                columns: new[] { "PlayerId", "GameId" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayersGames_PlayerCharacterStatPlayerId_PlayerCharacterStatCharacterId",
                table: "PlayersGames",
                columns: new[] { "PlayerCharacterStatPlayerId", "PlayerCharacterStatCharacterId" });

            migrationBuilder.AddForeignKey(
                name: "FK_PlayersGames_PlayersCharactersStats_PlayerCharacterStatPlayerId_PlayerCharacterStatCharacterId",
                table: "PlayersGames",
                columns: new[] { "PlayerCharacterStatPlayerId", "PlayerCharacterStatCharacterId" },
                principalTable: "PlayersCharactersStats",
                principalColumns: new[] { "PlayerId", "CharacterId" });
        }
    }
}
