using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualQueue.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPriorityToUserSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "UserSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                table: "UserSessions");
        }
    }
}
