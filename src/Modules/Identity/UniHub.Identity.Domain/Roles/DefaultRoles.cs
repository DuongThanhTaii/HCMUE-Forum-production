using UniHub.Identity.Domain.Permissions;
using UniHub.Identity.Domain.Roles;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Domain.Roles;

public static class DefaultRoles
{
    public static class Names
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string Moderator = "Moderator";
        public const string Teacher = "Teacher";
        public const string Student = "Student";
        public const string Guest = "Guest";
    }

    public static class Descriptions
    {
        public const string SuperAdmin = "System super administrator with full access";
        public const string Admin = "System administrator with management privileges";
        public const string Moderator = "Content moderator with moderation privileges";
        public const string Teacher = "Teacher role with course and content management";
        public const string Student = "Default student role with basic access";
        public const string Guest = "Guest role with read-only access";
    }

    /// <summary>
    /// Creates all default system roles
    /// </summary>
    public static IEnumerable<Result<Role>> CreateDefaultRoles()
    {
        yield return CreateSuperAdminRole();
        yield return CreateAdminRole();
        yield return CreateModeratorRole();
        yield return CreateTeacherRole();
        yield return CreateStudentRole();
        yield return CreateGuestRole();
    }

    public static Result<Role> CreateSuperAdminRole()
    {
        return Role.CreateSystem(Names.SuperAdmin, Descriptions.SuperAdmin);
    }

    public static Result<Role> CreateAdminRole()
    {
        return Role.CreateSystem(Names.Admin, Descriptions.Admin);
    }

    public static Result<Role> CreateModeratorRole()
    {
        return Role.CreateSystem(Names.Moderator, Descriptions.Moderator);
    }

    public static Result<Role> CreateTeacherRole()
    {
        return Role.CreateSystem(Names.Teacher, Descriptions.Teacher);
    }

    public static Result<Role> CreateStudentRole()
    {
        var roleResult = Role.CreateSystem(Names.Student, Descriptions.Student);
        if (roleResult.IsSuccess)
        {
            var setDefaultResult = roleResult.Value.SetAsDefault(); // Student is default role
            if (setDefaultResult.IsFailure)
                return Result.Failure<Role>(setDefaultResult.Error);
        }
        return roleResult;
    }

    public static Result<Role> CreateGuestRole()
    {
        return Role.CreateSystem(Names.Guest, Descriptions.Guest);
    }

    /// <summary>
    /// Gets permissions for Super Admin (all permissions)
    /// </summary>
    public static IEnumerable<string> GetSuperAdminPermissions()
    {
        return PermissionCodes.GetAllPermissions();
    }

    /// <summary>
    /// Gets permissions for Admin role
    /// </summary>
    public static IEnumerable<string> GetAdminPermissions()
    {
        return new[]
        {
            // Identity Management
            PermissionCodes.Identity.UserManage,
            PermissionCodes.Identity.UserSuspend,
            PermissionCodes.Identity.RoleManage,
            PermissionCodes.Identity.PermissionAssign,
            PermissionCodes.Identity.BadgeManage,
            
            // Forum Management
            PermissionCodes.Forum.PostModerate,
            PermissionCodes.Forum.CommentModerate,
            PermissionCodes.Forum.CategoryManage,
            
            // Learning Management
            PermissionCodes.Learning.DocumentApprove,
            PermissionCodes.Learning.CourseManage,
            PermissionCodes.Learning.MaterialManage,
            
            // Career Management
            PermissionCodes.Career.JobManage,
            PermissionCodes.Career.ApplicationManage,
            PermissionCodes.Career.CompanyVerify,
            
            // Notification Management
            PermissionCodes.Notification.NotificationManage,
            PermissionCodes.Notification.NotificationBroadcast
        };
    }

    /// <summary>
    /// Gets permissions for Moderator role
    /// </summary>
    public static IEnumerable<string> GetModeratorPermissions()
    {
        return new[]
        {
            // Forum Moderation
            PermissionCodes.Forum.PostModerate,
            PermissionCodes.Forum.CommentModerate,
            PermissionCodes.Forum.PostPin,
            PermissionCodes.Forum.PostClose,
            
            // Chat Moderation
            PermissionCodes.Chat.MessageModerate,
            PermissionCodes.Chat.ChannelManage,
            PermissionCodes.Chat.GroupModerate,
            
            // Learning Moderation
            PermissionCodes.Learning.DocumentApprove,
            PermissionCodes.Learning.DocumentReject
        };
    }

    /// <summary>
    /// Gets permissions for Teacher role
    /// </summary>
    public static IEnumerable<string> GetTeacherPermissions()
    {
        return new[]
        {
            // Forum
            PermissionCodes.Forum.PostCreate,
            PermissionCodes.Forum.PostEdit,
            PermissionCodes.Forum.CommentCreate,
            PermissionCodes.Forum.CommentEdit,
            
            // Learning
            PermissionCodes.Learning.DocumentUpload,
            PermissionCodes.Learning.DocumentEdit,
            PermissionCodes.Learning.CourseCreate,
            PermissionCodes.Learning.CourseEdit,
            PermissionCodes.Learning.MaterialUpload,
            PermissionCodes.Learning.MaterialEdit,
            PermissionCodes.Learning.CourseEnroll,
            
            // Chat
            PermissionCodes.Chat.MessageSend,
            PermissionCodes.Chat.ChannelCreate,
            PermissionCodes.Chat.GroupCreate,
            
            // Career
            PermissionCodes.Career.JobCreate,
            PermissionCodes.Career.JobEdit
        };
    }

    /// <summary>
    /// Gets permissions for Student role
    /// </summary>
    public static IEnumerable<string> GetStudentPermissions()
    {
        return new[]
        {
            // Forum
            PermissionCodes.Forum.PostCreate,
            PermissionCodes.Forum.PostEdit, // Own posts only
            PermissionCodes.Forum.CommentCreate,
            PermissionCodes.Forum.CommentEdit, // Own comments only
            
            // Learning
            PermissionCodes.Learning.CourseEnroll,
            PermissionCodes.Learning.DocumentUpload, // Limited
            
            // Chat
            PermissionCodes.Chat.MessageSend,
            PermissionCodes.Chat.MessageEdit, // Own messages only
            PermissionCodes.Chat.GroupCreate,
            
            // Career
            PermissionCodes.Career.ApplicationView,
            
            // AI
            PermissionCodes.AI.ChatAccess,
            PermissionCodes.AI.RecommendationView
        };
    }

    /// <summary>
    /// Gets permissions for Guest role
    /// </summary>
    public static IEnumerable<string> GetGuestPermissions()
    {
        return new[]
        {
            // Very limited read-only access
            PermissionCodes.AI.RecommendationView
        };
    }
}