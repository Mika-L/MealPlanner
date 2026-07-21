using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Modules.Meals.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Meals_Meals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Latin1_General_CI_AI"),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false, collation: "Latin1_General_CI_AI"),
                    Seasons = table.Column<int>(type: "int", nullable: false),
                    Styles = table.Column<int>(type: "int", nullable: false),
                    PrepTimeMinutes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meals_Meals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Meals_Ingredients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MealId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Latin1_General_CI_AI")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meals_Ingredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Meals_Ingredients_Meals_Meals_MealId",
                        column: x => x.MealId,
                        principalTable: "Meals_Meals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Meals_Ingredients_MealId",
                table: "Meals_Ingredients",
                column: "MealId");

            migrationBuilder.CreateIndex(
                name: "IX_Meals_Ingredients_Name",
                table: "Meals_Ingredients",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Meals_Meals_OwnerId",
                table: "Meals_Meals",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Meals_Ingredients");

            migrationBuilder.DropTable(
                name: "Meals_Meals");
        }
    }
}
