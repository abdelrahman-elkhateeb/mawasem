using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mawasem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductImageManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_ProductVariants_ProductVariantId" ,
                table: "ProductImages");

            migrationBuilder.DropIndex(
                name: "IX_ProductImages_ProductVariantId" ,
                table: "ProductImages");

            migrationBuilder.RenameColumn(
                name: "ProductVariantId" ,
                table: "ProductImages" ,
                newName: "ProductId");

            migrationBuilder.AddColumn<int>(
                name: "Type" ,
                table: "ProductOptions" ,
                type: "int" ,
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "ColorOptionValueId" ,
                table: "ProductImages" ,
                type: "int" ,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StorageKey" ,
                table: "ProductImages" ,
                type: "nvarchar(500)" ,
                maxLength: 500 ,
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "UX_ProductOptions_SingleColorOption" ,
                table: "ProductOptions" ,
                column: "Type" ,
                unique: true ,
                filter: "[Type] = 2");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ColorOptionValueId" ,
                table: "ProductImages" ,
                column: "ColorOptionValueId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_StorageKey" ,
                table: "ProductImages" ,
                column: "StorageKey" ,
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_ProductImages_GalleryDisplayOrder" ,
                table: "ProductImages" ,
                columns: new[]
                {
                    "ProductId",
                    "ColorOptionValueId",
                    "DisplayOrder"
                } ,
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_ProductImages_GalleryPrimary" ,
                table: "ProductImages" ,
                columns: new[]
                {
                    "ProductId",
                    "ColorOptionValueId"
                } ,
                unique: true ,
                filter: "[IsPrimary] = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProductImages_DisplayOrder_NonNegative" ,
                table: "ProductImages" ,
                sql: "[DisplayOrder] >= 0");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_ProductOptionValues_ColorOptionValueId" ,
                table: "ProductImages" ,
                column: "ColorOptionValueId" ,
                principalTable: "ProductOptionValues" ,
                principalColumn: "Id" ,
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_Products_ProductId" ,
                table: "ProductImages" ,
                column: "ProductId" ,
                principalTable: "Products" ,
                principalColumn: "Id" ,
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_ProductOptionValues_ColorOptionValueId" ,
                table: "ProductImages");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_Products_ProductId" ,
                table: "ProductImages");

            migrationBuilder.DropIndex(
                name: "UX_ProductOptions_SingleColorOption" ,
                table: "ProductOptions");

            migrationBuilder.DropIndex(
                name: "IX_ProductImages_ColorOptionValueId" ,
                table: "ProductImages");

            migrationBuilder.DropIndex(
                name: "IX_ProductImages_StorageKey" ,
                table: "ProductImages");

            migrationBuilder.DropIndex(
                name: "UX_ProductImages_GalleryDisplayOrder" ,
                table: "ProductImages");

            migrationBuilder.DropIndex(
                name: "UX_ProductImages_GalleryPrimary" ,
                table: "ProductImages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProductImages_DisplayOrder_NonNegative" ,
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "Type" ,
                table: "ProductOptions");

            migrationBuilder.DropColumn(
                name: "ColorOptionValueId" ,
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "StorageKey" ,
                table: "ProductImages");

            migrationBuilder.RenameColumn(
                name: "ProductId" ,
                table: "ProductImages" ,
                newName: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductVariantId" ,
                table: "ProductImages" ,
                column: "ProductVariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_ProductVariants_ProductVariantId" ,
                table: "ProductImages" ,
                column: "ProductVariantId" ,
                principalTable: "ProductVariants" ,
                principalColumn: "Id" ,
                onDelete: ReferentialAction.Cascade);
        }
    }
}