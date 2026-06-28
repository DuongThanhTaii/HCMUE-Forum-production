using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniHub.AI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AIPhase1Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIConversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SessionId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HandedOffToSupport = table.Column<bool>(type: "boolean", nullable: false),
                    HandoffReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SupportAgentId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastActiveAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIConversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AIFAQItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Question = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Answer = table.Column<string>(type: "character varying(12000)", maxLength: 12000, nullable: false),
                    Category = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Tags = table.Column<string>(type: "jsonb", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UsageCount = table.Column<int>(type: "integer", nullable: false),
                    AverageRating = table.Column<double>(type: "double precision", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIFAQItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AISearchHistories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Query = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    NormalizedQuery = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SearchType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ResultCount = table.Column<int>(type: "integer", nullable: false),
                    HadClickthrough = table.Column<bool>(type: "boolean", nullable: false),
                    ClickedResultId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProcessingTimeMs = table.Column<long>(type: "bigint", nullable: false),
                    SearchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SessionId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Language = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AISearchHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AISummaryCacheEntries",
                columns: table => new
                {
                    CacheKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Summary = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: false),
                    KeyPoints = table.Column<string>(type: "jsonb", nullable: false),
                    DetectedLanguage = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    OriginalLength = table.Column<int>(type: "integer", nullable: false),
                    SummaryLength = table.Column<int>(type: "integer", nullable: false),
                    CompressionRatio = table.Column<double>(type: "double precision", nullable: false),
                    TokensUsed = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AccessCount = table.Column<int>(type: "integer", nullable: false),
                    LastAccessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AISummaryCacheEntries", x => x.CacheKey);
                });

            migrationBuilder.CreateTable(
                name: "AIConversationMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    SourceFAQId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConfidenceScore = table.Column<double>(type: "double precision", nullable: true),
                    IsHelpful = table.Column<bool>(type: "boolean", nullable: true),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIConversationMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIConversationMessages_AIConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "AIConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIConversationMessages_ConversationId",
                table: "AIConversationMessages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_AIConversationMessages_Role",
                table: "AIConversationMessages",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_AIConversationMessages_SentAt",
                table: "AIConversationMessages",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_AIConversationMessages_SourceFAQId",
                table: "AIConversationMessages",
                column: "SourceFAQId");

            migrationBuilder.CreateIndex(
                name: "IX_AIConversations_IsClosed",
                table: "AIConversations",
                column: "IsClosed");

            migrationBuilder.CreateIndex(
                name: "IX_AIConversations_LastActiveAt",
                table: "AIConversations",
                column: "LastActiveAt");

            migrationBuilder.CreateIndex(
                name: "IX_AIConversations_SessionId",
                table: "AIConversations",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AIConversations_UserId",
                table: "AIConversations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AIFAQItems_Category",
                table: "AIFAQItems",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_AIFAQItems_CreatedAt",
                table: "AIFAQItems",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AIFAQItems_IsActive",
                table: "AIFAQItems",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AIFAQItems_Priority",
                table: "AIFAQItems",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_AISearchHistories_NormalizedQuery",
                table: "AISearchHistories",
                column: "NormalizedQuery");

            migrationBuilder.CreateIndex(
                name: "IX_AISearchHistories_SearchedAt",
                table: "AISearchHistories",
                column: "SearchedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AISearchHistories_SearchType",
                table: "AISearchHistories",
                column: "SearchType");

            migrationBuilder.CreateIndex(
                name: "IX_AISearchHistories_UserId",
                table: "AISearchHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AISummaryCacheEntries_CreatedAt",
                table: "AISummaryCacheEntries",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AISummaryCacheEntries_ExpiresAt",
                table: "AISummaryCacheEntries",
                column: "ExpiresAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIConversationMessages");

            migrationBuilder.DropTable(
                name: "AIFAQItems");

            migrationBuilder.DropTable(
                name: "AISearchHistories");

            migrationBuilder.DropTable(
                name: "AISummaryCacheEntries");

            migrationBuilder.DropTable(
                name: "AIConversations");
        }
    }
}
