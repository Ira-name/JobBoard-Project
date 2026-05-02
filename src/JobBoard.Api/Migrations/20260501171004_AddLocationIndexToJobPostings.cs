using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobBoard.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationIndexToJobPostings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_JobPostings_Location",
                table: "JobPostings",
                column: "Location");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobPostings_Location",
                table: "JobPostings");
        }
    }
}
