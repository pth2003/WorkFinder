using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFinder.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddMetadataToJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                schema: "public",
                table: "jobs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Metadata",
                schema: "public",
                table: "jobs");
        }
    }
}
