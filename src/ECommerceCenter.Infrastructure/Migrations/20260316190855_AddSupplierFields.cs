using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalSkuId",
                table: "ProductVariants",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalProductId",
                table: "Products",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Supplier",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalCategoryId",
                table: "Categories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Supplier",
                table: "Categories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SupplierCredentials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierType = table.Column<int>(type: "int", nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    OpenId = table.Column<long>(type: "bigint", nullable: true),
                    AccessToken = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AccessTokenExpiryDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    RefreshTokenExpiryDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LastRefreshedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierCredentials", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "UX_Categories_Supplier_ExternalCategoryId",
                table: "Categories",
                columns: new[] { "Supplier", "ExternalCategoryId" },
                unique: true,
                filter: "[Supplier] IS NOT NULL AND [ExternalCategoryId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_SupplierCredentials_SupplierType",
                table: "SupplierCredentials",
                column: "SupplierType",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupplierCredentials");

            migrationBuilder.DropIndex(
                name: "UX_Categories_Supplier_ExternalCategoryId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "ExternalSkuId",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "ExternalProductId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Supplier",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ExternalCategoryId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Supplier",
                table: "Categories");
        }
    }
}
