using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shop.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updatePriceAndDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "Price",
                table: "Products",
                type: "real",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "Discount",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<float>(
                name: "OrderTotal",
                table: "OrderHeaders",
                type: "real",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<float>(
                name: "Price",
                table: "OrderDetails",
                type: "real",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "Discount", "PEGI", "Price", "Stock" },
                values: new object[] { 0, 12, 20f, 20 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "Discount", "PEGI", "Price", "Stock" },
                values: new object[] { 0, 16, 15f, 20 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discount",
                table: "Products");

            migrationBuilder.AlterColumn<int>(
                name: "Price",
                table: "Products",
                type: "int",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<int>(
                name: "OrderTotal",
                table: "OrderHeaders",
                type: "int",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<int>(
                name: "Price",
                table: "OrderDetails",
                type: "int",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "PEGI", "Price", "Stock" },
                values: new object[] { 0, 20, 0 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "PEGI", "Price", "Stock" },
                values: new object[] { 0, 15, 0 });
        }
    }
}
