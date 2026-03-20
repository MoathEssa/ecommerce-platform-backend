using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeSkuNullableAndFilteredUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_ProductVariants_Sku",
                table: "ProductVariants");

            migrationBuilder.AlterColumn<string>(
                name: "Sku",
                table: "ProductVariants",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.CreateIndex(
                name: "UX_ProductVariants_Sku",
                table: "ProductVariants",
                column: "Sku",
                unique: true,
                filter: "[Sku] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_ProductVariants_Sku",
                table: "ProductVariants");

            migrationBuilder.AlterColumn<string>(
                name: "Sku",
                table: "ProductVariants",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_ProductVariants_Sku",
                table: "ProductVariants",
                column: "Sku",
                unique: true);
        }
    }
}
