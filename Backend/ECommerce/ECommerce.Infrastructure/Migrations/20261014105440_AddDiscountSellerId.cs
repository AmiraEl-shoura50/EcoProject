using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountSellerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SellerId",
                table: "Discounts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Discounts_SellerId",
                table: "Discounts",
                column: "SellerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Discounts_Sellers_SellerId",
                table: "Discounts",
                column: "SellerId",
                principalTable: "Sellers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Discounts_Sellers_SellerId",
                table: "Discounts");

            migrationBuilder.DropIndex(
                name: "IX_Discounts_SellerId",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "SellerId",
                table: "Discounts");
        }
    }
}
