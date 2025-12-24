using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Model.Migrations
{
    /// <inheritdoc />
    public partial class SoldCountAndShippingAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SoldCount",
                table: "Product",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SoldCount",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "ShippingAddress",
                table: "Orders");
        }
    }
}
