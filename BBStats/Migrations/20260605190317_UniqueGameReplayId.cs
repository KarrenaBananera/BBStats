using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBStats.Migrations
{
    /// <inheritdoc />
    public partial class UniqueGameReplayId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM PlayersGames
                WHERE GameId IN (
                    SELECT Id
                    FROM (
                        SELECT Id, ROW_NUMBER() OVER (PARTITION BY ReplayId ORDER BY Id) AS rn
                        FROM Games
                    ) ranked
                    WHERE rn > 1
                );

                DELETE FROM Games
                WHERE Id IN (
                    SELECT Id
                    FROM (
                        SELECT Id, ROW_NUMBER() OVER (PARTITION BY ReplayId ORDER BY Id) AS rn
                        FROM Games
                    ) ranked
                    WHERE rn > 1
                );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Games_ReplayId",
                table: "Games",
                column: "ReplayId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Games_ReplayId",
                table: "Games");
        }
    }
}
