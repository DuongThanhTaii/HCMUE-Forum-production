namespace UniHub.Identity.Domain.Permissions;

/// <summary>
/// Defines permission codes following pattern: {module}.{resource}.{action}
/// </summary>
public static class PermissionCodes
{
    public static class Forum
    {
        public const string PostCreate = "forum.post.create";
        public const string PostEdit = "forum.post.edit";
        public const string PostDelete = "forum.post.delete";
        public const string PostModerate = "forum.post.moderate";
        public const string PostPin = "forum.post.pin";
        public const string PostClose = "forum.post.close";
        
        public const string CommentCreate = "forum.comment.create";
        public const string CommentEdit = "forum.comment.edit";
        public const string CommentDelete = "forum.comment.delete";
        public const string CommentModerate = "forum.comment.moderate";
        
        public const string CategoryCreate = "forum.category.create";
        public const string CategoryEdit = "forum.category.edit";
        public const string CategoryDelete = "forum.category.delete";
        public const string CategoryManage = "forum.category.manage";
    }

    public static class Learning
    {
        public const string DocumentUpload = "learning.document.upload";
        public const string DocumentEdit = "learning.document.edit";
        public const string DocumentDelete = "learning.document.delete";
        public const string DocumentApprove = "learning.document.approve";
        public const string DocumentReject = "learning.document.reject";
        
        public const string CourseCreate = "learning.course.create";
        public const string CourseEdit = "learning.course.edit";
        public const string CourseDelete = "learning.course.delete";
        public const string CourseManage = "learning.course.manage";
        public const string CourseEnroll = "learning.course.enroll";
        
        public const string MaterialUpload = "learning.material.upload";
        public const string MaterialEdit = "learning.material.edit";
        public const string MaterialDelete = "learning.material.delete";
        public const string MaterialManage = "learning.material.manage";
    }

    public static class Identity
    {
        public const string UserCreate = "identity.user.create";
        public const string UserEdit = "identity.user.edit";
        public const string UserDelete = "identity.user.delete";
        public const string UserManage = "identity.user.manage";
        public const string UserSuspend = "identity.user.suspend";
        public const string UserBan = "identity.user.ban";
        
        public const string RoleCreate = "identity.role.create";
        public const string RoleEdit = "identity.role.edit";
        public const string RoleDelete = "identity.role.delete";
        public const string RoleManage = "identity.role.manage";
        
        public const string PermissionView = "identity.permission.view";
        public const string PermissionAssign = "identity.permission.assign";
        public const string PermissionRevoke = "identity.permission.revoke";
        
        public const string BadgeAssign = "identity.badge.assign";
        public const string BadgeRevoke = "identity.badge.revoke";
        public const string BadgeManage = "identity.badge.manage";
    }

    public static class Career
    {
        public const string JobCreate = "career.job.create";
        public const string JobEdit = "career.job.edit";
        public const string JobDelete = "career.job.delete";
        public const string JobManage = "career.job.manage";
        public const string JobApprove = "career.job.approve";
        
        public const string ApplicationView = "career.application.view";
        public const string ApplicationManage = "career.application.manage";
        
        public const string CompanyCreate = "career.company.create";
        public const string CompanyEdit = "career.company.edit";
        public const string CompanyVerify = "career.company.verify";
    }

    public static class Chat
    {
        public const string MessageSend = "chat.message.send";
        public const string MessageEdit = "chat.message.edit";
        public const string MessageDelete = "chat.message.delete";
        public const string MessageModerate = "chat.message.moderate";
        
        public const string ChannelCreate = "chat.channel.create";
        public const string ChannelEdit = "chat.channel.edit";
        public const string ChannelDelete = "chat.channel.delete";
        public const string ChannelManage = "chat.channel.manage";
        
        public const string GroupCreate = "chat.group.create";
        public const string GroupManage = "chat.group.manage";
        public const string GroupModerate = "chat.group.moderate";
    }

    public static class Notification
    {
        public const string NotificationSend = "notification.send";
        public const string NotificationManage = "notification.manage";
        public const string NotificationBroadcast = "notification.broadcast";
        
        public const string TemplateCreate = "notification.template.create";
        public const string TemplateEdit = "notification.template.edit";
        public const string TemplateDelete = "notification.template.delete";
    }

    public static class AI
    {
        public const string ChatAccess = "ai.chat.access";
        public const string ChatManage = "ai.chat.manage";
        
        public const string RecommendationView = "ai.recommendation.view";
        public const string RecommendationManage = "ai.recommendation.manage";
        
        public const string ModelManage = "ai.model.manage";
        public const string ModelTrain = "ai.model.train";
    }

    /// <summary>
    /// Gets all permission codes defined in the system
    /// </summary>
    public static IEnumerable<string> GetAllPermissions()
    {
        var permissionClasses = new[]
        {
            typeof(Forum),
            typeof(Learning),
            typeof(Identity),
            typeof(Career),
            typeof(Chat),
            typeof(Notification),
            typeof(AI)
        };

        foreach (var permissionClass in permissionClasses)
        {
            var fields = permissionClass.GetFields();
            foreach (var field in fields)
            {
                if (field.IsLiteral && field.FieldType == typeof(string))
                {
                    var value = field.GetValue(null) as string;
                    if (!string.IsNullOrEmpty(value))
                        yield return value;
                }
            }
        }
    }

    /// <summary>
    /// Gets permission codes for a specific module
    /// </summary>
    public static IEnumerable<string> GetModulePermissions(string module)
    {
        return GetAllPermissions().Where(p => p.StartsWith($"{module.ToLowerInvariant()}."));
    }
}