using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Modules.Meals.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMealOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Meals_Meals",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Meals_Meals_OwnerId",
                table: "Meals_Meals",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Meals_Meals_OwnerId",
                table: "Meals_Meals");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Meals_Meals");
        }
    }
}
