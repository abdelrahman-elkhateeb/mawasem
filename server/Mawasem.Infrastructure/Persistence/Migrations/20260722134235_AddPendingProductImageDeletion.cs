using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mawasem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingProductImageDeletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PendingProductImageDeletions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StorageKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    NextAttemptAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastAttemptAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingProductImageDeletions", x => x.Id);
                    table.CheckConstraint("CK_PendingProductImageDeletions_AttemptCount", "[AttemptCount] >= 0");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PendingProductImageDeletions_NextAttemptAt_Id",
                table: "PendingProductImageDeletions",
                columns: new[] { "NextAttemptAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_PendingProductImageDeletions_StorageKey",
                table: "PendingProductImageDeletions",
                column: "StorageKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PendingProductImageDeletions");
        }
    }
}
