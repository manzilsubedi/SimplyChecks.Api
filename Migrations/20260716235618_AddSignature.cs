using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimplyChecks.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSignature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SignatureJson",
                table: "Inspections",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SignerName",
                table: "Inspections",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SignatureJson",
                table: "Inspections");

            migrationBuilder.DropColumn(
                name: "SignerName",
                table: "Inspections");
        }
    }
}
