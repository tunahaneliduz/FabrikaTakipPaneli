using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FabrikaTakipPaneli.Migrations
{
    /// <inheritdoc />
    public partial class AddIconToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Products",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("UPDATE Products SET Icon = '📦' WHERE Icon IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Products");
        }
    }
}
