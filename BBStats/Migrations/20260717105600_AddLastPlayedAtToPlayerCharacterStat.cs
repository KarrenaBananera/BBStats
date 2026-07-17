using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBStats.Migrations
{
    /// <inheritdoc />
    public partial class AddLastPlayedAtToPlayerCharacterStat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "LastDecayAppliedAt",
                table: "PlayersCharactersStats",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "LastPlayedAt",
                table: "PlayersCharactersStats",
                type: "date",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastDecayAppliedAt",
                table: "PlayersCharactersStats");

            migrationBuilder.DropColumn(
                name: "LastPlayedAt",
                table: "PlayersCharactersStats");
        }
    }
}
