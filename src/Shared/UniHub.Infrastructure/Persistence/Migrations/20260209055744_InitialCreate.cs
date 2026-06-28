using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UniHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "career");

            migrationBuilder.EnsureSchema(
                name: "forum");

            migrationBuilder.EnsureSchema(
                name: "chat");

            migrationBuilder.EnsureSchema(
                name: "learning");

            migrationBuilder.EnsureSchema(
                name: "notification");

            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.CreateTable(
                name: "applications",
                schema: "career",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_posting_id = table.Column<Guid>(type: "uuid", nullable: false),
                    applicant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    resume_file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    resume_file_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    resume_file_size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    resume_content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    cover_letter_content = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_status_changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_status_changed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    review_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_applications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bookmarks",
                schema: "forum",
                columns: table => new
                {
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookmarks", x => new { x.post_id, x.user_id });
                });

            migrationBuilder.CreateTable(
                name: "categories",
                schema: "forum",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    slug = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    parent_category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    post_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    moderator_ids = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "channels",
                schema: "chat",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    archived_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    members = table.Column<string>(type: "jsonb", nullable: false),
                    moderators = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_channels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "comments",
                schema: "forum",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    parent_comment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_accepted_answer = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    vote_score = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "companies",
                schema: "career",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    industry = table.Column<int>(type: "integer", nullable: false),
                    size = table.Column<int>(type: "integer", nullable: false),
                    website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    logo_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    founded_year = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    contact_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    contact_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    contact_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    social_linkedin = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    social_facebook = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    social_twitter = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    social_instagram = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    social_youtube = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    registered_by = table.Column<Guid>(type: "uuid", nullable: false),
                    registered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    verified_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    total_job_postings = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    benefits = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_companies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "conversations",
                schema: "chat",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_message_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    participants = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "courses",
                schema: "learning",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    semester = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    faculty_id = table.Column<Guid>(type: "uuid", nullable: true),
                    credits = table.Column<int>(type: "integer", nullable: false),
                    document_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    enrollment_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    moderator_ids = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_courses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "documents",
                schema: "learning",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    file_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    file_extension = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    uploader_id = table.Column<Guid>(type: "uuid", nullable: false),
                    course_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    review_comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    download_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    view_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    average_rating = table.Column<double>(type: "double precision", precision: 3, scale: 2, nullable: false, defaultValue: 0.0),
                    rating_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "faculties",
                schema: "learning",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    manager_id = table.Column<Guid>(type: "uuid", nullable: true),
                    course_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_faculties", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "job_postings",
                schema: "career",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    posted_by = table.Column<Guid>(type: "uuid", nullable: false),
                    job_type = table.Column<int>(type: "integer", nullable: false),
                    experience_level = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    salary_min_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    salary_max_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    salary_currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    salary_period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    location_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    location_district = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    location_address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    location_is_remote = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    view_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    application_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    tags = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_postings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                schema: "chat",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    conversation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    edited_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reply_to_message_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notification_preferences",
                schema: "notification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    push_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    in_app_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_preferences", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notification_templates",
                schema: "notification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    category = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    email_subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    email_body = table.Column<string>(type: "character varying(50000)", maxLength: 50000, nullable: true),
                    email_from_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    email_from_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    push_title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    push_body = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    push_icon_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    push_badge_count = table.Column<int>(type: "integer", nullable: true),
                    inapp_title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    inapp_body = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    inapp_action_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    inapp_icon_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    activated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    activated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    channels = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "notification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_id = table.Column<Guid>(type: "uuid", nullable: true),
                    channel = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    content_subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    content_body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    content_action_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    content_icon_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    metadata = table.Column<Dictionary<string, string>>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dismissed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    send_attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "password_reset_tokens",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_used = table.Column<bool>(type: "boolean", nullable: false),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_password_reset_tokens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    resource = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "post_tags",
                schema: "forum",
                columns: table => new
                {
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<int>(type: "integer", nullable: false),
                    added_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post_tags", x => new { x.post_id, x.tag_id });
                });

            migrationBuilder.CreateTable(
                name: "posts",
                schema: "forum",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    content = table.Column<string>(type: "character varying(50000)", maxLength: 50000, nullable: false),
                    slug = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    view_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    vote_score = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    comment_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_pinned = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_locked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tags = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_posts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "recruiters",
                schema: "career",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    can_manage_job_postings = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    can_review_applications = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    can_update_application_status = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    can_invite_recruiters = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    added_by = table.Column<Guid>(type: "uuid", nullable: false),
                    added_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recruiters", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reports",
                schema: "forum",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    reported_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reported_item_type = table.Column<int>(type: "integer", nullable: false),
                    reporter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reviewed_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reports", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    is_system_role = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                schema: "forum",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    usage_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    avatar = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    bio = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    phone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    badge_type = table.Column<int>(type: "integer", nullable: true),
                    badge_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    badge_description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    badge_verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    badge_verified_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "comment_votes",
                schema: "forum",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vote_type = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comment_votes", x => new { x.comment_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_comment_votes_comments_comment_id",
                        column: x => x.comment_id,
                        principalSchema: "forum",
                        principalTable: "comments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "job_posting_requirements",
                schema: "career",
                columns: table => new
                {
                    skill = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    job_posting_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_posting_requirements", x => new { x.job_posting_id, x.skill });
                    table.ForeignKey(
                        name: "FK_job_posting_requirements_job_postings_job_posting_id",
                        column: x => x.job_posting_id,
                        principalSchema: "career",
                        principalTable: "job_postings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "message_attachments",
                schema: "chat",
                columns: table => new
                {
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    mime_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    thumbnail_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_message_attachments", x => new { x.message_id, x.file_name });
                    table.ForeignKey(
                        name: "FK_message_attachments_messages_message_id",
                        column: x => x.message_id,
                        principalSchema: "chat",
                        principalTable: "messages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "message_reactions",
                schema: "chat",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    emoji = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reacted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_message_reactions", x => new { x.message_id, x.user_id, x.emoji });
                    table.ForeignKey(
                        name: "FK_message_reactions_messages_message_id",
                        column: x => x.message_id,
                        principalSchema: "chat",
                        principalTable: "messages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "message_read_receipts",
                schema: "chat",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_message_read_receipts", x => new { x.message_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_message_read_receipts_messages_message_id",
                        column: x => x.message_id,
                        principalSchema: "chat",
                        principalTable: "messages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notification_template_variables",
                schema: "notification",
                columns: table => new
                {
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    example_value = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_template_variables", x => new { x.template_id, x.name });
                    table.ForeignKey(
                        name: "FK_notification_template_variables_notification_templates_temp~",
                        column: x => x.template_id,
                        principalSchema: "notification",
                        principalTable: "notification_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post_votes",
                schema: "forum",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vote_type = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post_votes", x => new { x.post_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_post_votes_posts_post_id",
                        column: x => x.post_id,
                        principalSchema: "forum",
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    scope_type = table.Column<int>(type: "integer", nullable: false),
                    scope_value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_role_permissions_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "identity",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_ip = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    revoked_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    revoked_by_ip = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    replaced_by_token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    revoke_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_applications_applicant_id",
                schema: "career",
                table: "applications",
                column: "applicant_id");

            migrationBuilder.CreateIndex(
                name: "IX_applications_job_posting_id",
                schema: "career",
                table: "applications",
                column: "job_posting_id");

            migrationBuilder.CreateIndex(
                name: "IX_applications_job_posting_id_applicant_id",
                schema: "career",
                table: "applications",
                columns: new[] { "job_posting_id", "applicant_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_applications_status",
                schema: "career",
                table: "applications",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_applications_submitted_at",
                schema: "career",
                table: "applications",
                column: "submitted_at");

            migrationBuilder.CreateIndex(
                name: "IX_bookmarks_created_at",
                schema: "forum",
                table: "bookmarks",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_bookmarks_post_id",
                schema: "forum",
                table: "bookmarks",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_bookmarks_user_id",
                schema: "forum",
                table: "bookmarks",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_categories_display_order",
                schema: "forum",
                table: "categories",
                column: "display_order");

            migrationBuilder.CreateIndex(
                name: "IX_categories_is_active",
                schema: "forum",
                table: "categories",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_categories_name",
                schema: "forum",
                table: "categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_categories_parent_category_id",
                schema: "forum",
                table: "categories",
                column: "parent_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_categories_slug",
                schema: "forum",
                table: "categories",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_channels_is_archived",
                schema: "chat",
                table: "channels",
                column: "is_archived");

            migrationBuilder.CreateIndex(
                name: "IX_channels_name",
                schema: "chat",
                table: "channels",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_channels_owner_id",
                schema: "chat",
                table: "channels",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_channels_type",
                schema: "chat",
                table: "channels",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_comments_author_id",
                schema: "forum",
                table: "comments",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "IX_comments_created_at",
                schema: "forum",
                table: "comments",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_comments_parent_comment_id",
                schema: "forum",
                table: "comments",
                column: "parent_comment_id");

            migrationBuilder.CreateIndex(
                name: "IX_comments_post_id",
                schema: "forum",
                table: "comments",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_companies_industry",
                schema: "career",
                table: "companies",
                column: "industry");

            migrationBuilder.CreateIndex(
                name: "IX_companies_name",
                schema: "career",
                table: "companies",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_companies_registered_at",
                schema: "career",
                table: "companies",
                column: "registered_at");

            migrationBuilder.CreateIndex(
                name: "IX_companies_registered_by",
                schema: "career",
                table: "companies",
                column: "registered_by");

            migrationBuilder.CreateIndex(
                name: "IX_companies_status",
                schema: "career",
                table: "companies",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_conversations_created_by",
                schema: "chat",
                table: "conversations",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_conversations_is_archived",
                schema: "chat",
                table: "conversations",
                column: "is_archived");

            migrationBuilder.CreateIndex(
                name: "IX_conversations_last_message_at",
                schema: "chat",
                table: "conversations",
                column: "last_message_at");

            migrationBuilder.CreateIndex(
                name: "IX_conversations_type",
                schema: "chat",
                table: "conversations",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_courses_code",
                schema: "learning",
                table: "courses",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_courses_created_at",
                schema: "learning",
                table: "courses",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_courses_created_by",
                schema: "learning",
                table: "courses",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_courses_faculty_id",
                schema: "learning",
                table: "courses",
                column: "faculty_id");

            migrationBuilder.CreateIndex(
                name: "IX_courses_status",
                schema: "learning",
                table: "courses",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_documents_course_id",
                schema: "learning",
                table: "documents",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_documents_created_at",
                schema: "learning",
                table: "documents",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_documents_status",
                schema: "learning",
                table: "documents",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_documents_type",
                schema: "learning",
                table: "documents",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_documents_uploader_id",
                schema: "learning",
                table: "documents",
                column: "uploader_id");

            migrationBuilder.CreateIndex(
                name: "IX_faculties_code",
                schema: "learning",
                table: "faculties",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_faculties_created_at",
                schema: "learning",
                table: "faculties",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_faculties_manager_id",
                schema: "learning",
                table: "faculties",
                column: "manager_id");

            migrationBuilder.CreateIndex(
                name: "IX_faculties_name",
                schema: "learning",
                table: "faculties",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_faculties_status",
                schema: "learning",
                table: "faculties",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_job_postings_company_id",
                schema: "career",
                table: "job_postings",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_postings_created_at",
                schema: "career",
                table: "job_postings",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_job_postings_deadline",
                schema: "career",
                table: "job_postings",
                column: "deadline");

            migrationBuilder.CreateIndex(
                name: "IX_job_postings_experience_level",
                schema: "career",
                table: "job_postings",
                column: "experience_level");

            migrationBuilder.CreateIndex(
                name: "IX_job_postings_job_type",
                schema: "career",
                table: "job_postings",
                column: "job_type");

            migrationBuilder.CreateIndex(
                name: "IX_job_postings_posted_by",
                schema: "career",
                table: "job_postings",
                column: "posted_by");

            migrationBuilder.CreateIndex(
                name: "IX_job_postings_status",
                schema: "career",
                table: "job_postings",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_messages_conversation_id",
                schema: "chat",
                table: "messages",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_is_deleted",
                schema: "chat",
                table: "messages",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_messages_reply_to_message_id",
                schema: "chat",
                table: "messages",
                column: "reply_to_message_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_sender_id",
                schema: "chat",
                table: "messages",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_sent_at",
                schema: "chat",
                table: "messages",
                column: "sent_at");

            migrationBuilder.CreateIndex(
                name: "IX_notification_preferences_user_id",
                schema: "notification",
                table: "notification_preferences",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notification_templates_category",
                schema: "notification",
                table: "notification_templates",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "IX_notification_templates_created_at",
                schema: "notification",
                table: "notification_templates",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_notification_templates_name",
                schema: "notification",
                table: "notification_templates",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notification_templates_status",
                schema: "notification",
                table: "notification_templates",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_channel",
                schema: "notification",
                table: "notifications",
                column: "channel");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_created_at",
                schema: "notification",
                table: "notifications",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_recipient_id",
                schema: "notification",
                table: "notifications",
                column: "recipient_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_recipient_id_status",
                schema: "notification",
                table: "notifications",
                columns: new[] { "recipient_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_status",
                schema: "notification",
                table: "notifications",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_template_id",
                schema: "notification",
                table: "notifications",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_tokens_token",
                schema: "identity",
                table: "password_reset_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_permissions_code",
                schema: "identity",
                table: "permissions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_post_tags_post_id",
                schema: "forum",
                table: "post_tags",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_post_tags_tag_id",
                schema: "forum",
                table: "post_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_posts_author_id",
                schema: "forum",
                table: "posts",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "IX_posts_category_id",
                schema: "forum",
                table: "posts",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_posts_created_at",
                schema: "forum",
                table: "posts",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_posts_slug",
                schema: "forum",
                table: "posts",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_posts_status",
                schema: "forum",
                table: "posts",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_recruiters_company_id",
                schema: "career",
                table: "recruiters",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_recruiters_status",
                schema: "career",
                table: "recruiters",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_recruiters_user_id",
                schema: "career",
                table: "recruiters",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_recruiters_user_id_company_id",
                schema: "career",
                table: "recruiters",
                columns: new[] { "user_id", "company_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_token",
                schema: "identity",
                table: "refresh_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_user_id",
                schema: "identity",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_reports_created_at",
                schema: "forum",
                table: "reports",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_reports_reported_item_id",
                schema: "forum",
                table: "reports",
                column: "reported_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_reports_reported_item_id_reported_item_type",
                schema: "forum",
                table: "reports",
                columns: new[] { "reported_item_id", "reported_item_type" });

            migrationBuilder.CreateIndex(
                name: "IX_reports_reported_item_type",
                schema: "forum",
                table: "reports",
                column: "reported_item_type");

            migrationBuilder.CreateIndex(
                name: "IX_reports_reporter_id",
                schema: "forum",
                table: "reports",
                column: "reporter_id");

            migrationBuilder.CreateIndex(
                name: "IX_reports_status",
                schema: "forum",
                table: "reports",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_role_id_permission_id",
                schema: "identity",
                table: "role_permissions",
                columns: new[] { "role_id", "permission_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_name",
                schema: "identity",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tags_created_at",
                schema: "forum",
                table: "tags",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_tags_name",
                schema: "forum",
                table: "tags",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tags_slug",
                schema: "forum",
                table: "tags",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tags_usage_count",
                schema: "forum",
                table: "tags",
                column: "usage_count");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_user_id_role_id",
                schema: "identity",
                table: "user_roles",
                columns: new[] { "user_id", "role_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                schema: "identity",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "applications",
                schema: "career");

            migrationBuilder.DropTable(
                name: "bookmarks",
                schema: "forum");

            migrationBuilder.DropTable(
                name: "categories",
                schema: "forum");

            migrationBuilder.DropTable(
                name: "channels",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "comment_votes",
                schema: "forum");

            migrationBuilder.DropTable(
                name: "companies",
                schema: "career");

            migrationBuilder.DropTable(
                name: "conversations",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "courses",
                schema: "learning");

            migrationBuilder.DropTable(
                name: "documents",
                schema: "learning");

            migrationBuilder.DropTable(
                name: "faculties",
                schema: "learning");

            migrationBuilder.DropTable(
                name: "job_posting_requirements",
                schema: "career");

            migrationBuilder.DropTable(
                name: "message_attachments",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "message_reactions",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "message_read_receipts",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "notification_preferences",
                schema: "notification");

            migrationBuilder.DropTable(
                name: "notification_template_variables",
                schema: "notification");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "notification");

            migrationBuilder.DropTable(
                name: "password_reset_tokens",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "permissions",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "post_tags",
                schema: "forum");

            migrationBuilder.DropTable(
                name: "post_votes",
                schema: "forum");

            migrationBuilder.DropTable(
                name: "recruiters",
                schema: "career");

            migrationBuilder.DropTable(
                name: "refresh_tokens",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "reports",
                schema: "forum");

            migrationBuilder.DropTable(
                name: "role_permissions",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "tags",
                schema: "forum");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "comments",
                schema: "forum");

            migrationBuilder.DropTable(
                name: "job_postings",
                schema: "career");

            migrationBuilder.DropTable(
                name: "messages",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "notification_templates",
                schema: "notification");

            migrationBuilder.DropTable(
                name: "posts",
                schema: "forum");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "users",
                schema: "identity");
        }
    }
}
