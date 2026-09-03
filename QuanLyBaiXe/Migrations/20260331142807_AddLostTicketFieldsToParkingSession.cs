using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyBaiXe.Migrations
{
    /// <inheritdoc />
    public partial class AddLostTicketFieldsToParkingSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLostTicket",
                table: "ParkingSessions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "ParkingSessions",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLostTicket",
                table: "ParkingSessions");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "ParkingSessions");
        }
    }
}
