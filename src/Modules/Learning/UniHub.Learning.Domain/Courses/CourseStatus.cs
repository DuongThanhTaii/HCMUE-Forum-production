namespace UniHub.Learning.Domain.Courses;

/// <summary>
/// Trạng thái của course
/// </summary>
public enum CourseStatus
{
    /// <summary>
    /// Course đang active (có thể enroll, upload document)
    /// </summary>
    Active = 1,
    
    /// <summary>
    /// Course đã kết thúc nhưng vẫn xem được
    /// </summary>
    Completed = 2,
    
    /// <summary>
    /// Course đã bị archive (chỉ moderator xem được)
    /// </summary>
    Archived = 3,
    
    /// <summary>
    /// Course đã bị xóa
    /// </summary>
    Deleted = 4
}
