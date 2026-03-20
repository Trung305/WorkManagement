using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixFileAttachmentForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileAttachments_Users_UploadedByUserId",
                table: "FileAttachments");

            migrationBuilder.DropIndex(
                name: "IX_FileAttachments_UploadedByUserId",
                table: "FileAttachments");

            migrationBuilder.DropColumn(
                name: "UploadedByUserId",
                table: "FileAttachments");

            migrationBuilder.CreateIndex(
                name: "IX_FileAttachments_UploadedBy",
                table: "FileAttachments",
                column: "UploadedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_FileAttachments_Users_UploadedBy",
                table: "FileAttachments",
                column: "UploadedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileAttachments_Users_UploadedBy",
                table: "FileAttachments");

            migrationBuilder.DropIndex(
                name: "IX_FileAttachments_UploadedBy",
                table: "FileAttachments");

            migrationBuilder.AddColumn<int>(
                name: "UploadedByUserId",
                table: "FileAttachments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_FileAttachments_UploadedByUserId",
                table: "FileAttachments",
                column: "UploadedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FileAttachments_Users_UploadedByUserId",
                table: "FileAttachments",
                column: "UploadedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
