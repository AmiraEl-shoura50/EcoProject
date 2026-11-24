using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSettlementsAndSellerPayout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CommissionPercent",
                table: "Sellers",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayoutAccount",
                table: "Sellers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PayoutMethod",
                table: "Sellers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SettlementId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Settlements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SellerId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CutoffDay = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrdersCount = table.Column<int>(type: "int", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CommissionPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CommissionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdjustmentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdjustmentNote = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    NetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PayoutMethod = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PayoutAccount = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Settlements_Sellers_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Sellers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SettlementId",
                table: "Orders",
                column: "SettlementId");

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_SellerId_CreatedAt",
                table: "Settlements",
                columns: new[] { "SellerId", "CreatedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Settlements_SettlementId",
                table: "Orders",
                column: "SettlementId",
                principalTable: "Settlements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Settlements_SettlementId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "Settlements");

            migrationBuilder.DropIndex(
                name: "IX_Orders_SettlementId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CommissionPercent",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "PayoutAccount",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "PayoutMethod",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "SettlementId",
                table: "Orders");
        }
    }
}
