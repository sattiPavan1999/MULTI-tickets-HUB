using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TrainService.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Trains",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrainNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TrainName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SourceStation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DestinationStation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DepartureTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    ArrivalTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    TotalSeats = table.Column<string>(type: "jsonb", nullable: false),
                    Fares = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trains", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrainBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PNR = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    TrainId = table.Column<int>(type: "integer", nullable: false),
                    TravelDate = table.Column<DateOnly>(type: "date", nullable: false),
                    SeatClass = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PassengerDetails = table.Column<string>(type: "jsonb", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BookedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainBookings", x => x.Id);
                    table.CheckConstraint("CK_TrainBooking_SeatClass", "\"SeatClass\" IN ('Sleeper', '3AC', '2AC', '1AC')");
                    table.CheckConstraint("CK_TrainBooking_Status", "\"Status\" IN ('Confirmed', 'Cancelled')");
                    table.CheckConstraint("CK_TrainBooking_TotalAmount", "\"TotalAmount\" > 0");
                    table.ForeignKey(
                        name: "FK_TrainBookings_Trains_TrainId",
                        column: x => x.TrainId,
                        principalTable: "Trains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrainBookings_PNR",
                table: "TrainBookings",
                column: "PNR",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainBookings_TrainId_TravelDate_Status",
                table: "TrainBookings",
                columns: new[] { "TrainId", "TravelDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainBookings_UserId",
                table: "TrainBookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Trains_SourceStation_DestinationStation",
                table: "Trains",
                columns: new[] { "SourceStation", "DestinationStation" });

            migrationBuilder.CreateIndex(
                name: "IX_Trains_TrainNumber",
                table: "Trains",
                column: "TrainNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrainBookings");

            migrationBuilder.DropTable(
                name: "Trains");
        }
    }
}
