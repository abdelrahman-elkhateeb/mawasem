using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mawasem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSeasonToCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.AddColumn<int>(
                name: "SeasonId" ,
                table: "Collections" ,
                type: "int" ,
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_Collections_SeasonId" ,
                table: "Collections" ,
                column: "SeasonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Collections_Seasons_SeasonId" ,
                table: "Collections" ,
                column: "SeasonId" ,
                principalTable: "Seasons" ,
                principalColumn: "Id" ,
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Collections_Seasons_SeasonId" ,
                table: "Collections");

            migrationBuilder.DropIndex(
                name: "IX_Collections_SeasonId" ,
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "SeasonId" ,
                table: "Collections");
        }
    }
}
