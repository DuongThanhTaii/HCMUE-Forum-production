using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniHub.Career.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CareerPhase1Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CareerApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobPostingId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Resume = table.Column<string>(type: "jsonb", nullable: false),
                    CoverLetter = table.Column<string>(type: "text", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastStatusChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastStatusChangedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewNotes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareerApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CareerCompanies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    Industry = table.Column<int>(type: "integer", nullable: false),
                    Size = table.Column<int>(type: "integer", nullable: false),
                    Website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LogoUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FoundedYear = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ContactInfo = table.Column<string>(type: "jsonb", nullable: false),
                    SocialLinks = table.Column<string>(type: "jsonb", nullable: false),
                    RegisteredBy = table.Column<Guid>(type: "uuid", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalJobPostings = table.Column<int>(type: "integer", nullable: false),
                    Benefits = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareerCompanies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CareerJobPostings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    JobType = table.Column<int>(type: "integer", nullable: false),
                    ExperienceLevel = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Salary = table.Column<string>(type: "text", nullable: true),
                    Location = table.Column<string>(type: "jsonb", nullable: false),
                    Deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    ApplicationCount = table.Column<int>(type: "integer", nullable: false),
                    Requirements = table.Column<string>(type: "jsonb", nullable: false),
                    Tags = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareerJobPostings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CareerRecruiters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Permissions = table.Column<string>(type: "jsonb", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AddedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareerRecruiters", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CareerApplications_ApplicantId",
                table: "CareerApplications",
                column: "ApplicantId");

            migrationBuilder.CreateIndex(
                name: "IX_CareerApplications_JobPostingId",
                table: "CareerApplications",
                column: "JobPostingId");

            migrationBuilder.CreateIndex(
                name: "IX_CareerApplications_Status",
                table: "CareerApplications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CareerApplications_SubmittedAt",
                table: "CareerApplications",
                column: "SubmittedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CareerCompanies_Name",
                table: "CareerCompanies",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CareerCompanies_RegisteredBy",
                table: "CareerCompanies",
                column: "RegisteredBy");

            migrationBuilder.CreateIndex(
                name: "IX_CareerCompanies_Status",
                table: "CareerCompanies",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CareerJobPostings_CompanyId",
                table: "CareerJobPostings",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CareerJobPostings_Deadline",
                table: "CareerJobPostings",
                column: "Deadline");

            migrationBuilder.CreateIndex(
                name: "IX_CareerJobPostings_PostedBy",
                table: "CareerJobPostings",
                column: "PostedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CareerJobPostings_Status",
                table: "CareerJobPostings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CareerRecruiters_CompanyId",
                table: "CareerRecruiters",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CareerRecruiters_Status",
                table: "CareerRecruiters",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CareerRecruiters_UserId",
                table: "CareerRecruiters",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CareerApplications");

            migrationBuilder.DropTable(
                name: "CareerCompanies");

            migrationBuilder.DropTable(
                name: "CareerJobPostings");

            migrationBuilder.DropTable(
                name: "CareerRecruiters");
        }
    }
}
