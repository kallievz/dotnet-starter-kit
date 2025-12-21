using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FSH.Playground.Migrations.PostgreSQL.Identity
{
    /// <inheritdoc />
    public partial class AddPasswordHistoryAndExpiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add LastPasswordChangeDate column to Users table
            migrationBuilder.AddColumn<DateTime>(
                name: "LastPasswordChangeDate",
                schema: "identity",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            // Create PasswordHistory table
            migrationBuilder.CreateTable(
                name: "PasswordHistory",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordHistory_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes for PasswordHistory
            migrationBuilder.CreateIndex(
                name: "IX_PasswordHistory_UserId",
                schema: "identity",
                table: "PasswordHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordHistory_UserId_CreatedAt",
                schema: "identity",
                table: "PasswordHistory",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop PasswordHistory table and its indexes
            migrationBuilder.DropTable(
                name: "PasswordHistory",
                schema: "identity");

            // Remove LastPasswordChangeDate column from Users table
            migrationBuilder.DropColumn(
                name: "LastPasswordChangeDate",
                schema: "identity",
                table: "Users");
        }
    }
}
