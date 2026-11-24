using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JavaMaster.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDeckCardsAndSharing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cards",
                table: "Decks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "Share",
                table: "Decks",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "public");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Decks",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            // Decks created before this migration have never been edited.
            migrationBuilder.Sql("UPDATE Decks SET UpdatedAt = CreatedAt;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cards",
                table: "Decks");

            migrationBuilder.DropColumn(
                name: "Share",
                table: "Decks");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Decks");
        }
    }
}
