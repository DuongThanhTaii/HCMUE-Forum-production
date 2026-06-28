using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Domain.Conversations;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Domain.Categories;
using UniHub.Forum.Domain.Comments;
using UniHub.Forum.Domain.Posts;
using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Roles;
using UniHub.Learning.Application.Abstractions;
using UniHub.Learning.Domain.Courses;
using UniHub.Learning.Domain.Documents;
using UniHub.Notification.Application.Abstractions;

namespace UniHub.API.Services;

/// <summary>
/// Resolves notification recipients across Forum, Learning, Chat, and Identity modules.
/// </summary>
public sealed class NotificationRecipientResolver : INotificationRecipientResolver
{
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public NotificationRecipientResolver(
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        ICategoryRepository categoryRepository,
        IDocumentRepository documentRepository,
        ICourseRepository courseRepository,
        IConversationRepository conversationRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository)
    {
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _categoryRepository = categoryRepository;
        _documentRepository = documentRepository;
        _courseRepository = courseRepository;
        _conversationRepository = conversationRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task<(Guid AuthorId, string Title)?> GetPostAuthorAsync(
        Guid postId,
        CancellationToken cancellationToken = default)
    {
        var ctx = await GetPostContextAsync(postId, cancellationToken);
        return ctx is null ? null : (ctx.Value.AuthorId, ctx.Value.Title);
    }

    public async Task<(Guid AuthorId, Guid? CategoryId, string Title)?> GetPostContextAsync(
        Guid postId,
        CancellationToken cancellationToken = default)
    {
        var post = await _postRepository.GetByIdAsync(PostId.Create(postId), cancellationToken);
        if (post is null)
        {
            return null;
        }

        return (post.AuthorId, post.CategoryId, post.Title.Value);
    }

    public async Task<(Guid AuthorId, Guid PostId, string PostTitle)?> GetCommentContextAsync(
        Guid commentId,
        CancellationToken cancellationToken = default)
    {
        var comment = await _commentRepository.GetByIdAsync(CommentId.Create(commentId), cancellationToken);
        if (comment is null)
        {
            return null;
        }

        var post = await _postRepository.GetByIdAsync(comment.PostId, cancellationToken);
        var title = post?.Title.Value ?? "Bài viết";
        return (comment.AuthorId, comment.PostId.Value, title);
    }

    public async Task<IReadOnlyList<Guid>> GetForumModeratorUserIdsAsync(
        Guid? categoryId,
        CancellationToken cancellationToken = default)
    {
        var ids = new HashSet<Guid>();

        if (categoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(CategoryId.Create(categoryId.Value), cancellationToken);
            if (category is not null)
            {
                foreach (var modId in category.ModeratorIds)
                {
                    ids.Add(modId);
                }
            }
        }

        foreach (var globalMod in await GetAdminAndModeratorUserIdsAsync(cancellationToken))
        {
            ids.Add(globalMod);
        }

        return ids.ToList();
    }

    public async Task<(Guid UploaderId, string Title, Guid? CourseId)?> GetDocumentContextAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(DocumentId.Create(documentId), cancellationToken);
        if (document is null)
        {
            return null;
        }

        return (document.UploaderId, document.Title.Value, document.CourseId);
    }

    public async Task<IReadOnlyList<Guid>> GetLearningModeratorUserIdsAsync(
        Guid? courseId,
        CancellationToken cancellationToken = default)
    {
        var ids = new HashSet<Guid>();

        if (courseId.HasValue)
        {
            var course = await _courseRepository.GetByIdAsync(CourseId.Create(courseId.Value), cancellationToken);
            if (course is not null)
            {
                foreach (var modId in course.ModeratorIds)
                {
                    ids.Add(modId);
                }
            }
        }

        foreach (var globalMod in await GetAdminAndModeratorUserIdsAsync(cancellationToken))
        {
            ids.Add(globalMod);
        }

        return ids.ToList();
    }

    public async Task<IReadOnlyList<Guid>> GetConversationParticipantIdsExceptAsync(
        Guid conversationId,
        Guid exceptUserId,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(
            ConversationId.Create(conversationId),
            cancellationToken);

        if (conversation is null)
        {
            return Array.Empty<Guid>();
        }

        return conversation.Participants
            .Where(id => id != exceptUserId)
            .ToList();
    }

    public async Task<IReadOnlyList<Guid>> GetAdminAndModeratorUserIdsAsync(
        CancellationToken cancellationToken = default)
    {
        var adminRole = await _roleRepository.GetByNameAsync("Admin", cancellationToken);
        var modRole = await _roleRepository.GetByNameAsync("Moderator", cancellationToken);

        var roleIds = new HashSet<Guid>();
        if (adminRole is not null)
        {
            roleIds.Add(adminRole.Id.Value);
        }

        if (modRole is not null)
        {
            roleIds.Add(modRole.Id.Value);
        }

        if (roleIds.Count == 0)
        {
            return Array.Empty<Guid>();
        }

        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users
            .Where(u => u.Roles.Any(r => roleIds.Contains(r.RoleId.Value)))
            .Select(u => u.Id.Value)
            .ToList();
    }
}
