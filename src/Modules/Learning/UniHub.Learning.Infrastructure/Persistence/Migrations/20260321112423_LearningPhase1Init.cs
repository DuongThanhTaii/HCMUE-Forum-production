using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniHub.Learning.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LearningPhase1Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LearningCourses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Semester = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FacultyId = table.Column<Guid>(type: "uuid", nullable: true),
                    Credits = table.Column<int>(type: "integer", nullable: false),
                    DocumentCount = table.Column<int>(type: "integer", nullable: false),
                    EnrollmentCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ModeratorIds = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningCourses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LearningDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    File = table.Column<string>(type: "jsonb", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UploaderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewerId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewComment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DownloadCount = table.Column<int>(type: "integer", nullable: false),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    AverageRating = table.Column<double>(type: "double precision", nullable: false),
                    RatingCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LearningFaculties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ManagerId = table.Column<Guid>(type: "uuid", nullable: true),
                    CourseCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningFaculties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LearningStoredEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AggregateId = table.Column<Guid>(type: "uuid", nullable: false),
                    AggregateType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EventType = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    EventData = table.Column<string>(type: "jsonb", nullable: false),
                    OccurredOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StoredOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningStoredEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearningCourses_Code",
                table: "LearningCourses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearningCourses_CreatedBy",
                table: "LearningCourses",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_LearningCourses_FacultyId",
                table: "LearningCourses",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningCourses_Status",
                table: "LearningCourses",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LearningDocuments_CourseId",
                table: "LearningDocuments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningDocuments_ReviewerId",
                table: "LearningDocuments",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningDocuments_Status",
                table: "LearningDocuments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LearningDocuments_Type",
                table: "LearningDocuments",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_LearningDocuments_UploaderId",
                table: "LearningDocuments",
                column: "UploaderId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningFaculties_Code",
                table: "LearningFaculties",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearningFaculties_Status",
                table: "LearningFaculties",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LearningStoredEvents_AggregateId",
                table: "LearningStoredEvents",
                column: "AggregateId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningStoredEvents_EventType",
                table: "LearningStoredEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_LearningStoredEvents_OccurredOn",
                table: "LearningStoredEvents",
                column: "OccurredOn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearningCourses");

            migrationBuilder.DropTable(
                name: "LearningDocuments");

            migrationBuilder.DropTable(
                name: "LearningFaculties");

            migrationBuilder.DropTable(
                name: "LearningStoredEvents");
        }
    }
}
