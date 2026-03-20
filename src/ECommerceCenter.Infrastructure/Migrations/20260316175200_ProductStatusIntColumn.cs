using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProductStatusIntColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupplierCredentials");

            // Step 1: drop the index on the string Status column
            migrationBuilder.DropIndex(
                name: "IX_Products_Status",
                table: "Products");

            // Step 2: add temp int column
            migrationBuilder.Sql(
                "ALTER TABLE [Products] ADD [StatusInt] INT NOT NULL DEFAULT 1;");

            // Step 3: migrate string → int
            migrationBuilder.Sql(@"
                UPDATE [Products] SET [StatusInt] = CASE [Status]
                    WHEN 'Draft'    THEN 1
                    WHEN 'Active'   THEN 2
                    WHEN 'Archived' THEN 3
                    ELSE 1
                END;");

            // Step 4: drop old string column
            migrationBuilder.Sql(
                "ALTER TABLE [Products] DROP COLUMN [Status];");

            // Step 5: rename temp column to Status
            migrationBuilder.Sql(
                "EXEC sp_rename 'Products.StatusInt', 'Status', 'COLUMN';");

            // Step 6: recreate index on the new int column
            migrationBuilder.CreateIndex(
                name: "IX_Products_Status",
                table: "Products",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse: int → nvarchar(32)
            migrationBuilder.DropIndex(
                name: "IX_Products_Status",
                table: "Products");

            migrationBuilder.Sql(
                "ALTER TABLE [Products] ADD [StatusStr] NVARCHAR(32) NOT NULL DEFAULT 'Draft';");

            migrationBuilder.Sql(@"
                UPDATE [Products] SET [StatusStr] = CASE [Status]
                    WHEN 1 THEN 'Draft'
                    WHEN 2 THEN 'Active'
                    WHEN 3 THEN 'Archived'
                    ELSE 'Draft'
                END;");

            migrationBuilder.Sql(
                "ALTER TABLE [Products] DROP COLUMN [Status];");

            migrationBuilder.Sql(
                "EXEC sp_rename 'Products.StatusStr', 'Status', 'COLUMN';");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Status",
                table: "Products",
                column: "Status");

            migrationBuilder.CreateTable(
                name: "SupplierCredentials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccessToken = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AccessTokenExpiryDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ApiKey = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LastRefreshedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OpenId = table.Column<long>(type: "bigint", nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    RefreshTokenExpiryDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SupplierType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierCredentials", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "UX_SupplierCredentials_SupplierType",
                table: "SupplierCredentials",
                column: "SupplierType",
                unique: true);
        }
    }
}
