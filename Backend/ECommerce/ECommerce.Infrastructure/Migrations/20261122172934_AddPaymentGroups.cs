using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                table: "PaymentProofs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "PaymentGroupId",
                table: "PaymentProofs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentGroupId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PaymentGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    PaymentMethodId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentGroups_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentGroups_PaymentMethods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentProofs_PaymentGroupId",
                table: "PaymentProofs",
                column: "PaymentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PaymentGroupId",
                table: "Orders",
                column: "PaymentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentGroups_CustomerId",
                table: "PaymentGroups",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentGroups_PaymentMethodId",
                table: "PaymentGroups",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentGroups_Status",
                table: "PaymentGroups",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_PaymentGroups_PaymentGroupId",
                table: "Orders",
                column: "PaymentGroupId",
                principalTable: "PaymentGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentProofs_PaymentGroups_PaymentGroupId",
                table: "PaymentProofs",
                column: "PaymentGroupId",
                principalTable: "PaymentGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_PaymentGroups_PaymentGroupId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentProofs_PaymentGroups_PaymentGroupId",
                table: "PaymentProofs");

            migrationBuilder.DropTable(
                name: "PaymentGroups");

            migrationBuilder.DropIndex(
                name: "IX_PaymentProofs_PaymentGroupId",
                table: "PaymentProofs");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PaymentGroupId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentGroupId",
                table: "PaymentProofs");

            migrationBuilder.DropColumn(
                name: "PaymentGroupId",
                table: "Orders");

            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                table: "PaymentProofs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
