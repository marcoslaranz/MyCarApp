using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyCarApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceLogsAndDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VehicleId = table.Column<int>(type: "integer", nullable: false),
                    ServiceDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    OdometerKm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceLogs_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServiceLogId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    FileUrl = table.Column<string>(type: "text", nullable: false),
                    FileType = table.Column<string>(type: "text", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceDocuments_ServiceLogs_ServiceLogId",
                        column: x => x.ServiceLogId,
                        principalTable: "ServiceLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceLogItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServiceLogId = table.Column<int>(type: "integer", nullable: false),
                    ServiceItemId = table.Column<int>(type: "integer", nullable: false),
                    Done = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceLogItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceLogItems_ServiceItems_ServiceItemId",
                        column: x => x.ServiceItemId,
                        principalTable: "ServiceItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceLogItems_ServiceLogs_ServiceLogId",
                        column: x => x.ServiceLogId,
                        principalTable: "ServiceLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceDocuments_ServiceLogId",
                table: "ServiceDocuments",
                column: "ServiceLogId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceLogItems_ServiceItemId",
                table: "ServiceLogItems",
                column: "ServiceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceLogItems_ServiceLogId",
                table: "ServiceLogItems",
                column: "ServiceLogId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceLogs_VehicleId",
                table: "ServiceLogs",
                column: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceDocuments");

            migrationBuilder.DropTable(
                name: "ServiceLogItems");

            migrationBuilder.DropTable(
                name: "ServiceLogs");
        }
    }
}
