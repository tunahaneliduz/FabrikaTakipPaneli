using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FabrikaTakipPaneli.Migrations
{
    /// <inheritdoc />
    public partial class AddMinStockLevelToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MinStockLevel",
                table: "Products",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinStockLevel",
                table: "Products");
        }
    }
}
