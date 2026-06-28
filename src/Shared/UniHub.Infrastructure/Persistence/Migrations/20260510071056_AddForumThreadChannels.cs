using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddForumThreadChannels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS forum.thread_channels (
                    id uuid NOT NULL PRIMARY KEY,
                    code character varying(64) NOT NULL,
                    name character varying(120) NOT NULL,
                    description character varying(500),
                    display_order integer NOT NULL DEFAULT 0,
                    is_active boolean NOT NULL DEFAULT true,
                    allow_pinned_comments boolean NOT NULL DEFAULT true,
                    allow_accepted_answers boolean NOT NULL DEFAULT true,
                    allow_moderator_actions boolean NOT NULL DEFAULT true,
                    created_at timestamp with time zone NOT NULL,
                    updated_at timestamp with time zone
                );
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE forum.posts
                    ADD COLUMN IF NOT EXISTS thread_channel_id uuid;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE forum.comments
                    ADD COLUMN IF NOT EXISTS is_pinned boolean NOT NULL DEFAULT false;
                """);

            migrationBuilder.Sql(
                """
                CREATE UNIQUE INDEX IF NOT EXISTS ix_thread_channels_code
                    ON forum.thread_channels (code);
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS ix_thread_channels_is_active_display_order
                    ON forum.thread_channels (is_active, display_order);
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS ix_posts_thread_channel_id
                    ON forum.posts (thread_channel_id);
                """);

            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conname = 'fk_posts_thread_channels_thread_channel_id'
                    ) THEN
                        ALTER TABLE forum.posts
                            ADD CONSTRAINT fk_posts_thread_channels_thread_channel_id
                            FOREIGN KEY (thread_channel_id)
                            REFERENCES forum.thread_channels (id)
                            ON DELETE SET NULL;
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE forum.posts
                    DROP CONSTRAINT IF EXISTS fk_posts_thread_channels_thread_channel_id;
                """);

            migrationBuilder.Sql(
                """
                DROP INDEX IF EXISTS forum.ix_posts_thread_channel_id;
                DROP INDEX IF EXISTS forum.ix_thread_channels_is_active_display_order;
                DROP INDEX IF EXISTS forum.ix_thread_channels_code;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE forum.posts
                    DROP COLUMN IF EXISTS thread_channel_id;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE forum.comments
                    DROP COLUMN IF EXISTS is_pinned;
                """);

            migrationBuilder.Sql(
                """
                DROP TABLE IF EXISTS forum.thread_channels;
                """);
        }
    }
}
