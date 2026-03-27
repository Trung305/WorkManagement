using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationFailCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailCount",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailCount",
                table: "Notifications");
        }
    }
}
