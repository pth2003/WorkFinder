using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WorkFinder.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddJobAlertsAndNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "job_alerts",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Keywords = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: true),
                    JobType = table.Column<string>(type: "text", nullable: true),
                    ExperienceLevel = table.Column<string>(type: "text", nullable: true),
                    MinSalary = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MaxSalary = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    IsDaily = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastSentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_job_alerts_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "public",
                        principalTable: "categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_job_alerts_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Link = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    JobId = table.Column<int>(type: "integer", nullable: true),
                    JobApplicationId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notifications_job_applications_JobApplicationId",
                        column: x => x.JobApplicationId,
                        principalSchema: "public",
                        principalTable: "job_applications",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_notifications_jobs_JobId",
                        column: x => x.JobId,
                        principalSchema: "public",
                        principalTable: "jobs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_notifications_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_job_alerts_CategoryId",
                schema: "public",
                table: "job_alerts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_job_alerts_UserId",
                schema: "public",
                table: "job_alerts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_JobApplicationId",
                schema: "public",
                table: "notifications",
                column: "JobApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_JobId",
                schema: "public",
                table: "notifications",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_UserId",
                schema: "public",
                table: "notifications",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "job_alerts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "public");
        }
    }
}
