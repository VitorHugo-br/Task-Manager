using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task_Manager.Migrations
{
    /// <inheritdoc />
    public partial class IssuerAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestedBy",
                table: "Tasks");

            migrationBuilder.AddColumn<int>(
                name: "IssuerId",
                table: "Tasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_IssuerId",
                table: "Tasks",
                column: "IssuerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_IssuerId",
                table: "Tasks",
                column: "IssuerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_IssuerId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_IssuerId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "IssuerId",
                table: "Tasks");

            migrationBuilder.AddColumn<string>(
                name: "RequestedBy",
                table: "Tasks",
                type: "longtext",
                nullable: false);
        }
    }
}
