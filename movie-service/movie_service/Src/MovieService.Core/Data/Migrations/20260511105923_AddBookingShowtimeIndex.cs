using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieService.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingShowtimeIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "movies",
                table: "Bookings",
                type: "text",
                nullable: false,
                defaultValue: "Confirmed",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Pending");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "movies",
                table: "Bookings",
                type: "text",
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Confirmed");
        }
    }
}
