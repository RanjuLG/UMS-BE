using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UMS_BE.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformToUsersAndRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserName",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Roles_Name",
                table: "Roles");

            migrationBuilder.AddColumn<int>(
                name: "PlatformId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlatformId",
                table: "Roles",
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

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name_PlatformId",
                table: "Roles",
                columns: new[] { "Name", "PlatformId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_PlatformId",
                table: "Roles",
                column: "PlatformId");

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Platforms_PlatformId",
                table: "Roles",
                column: "PlatformId",
                principalTable: "Platforms",
                principalColumn: "PlatformId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Platforms_PlatformId",
                table: "Users",
                column: "PlatformId",
                principalTable: "Platforms",
                principalColumn: "PlatformId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Platforms_PlatformId",
                table: "Roles");

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

            migrationBuilder.DropIndex(
                name: "IX_Roles_Name_PlatformId",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Roles_PlatformId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "PlatformId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PlatformId",
                table: "Roles");

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
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);
        }
    }
}
