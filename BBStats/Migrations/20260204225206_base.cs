using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BBStats.Migrations
{
    /// <inheritdoc />
    public partial class @base : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerA = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    PlayerB = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    PlayerAId = table.Column<long>(type: "INTEGER", nullable: false),
                    PlayerBId = table.Column<long>(type: "INTEGER", nullable: false),
                    GameUrl = table.Column<string>(type: "TEXT", nullable: false),
                    PlayedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsPlayerAWin = table.Column<bool>(type: "INTEGER", nullable: false),
                    CharacterAId = table.Column<int>(type: "INTEGER", nullable: false),
                    CharacterBId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Games_Characters_CharacterAId",
                        column: x => x.CharacterAId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Games_Characters_CharacterBId",
                        column: x => x.CharacterBId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Matchups",
                columns: table => new
                {
                    CharacterAId = table.Column<int>(type: "INTEGER", nullable: false),
                    CharacterBId = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalGames = table.Column<int>(type: "INTEGER", nullable: false),
                    WinsA = table.Column<int>(type: "INTEGER", nullable: false),
                    WinsB = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matchups", x => new { x.CharacterAId, x.CharacterBId });
                    table.CheckConstraint("CharacterOrder", "\"CharacterAId\" <= \"CharacterBId\"");
                    table.ForeignKey(
                        name: "FK_Matchups_Characters_CharacterAId",
                        column: x => x.CharacterAId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Matchups_Characters_CharacterBId",
                        column: x => x.CharacterBId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayersCharactersStats",
                columns: table => new
                {
                    PlayerId = table.Column<long>(type: "INTEGER", nullable: false),
                    CharacterId = table.Column<int>(type: "INTEGER", nullable: false),
                    Wins = table.Column<int>(type: "INTEGER", nullable: false),
                    Losses = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxRating = table.Column<double>(type: "REAL", nullable: false, defaultValue: 1000.0),
                    PlayerRating_CurrentRating = table.Column<double>(type: "REAL", nullable: false),
                    PlayerRating_RatingDeviation = table.Column<double>(type: "REAL", nullable: false),
                    PlayerRating_Volatility = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayersCharactersStats", x => new { x.PlayerId, x.CharacterId });
                    table.ForeignKey(
                        name: "FK_PlayersCharactersStats_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayersCharactersStats_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayersGames",
                columns: table => new
                {
                    GameId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayerId = table.Column<long>(type: "INTEGER", nullable: false),
                    EloBefore = table.Column<double>(type: "REAL", nullable: false),
                    EloAfter = table.Column<double>(type: "REAL", nullable: false),
                    PlayerCharacterStatCharacterId = table.Column<int>(type: "INTEGER", nullable: true),
                    PlayerCharacterStatPlayerId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayersGames", x => new { x.PlayerId, x.GameId });
                    table.ForeignKey(
                        name: "FK_PlayersGames_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayersGames_PlayersCharactersStats_PlayerCharacterStatPlayerId_PlayerCharacterStatCharacterId",
                        columns: x => new { x.PlayerCharacterStatPlayerId, x.PlayerCharacterStatCharacterId },
                        principalTable: "PlayersCharactersStats",
                        principalColumns: new[] { "PlayerId", "CharacterId" });
                    table.ForeignKey(
                        name: "FK_PlayersGames_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Characters",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Ragna" },
                    { 2, "Jin" },
                    { 3, "Noel" },
                    { 4, "Rachel" },
                    { 5, "Taokaka" },
                    { 6, "Tager" },
                    { 7, "Litchi" },
                    { 8, "Arakune" },
                    { 9, "Bang" },
                    { 10, "Carl" },
                    { 11, "Hakumen" },
                    { 12, "Nu" },
                    { 13, "Tsubaki" },
                    { 14, "Hazama" },
                    { 15, "Mu" },
                    { 16, "Makoto" },
                    { 17, "Valkenhayn" },
                    { 18, "Platinum" },
                    { 19, "Relius" },
                    { 20, "Izayoi" },
                    { 21, "Amane" },
                    { 22, "Bullet" },
                    { 23, "Azrael" },
                    { 24, "Kagura" },
                    { 25, "Kokonoe" },
                    { 26, "Terumi" },
                    { 27, "Celica" },
                    { 28, "Lambda" },
                    { 29, "Hibiki" },
                    { 30, "Nine" },
                    { 31, "Naoto" },
                    { 32, "Izanami" },
                    { 33, "Susanoo" },
                    { 34, "Es" },
                    { 35, "Mai" },
                    { 36, "Jubei" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Games_CharacterAId",
                table: "Games",
                column: "CharacterAId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_CharacterBId",
                table: "Games",
                column: "CharacterBId");

            migrationBuilder.CreateIndex(
                name: "IX_Matchups_CharacterBId",
                table: "Matchups",
                column: "CharacterBId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayersCharactersStats_CharacterId",
                table: "PlayersCharactersStats",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayersGames_GameId",
                table: "PlayersGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayersGames_PlayerCharacterStatPlayerId_PlayerCharacterStatCharacterId",
                table: "PlayersGames",
                columns: new[] { "PlayerCharacterStatPlayerId", "PlayerCharacterStatCharacterId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Matchups");

            migrationBuilder.DropTable(
                name: "PlayersGames");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "PlayersCharactersStats");

            migrationBuilder.DropTable(
                name: "Characters");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
