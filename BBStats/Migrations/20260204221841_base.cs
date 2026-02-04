using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBStats.Migrations
{
    /// <inheritdoc />
    public partial class @base : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentElo",
                table: "PlayersCharactersStats");

            migrationBuilder.DropColumn(
                name: "MaxElo",
                table: "PlayersCharactersStats");

            migrationBuilder.AlterColumn<double>(
                name: "EloBefore",
                table: "PlayersGames",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<double>(
                name: "EloAfter",
                table: "PlayersGames",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<double>(
                name: "MaxRating",
                table: "PlayersCharactersStats",
                type: "REAL",
                nullable: false,
                defaultValue: 1000.0);

            migrationBuilder.AddColumn<double>(
                name: "PlayerRating_CurrentRating",
                table: "PlayersCharactersStats",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PlayerRating_RatingDeviation",
                table: "PlayersCharactersStats",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PlayerRating_Volatility",
                table: "PlayersCharactersStats",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "CharacterAId",
                table: "Games",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CharacterBId",
                table: "Games",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Characters",
                keyColumn: "Id",
                keyValue: 12,
                column: "Name",
                value: "Nu");

            migrationBuilder.UpdateData(
                table: "Characters",
                keyColumn: "Id",
                keyValue: 15,
                column: "Name",
                value: "Mu");

            migrationBuilder.UpdateData(
                table: "Characters",
                keyColumn: "Id",
                keyValue: 33,
                column: "Name",
                value: "Susanoo");

            migrationBuilder.CreateIndex(
                name: "IX_PlayersGames_GameId",
                table: "PlayersGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_CharacterAId",
                table: "Games",
                column: "CharacterAId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_CharacterBId",
                table: "Games",
                column: "CharacterBId");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Characters_CharacterAId",
                table: "Games",
                column: "CharacterAId",
                principalTable: "Characters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Characters_CharacterBId",
                table: "Games",
                column: "CharacterBId",
                principalTable: "Characters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayersGames_Games_GameId",
                table: "PlayersGames",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_Characters_CharacterAId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_Characters_CharacterBId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayersGames_Games_GameId",
                table: "PlayersGames");

            migrationBuilder.DropIndex(
                name: "IX_PlayersGames_GameId",
                table: "PlayersGames");

            migrationBuilder.DropIndex(
                name: "IX_Games_CharacterAId",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Games_CharacterBId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "MaxRating",
                table: "PlayersCharactersStats");

            migrationBuilder.DropColumn(
                name: "PlayerRating_CurrentRating",
                table: "PlayersCharactersStats");

            migrationBuilder.DropColumn(
                name: "PlayerRating_RatingDeviation",
                table: "PlayersCharactersStats");

            migrationBuilder.DropColumn(
                name: "PlayerRating_Volatility",
                table: "PlayersCharactersStats");

            migrationBuilder.DropColumn(
                name: "CharacterAId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "CharacterBId",
                table: "Games");

            migrationBuilder.AlterColumn<int>(
                name: "EloBefore",
                table: "PlayersGames",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<int>(
                name: "EloAfter",
                table: "PlayersGames",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AddColumn<int>(
                name: "CurrentElo",
                table: "PlayersCharactersStats",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1000);

            migrationBuilder.AddColumn<int>(
                name: "MaxElo",
                table: "PlayersCharactersStats",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1000);

            migrationBuilder.UpdateData(
                table: "Characters",
                keyColumn: "Id",
                keyValue: 12,
                column: "Name",
                value: "Nu-13");

            migrationBuilder.UpdateData(
                table: "Characters",
                keyColumn: "Id",
                keyValue: 15,
                column: "Name",
                value: "Mu-12");

            migrationBuilder.UpdateData(
                table: "Characters",
                keyColumn: "Id",
                keyValue: 33,
                column: "Name",
                value: "Susano'o");
        }
    }
}
