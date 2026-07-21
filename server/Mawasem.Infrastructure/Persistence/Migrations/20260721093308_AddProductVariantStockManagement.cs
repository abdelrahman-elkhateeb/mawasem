using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mawasem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductVariantStockManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductVariantOptions_ProductOptionValues_ProductOptionValueId" ,
                table: "ProductVariantOptions");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_ProductId" ,
                table: "ProductVariants");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariantOptions_ProductVariantId" ,
                table: "ProductVariantOptions");

            migrationBuilder.AddColumn<string>(
                name: "OptionCombinationKey" ,
                table: "ProductVariants" ,
                type: "nvarchar(450)" ,
                maxLength: 450 ,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion" ,
                table: "ProductVariants" ,
                type: "rowversion" ,
                rowVersion: true ,
                nullable: false ,
                defaultValue: new byte[0]);

            migrationBuilder.Sql(
                """
                UPDATE [pv]
                SET [pv].[OptionCombinationKey] =
                    COALESCE(
                        STUFF(
                            (
                                SELECT
                                    N'|' +
                                    CONVERT(
                                        nvarchar(20),
                                        [pvo].[ProductOptionValueId])
                                FROM [ProductVariantOptions] AS [pvo]
                                WHERE
                                    [pvo].[ProductVariantId] =
                                    [pv].[Id]
                                ORDER BY
                                    [pvo].[ProductOptionValueId]
                                FOR XML PATH(''), TYPE
                            ).value(
                                '.',
                                'nvarchar(max)'),
                            1,
                            1,
                            N''),
                        N'DEFAULT')
                FROM [ProductVariants] AS [pv];
                """);

            migrationBuilder.AlterColumn<string>(
                name: "OptionCombinationKey" ,
                table: "ProductVariants" ,
                type: "nvarchar(450)" ,
                maxLength: 450 ,
                nullable: false ,
                oldClrType: typeof(string) ,
                oldType: "nvarchar(450)" ,
                oldMaxLength: 450 ,
                oldNullable: true);

            migrationBuilder.Sql(
                """
                IF EXISTS
                (
                    SELECT 1
                    FROM [ProductVariants]
                    GROUP BY
                        [ProductId],
                        [OptionCombinationKey]
                    HAVING COUNT(*) > 1
                )
                BEGIN
                    THROW 50001,
                        'Existing product variants contain duplicate option combinations for the same product.',
                        1;
                END;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId_OptionCombinationKey" ,
                table: "ProductVariants" ,
                columns: new[]
                {
                    "ProductId",
                    "OptionCombinationKey"
                } ,
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProductVariants_StockQuantity_NonNegative" ,
                table: "ProductVariants" ,
                sql: "[StockQuantity] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantOptions_ProductVariantId_ProductOptionValueId" ,
                table: "ProductVariantOptions" ,
                columns: new[]
                {
                    "ProductVariantId",
                    "ProductOptionValueId"
                } ,
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariantOptions_ProductOptionValues_ProductOptionValueId" ,
                table: "ProductVariantOptions" ,
                column: "ProductOptionValueId" ,
                principalTable: "ProductOptionValues" ,
                principalColumn: "Id" ,
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductVariantOptions_ProductOptionValues_ProductOptionValueId" ,
                table: "ProductVariantOptions");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_ProductId_OptionCombinationKey" ,
                table: "ProductVariants");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProductVariants_StockQuantity_NonNegative" ,
                table: "ProductVariants");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariantOptions_ProductVariantId_ProductOptionValueId" ,
                table: "ProductVariantOptions");

            migrationBuilder.DropColumn(
                name: "OptionCombinationKey" ,
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "RowVersion" ,
                table: "ProductVariants");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId" ,
                table: "ProductVariants" ,
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantOptions_ProductVariantId" ,
                table: "ProductVariantOptions" ,
                column: "ProductVariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariantOptions_ProductOptionValues_ProductOptionValueId" ,
                table: "ProductVariantOptions" ,
                column: "ProductOptionValueId" ,
                principalTable: "ProductOptionValues" ,
                principalColumn: "Id" ,
                onDelete: ReferentialAction.Cascade);
        }
    }
}