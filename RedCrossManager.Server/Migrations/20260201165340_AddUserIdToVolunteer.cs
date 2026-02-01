using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedCrossManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToVolunteer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Volunteers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Volunteers_UserId",
                table: "Volunteers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Volunteers_Users_UserId",
                table: "Volunteers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Volunteers_Users_UserId",
                table: "Volunteers");

            migrationBuilder.DropIndex(
                name: "IX_Volunteers_UserId",
                table: "Volunteers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Volunteers");
        }
    }
}
