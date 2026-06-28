using System.Text.Json;
using UniHub.Forum.Domain.Categories;
using UniHub.Forum.Domain.Categories.ValueObjects;
using UniHub.Forum.Domain.Comments;
using UniHub.Forum.Domain.Comments.ValueObjects;
using UniHub.Forum.Domain.Posts;
using UniHub.Forum.Domain.Posts.ValueObjects;
using UniHub.Forum.Domain.Reports;
using UniHub.Forum.Domain.Tags;
using UniHub.Forum.Domain.Tags.ValueObjects;

namespace UniHub.Forum.Infrastructure.Persistence.Configurations;

internal static class ForumModelConversion
{
    public static CategoryId ToCategoryId(Guid value)
    {
        return CategoryId.Create(value);
    }

    public static CategoryName ToCategoryName(string raw)
    {
        return CategoryName.Create(raw).Value;
    }

    public static CategoryDescription ToCategoryDescription(string raw)
    {
        return CategoryDescription.Create(raw).Value;
    }

    public static Slug ToSlug(string raw)
    {
        return Slug.Create(raw).Value;
    }

    public static PostId ToPostId(Guid value)
    {
        return PostId.Create(value);
    }

    public static CommentId ToCommentId(Guid value)
    {
        return CommentId.Create(value);
    }

    public static TagId ToTagId(int value)
    {
        return TagId.Create(value);
    }

    public static ReportId ToReportId(int value)
    {
        return new ReportId(value);
    }

    public static CommentContent ToCommentContent(string raw)
    {
        return CommentContent.Create(raw).Value;
    }

    public static TagName ToTagName(string raw)
    {
        return TagName.Create(raw).Value;
    }

    public static TagDescription ToTagDescription(string raw)
    {
        return TagDescription.Create(raw).Value;
    }

    public static string ToStringListDb(List<string> values)
    {
        return JsonSerializer.Serialize(values);
    }

    public static List<string> ToStringListDomain(string raw)
    {
        return JsonSerializer.Deserialize<List<string>>(raw) ?? new List<string>();
    }

    public static string ToGuidListDb(List<Guid> values)
    {
        return JsonSerializer.Serialize(values);
    }

    public static List<Guid> ToGuidListDomain(string raw)
    {
        return JsonSerializer.Deserialize<List<Guid>>(raw) ?? new List<Guid>();
    }
}
