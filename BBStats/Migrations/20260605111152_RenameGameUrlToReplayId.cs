using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBStats.Migrations
{
    /// <inheritdoc />
    public partial class RenameGameUrlToReplayId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE Games
                SET GameUrl = CASE
                    WHEN CHARINDEX('/download/', GameUrl) > 0 THEN
                        SUBSTRING(GameUrl, CHARINDEX('/download/', GameUrl) + LEN('/download/'), 32)
                    WHEN CHARINDEX('/uploads/', GameUrl) > 0 THEN
                        SUBSTRING(GameUrl, CHARINDEX('/uploads/', GameUrl) + LEN('/uploads/'), 32)
                    WHEN RIGHT(GameUrl, 4) = '.dat' THEN LEFT(GameUrl, LEN(GameUrl) - 4)
                    ELSE GameUrl
                END
                WHERE GameUrl LIKE '%/%' OR RIGHT(GameUrl, 4) = '.dat';
                """);

            migrationBuilder.RenameColumn(
                name: "GameUrl",
                table: "Games",
                newName: "ReplayId");

            migrationBuilder.AlterColumn<string>(
                name: "ReplayId",
                table: "Games",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ReplayId",
                table: "Games",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.RenameColumn(
                name: "ReplayId",
                table: "Games",
                newName: "GameUrl");
        }
    }
}
