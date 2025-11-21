using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThreadLab.Migrations
{
    /// <inheritdoc />
    public partial class initialDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ThreadJobs",
                columns: table => new
                {
                    ThreadJobId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ManagedThreadId = table.Column<int>(type: "int", nullable: false),
                    IsBackground = table.Column<bool>(type: "bit", nullable: false),
                    NumberOfThreads = table.Column<int>(type: "int", nullable: false),
                    NumberOfStepsPerThread = table.Column<int>(type: "int", nullable: false),
                    DateTimeStarted = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateTimeFinished = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThreadJobs", x => x.ThreadJobId);
                });

            migrationBuilder.CreateTable(
                name: "ThreadIterations",
                columns: table => new
                {
                    ThreadIterationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ThreadJobId = table.Column<int>(type: "int", nullable: false),
                    ManagedThreadId = table.Column<int>(type: "int", nullable: false),
                    IsBackground = table.Column<bool>(type: "bit", nullable: false),
                    StartNumber = table.Column<long>(type: "bigint", nullable: false),
                    EndNumber = table.Column<long>(type: "bigint", nullable: false),
                    DateTimeStarted = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateTimeFinished = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThreadIterations", x => x.ThreadIterationId);
                    table.ForeignKey(
                        name: "FK_ThreadIterations_ThreadJobs_ThreadJobId",
                        column: x => x.ThreadJobId,
                        principalTable: "ThreadJobs",
                        principalColumn: "ThreadJobId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ThreadIterations_ThreadJobId",
                table: "ThreadIterations",
                column: "ThreadJobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThreadIterations");

            migrationBuilder.DropTable(
                name: "ThreadJobs");
        }
    }
}
