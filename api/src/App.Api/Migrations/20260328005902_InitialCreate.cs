using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "day_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    IsFinalized = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_day_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "weekly_summaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    WeekNumber = table.Column<int>(type: "integer", nullable: false),
                    WeeklyScore = table.Column<int>(type: "integer", nullable: false),
                    WeightKg = table.Column<decimal>(type: "numeric", nullable: true),
                    CalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weekly_summaries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "moment_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    DayEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Moment = table.Column<string>(type: "text", nullable: false),
                    Food = table.Column<int>(type: "integer", nullable: false),
                    Exercise = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_moment_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_moment_entries_day_entries_DayEntryId",
                        column: x => x.DayEntryId,
                        principalTable: "day_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_day_entries_Date",
                table: "day_entries",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_moment_entries_DayEntryId",
                table: "moment_entries",
                column: "DayEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_weekly_summaries_Year_WeekNumber",
                table: "weekly_summaries",
                columns: new[] { "Year", "WeekNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "moment_entries");

            migrationBuilder.DropTable(
                name: "weekly_summaries");

            migrationBuilder.DropTable(
                name: "day_entries");
        }
    }
}
