using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryCreatedBySeller : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedBySellerId",
                table: "Categories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CreatedBySellerId",
                table: "Categories",
                column: "CreatedBySellerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Sellers_CreatedBySellerId",
                table: "Categories",
                column: "CreatedBySellerId",
                principalTable: "Sellers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Sellers_CreatedBySellerId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_CreatedBySellerId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CreatedBySellerId",
                table: "Categories");
        }
    }
}
