using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JavaMaster.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Questions_Purpose_IsActive",
                table: "Questions",
                columns: new[] { "Purpose", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionAttempts_UserId_LearnNodeId_IsCorrect_CreatedAt",
                table: "QuestionAttempts",
                columns: new[] { "UserId", "LearnNodeId", "IsCorrect", "CreatedAt" })
                .Annotation("SqlServer:Include", new[] { "QuestionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_UserId",
                table: "Purchases",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Questions_Purpose_IsActive",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_QuestionAttempts_UserId_LearnNodeId_IsCorrect_CreatedAt",
                table: "QuestionAttempts");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_UserId",
                table: "Purchases");
        }
    }
}
