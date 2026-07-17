using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SimplyChecks.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CheckItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Label = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Tenant = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Inspections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    DeviceId = table.Column<string>(type: "text", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inspections_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InspectionResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    PhotoFileName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionResults_Inspections_InspectionId",
                        column: x => x.InspectionId,
                        principalTable: "Inspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CheckItems",
                columns: new[] { "Id", "Category", "Label", "SortOrder" },
                values: new object[,]
                {
                    { "bathroom-mould", "Bathroom", "No mould or damp", 9 },
                    { "bathroom-shower", "Bathroom", "Shower / bath condition", 8 },
                    { "bathroom-toilet", "Bathroom", "Toilet flushing properly", 7 },
                    { "co-detector", "Safety", "Carbon monoxide detector", 2 },
                    { "doors-locks", "General", "Doors and locks working", 12 },
                    { "electrical", "General", "Power points and lights", 15 },
                    { "exits-clear", "Safety", "Exits clear and accessible", 3 },
                    { "flooring", "General", "Flooring condition", 13 },
                    { "heating", "General", "Heating system working", 14 },
                    { "kitchen-appliances", "Kitchen", "Appliances working", 5 },
                    { "kitchen-cabinets", "Kitchen", "Cabinets and drawers", 6 },
                    { "kitchen-taps", "Kitchen", "Taps and sink drainage", 4 },
                    { "smoke-alarms", "Safety", "Smoke alarms working", 1 },
                    { "walls-ceilings", "General", "Walls and ceilings condition", 10 },
                    { "windows-locks", "General", "Windows open and lock", 11 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionResults_InspectionId",
                table: "InspectionResults",
                column: "InspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_JobId_DeviceId",
                table: "Inspections",
                columns: new[] { "JobId", "DeviceId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckItems");

            migrationBuilder.DropTable(
                name: "InspectionResults");

            migrationBuilder.DropTable(
                name: "Inspections");

            migrationBuilder.DropTable(
                name: "Jobs");
        }
    }
}
