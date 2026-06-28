using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UniHub.Forum.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ForumPhase2FullSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ForumBookmarks",
                columns: table => new
                {
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForumBookmarks", x => new { x.UserId, x.PostId });
                });

            migrationBuilder.CreateTable(
                name: "ForumComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    ParentCommentId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsAcceptedAnswer = table.Column<bool>(type: "boolean", nullable: false),
                    VoteScore = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForumComments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ForumPostTags",
                columns: table => new
                {
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<int>(type: "integer", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForumPostTags", x => new { x.PostId, x.TagId });
                });

            migrationBuilder.CreateTable(
                name: "ForumPostVotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForumPostVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForumPostVotes_ForumPosts_PostId",
                        column: x => x.PostId,
                        principalTable: "ForumPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ForumReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    ReportedItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportedItemType = table.Column<int>(type: "integer", nullable: false),
                    ReporterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForumReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ForumTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Slug = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    UsageCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForumTags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ForumCommentVotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CommentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForumCommentVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForumCommentVotes_ForumComments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "ForumComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ForumPosts_AuthorId",
                table: "ForumPosts",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_ForumPosts_CategoryId",
                table: "ForumPosts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ForumBookmarks_PostId",
                table: "ForumBookmarks",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_ForumComments_PostId",
                table: "ForumComments",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_ForumCommentVotes_CommentId",
                table: "ForumCommentVotes",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_ForumPostTags_TagId",
                table: "ForumPostTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_ForumPostVotes_PostId",
                table: "ForumPostVotes",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_ForumReports_ReporterId_ReportedItemId_ReportedItemType",
                table: "ForumReports",
                columns: new[] { "ReporterId", "ReportedItemId", "ReportedItemType" });

            migrationBuilder.CreateIndex(
                name: "IX_ForumReports_Status",
                table: "ForumReports",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ForumTags_Name",
                table: "ForumTags",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ForumTags_Slug",
                table: "ForumTags",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ForumBookmarks");

            migrationBuilder.DropTable(
                name: "ForumCommentVotes");

            migrationBuilder.DropTable(
                name: "ForumPostTags");

            migrationBuilder.DropTable(
                name: "ForumPostVotes");

            migrationBuilder.DropTable(
                name: "ForumReports");

            migrationBuilder.DropTable(
                name: "ForumTags");

            migrationBuilder.DropTable(
                name: "ForumComments");

            migrationBuilder.DropIndex(
                name: "IX_ForumPosts_AuthorId",
                table: "ForumPosts");

            migrationBuilder.DropIndex(
                name: "IX_ForumPosts_CategoryId",
                table: "ForumPosts");
        }
    }
}
