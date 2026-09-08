using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThingRecommender.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddManualRecommendations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "RecommenderId",
                table: "Recommendations",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "ExternalRecommenderName",
                table: "Recommendations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalRecommenderName",
                table: "Recommendations");

            migrationBuilder.AlterColumn<Guid>(
                name: "RecommenderId",
                table: "Recommendations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
