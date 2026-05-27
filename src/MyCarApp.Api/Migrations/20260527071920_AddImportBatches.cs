using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyCarApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddImportBatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImportBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VehicleId = table.Column<int>(type: "integer", nullable: false),
                    ImportedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    RowCount = table.Column<int>(type: "integer", nullable: false),
                    IsLatest = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportBatches_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImportBatches_VehicleId",
                table: "ImportBatches",
                column: "VehicleId");

            migrationBuilder.AddColumn<int>(
                name: "ImportBatchId",
                table: "LogEntries",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LogEntries_ImportBatchId",
                table: "LogEntries",
                column: "ImportBatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_LogEntries_ImportBatches_ImportBatchId",
                table: "LogEntries",
                column: "ImportBatchId",
                principalTable: "ImportBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LogEntries_ImportBatches_ImportBatchId",
                table: "LogEntries");

            migrationBuilder.DropIndex(
                name: "IX_LogEntries_ImportBatchId",
                table: "LogEntries");

            migrationBuilder.DropColumn(
                name: "ImportBatchId",
                table: "LogEntries");

            migrationBuilder.DropTable(
                name: "ImportBatches");
        }
    }
}