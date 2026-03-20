using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVariantSupplierPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SupplierPrice",
                table: "ProductVariants",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SupplierPrice",
                table: "ProductVariants");
        }
    }
}
