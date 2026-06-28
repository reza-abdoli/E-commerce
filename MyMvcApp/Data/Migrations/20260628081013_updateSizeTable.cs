using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class updateSizeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Size",
                table: "ProductVariant");

            migrationBuilder.RenameColumn(
                name: "Size",
                table: "ProductSize",
                newName: "SizeType");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "ProductSize",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "ProductSize");

            migrationBuilder.RenameColumn(
                name: "SizeType",
                table: "ProductSize",
                newName: "Size");

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "ProductVariant",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
