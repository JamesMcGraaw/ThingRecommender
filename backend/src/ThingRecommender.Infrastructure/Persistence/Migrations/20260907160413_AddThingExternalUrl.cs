using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThingRecommender.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddThingExternalUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalUrl",
                table: "Things",
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalUrl",
                table: "Things");
        }
    }
}
