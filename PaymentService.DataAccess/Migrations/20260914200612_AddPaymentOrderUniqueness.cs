using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentService.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentOrderUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_payments_OrderId",
                table: "payments");

            migrationBuilder.CreateIndex(
                name: "IX_payments_OrderId",
                table: "payments",
                column: "OrderId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_payments_OrderId",
                table: "payments");

            migrationBuilder.CreateIndex(
                name: "IX_payments_OrderId",
                table: "payments",
                column: "OrderId");
        }
    }
}
