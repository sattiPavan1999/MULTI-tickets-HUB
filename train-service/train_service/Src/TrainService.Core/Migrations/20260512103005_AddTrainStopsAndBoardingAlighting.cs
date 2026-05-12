using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TrainService.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainStopsAndBoardingAlighting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlightingStation",
                schema: "trains",
                table: "Bookings",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BoardingStation",
                schema: "trains",
                table: "Bookings",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TrainStops",
                schema: "trains",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrainId = table.Column<int>(type: "integer", nullable: false),
                    StopNumber = table.Column<int>(type: "integer", nullable: false),
                    StationName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainStops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainStops_Trains_TrainId",
                        column: x => x.TrainId,
                        principalSchema: "trains",
                        principalTable: "Trains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrainStops_TrainId_StopNumber",
                schema: "trains",
                table: "TrainStops",
                columns: new[] { "TrainId", "StopNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrainStops",
                schema: "trains");

            migrationBuilder.DropColumn(
                name: "AlightingStation",
                schema: "trains",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "BoardingStation",
                schema: "trains",
                table: "Bookings");
        }
    }
}
