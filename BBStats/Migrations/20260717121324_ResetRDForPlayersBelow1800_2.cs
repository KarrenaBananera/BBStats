using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBStats.Migrations
{
    /// <inheritdoc />
    public partial class ResetRDForPlayersBelow1800_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE PlayersCharactersStats SET PlayerRating_RatingDeviation = 225 WHERE PlayerRating_CurrentRating < 1800;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
