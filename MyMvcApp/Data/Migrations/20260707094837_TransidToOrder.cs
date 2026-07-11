using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class TransidToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankTraceCode",
                table: "Order");

            migrationBuilder.AddColumn<long>(
                name: "TransactionId",
                table: "Order",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Order");

            migrationBuilder.AddColumn<string>(
                name: "BankTraceCode",
                table: "Order",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
