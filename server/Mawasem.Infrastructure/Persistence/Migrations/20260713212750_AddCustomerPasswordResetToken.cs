using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mawasem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerPasswordResetToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ResetTokenExpiresAtUtc",
                table: "PasswordResetCodes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResetTokenHash",
                table: "PasswordResetCodes",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetCodes_ResetTokenHash",
                table: "PasswordResetCodes",
                column: "ResetTokenHash",
                unique: true,
                filter: "[ResetTokenHash] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PasswordResetCodes_ResetTokenHash",
                table: "PasswordResetCodes");

            migrationBuilder.DropColumn(
                name: "ResetTokenExpiresAtUtc",
                table: "PasswordResetCodes");

            migrationBuilder.DropColumn(
                name: "ResetTokenHash",
                table: "PasswordResetCodes");
        }
    }
}
