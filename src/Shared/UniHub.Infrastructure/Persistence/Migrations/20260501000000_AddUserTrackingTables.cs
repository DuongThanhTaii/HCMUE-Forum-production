using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using UniHub.Infrastructure.Persistence;

#nullable disable

namespace UniHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260501000000_AddUserTrackingTables")]
    public partial class AddUserTrackingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS user_document_downloads (
                    user_id     UUID        NOT NULL,
                    document_id UUID        NOT NULL,
                    downloaded_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                    CONSTRAINT pk_user_document_downloads PRIMARY KEY (user_id, document_id)
                );
                CREATE INDEX IF NOT EXISTS ix_udd_document ON user_document_downloads (document_id);
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS user_document_ratings (
                    user_id     UUID        NOT NULL,
                    document_id UUID        NOT NULL,
                    rating      INTEGER     NOT NULL CHECK (rating BETWEEN 1 AND 5),
                    rated_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                    CONSTRAINT pk_user_document_ratings PRIMARY KEY (user_id, document_id)
                );
                CREATE INDEX IF NOT EXISTS ix_udr_document ON user_document_ratings (document_id);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS user_document_ratings;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS user_document_downloads;");
        }
    }
}
