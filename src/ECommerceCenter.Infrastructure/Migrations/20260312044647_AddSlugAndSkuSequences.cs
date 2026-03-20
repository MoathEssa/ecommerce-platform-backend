using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSlugAndSkuSequences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "slug_suffix_seq",
                startValue: 2L);

            migrationBuilder.CreateSequence(
                name: "variant_sku_seq");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "slug_suffix_seq");

            migrationBuilder.DropSequence(
                name: "variant_sku_seq");
        }
    }
}
