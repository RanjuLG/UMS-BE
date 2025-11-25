using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UMS_BE.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPlatformManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Platforms_PlatformId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email_PlatformId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_PlatformId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserName_PlatformId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PlatformId",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "UserPlatforms",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PlatformId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPlatforms", x => new { x.UserId, x.PlatformId });
                    table.ForeignKey(
                        name: "FK_UserPlatforms_Platforms_PlatformId",
                        column: x => x.PlatformId,
                        principalTable: "Platforms",
                        principalColumn: "PlatformId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPlatforms_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPlatforms_PlatformId",
                table: "UserPlatforms",
                column: "PlatformId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPlatforms");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserName",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "PlatformId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email_PlatformId",
                table: "Users",
                columns: new[] { "Email", "PlatformId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_PlatformId",
                table: "Users",
                column: "PlatformId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName_PlatformId",
                table: "Users",
                columns: new[] { "UserName", "PlatformId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Platforms_PlatformId",
                table: "Users",
                column: "PlatformId",
                principalTable: "Platforms",
                principalColumn: "PlatformId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
