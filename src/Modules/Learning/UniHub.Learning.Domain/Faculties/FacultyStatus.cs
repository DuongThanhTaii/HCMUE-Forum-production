namespace UniHub.Learning.Domain.Faculties;

/// <summary>
/// Trạng thái của Faculty
/// </summary>
public enum FacultyStatus
{
    /// <summary>
    /// Faculty đang hoạt động
    /// </summary>
    Active,
    
    /// <summary>
    /// Faculty bị vô hiệu hóa (không còn hoạt động)
    /// </summary>
    Inactive,
    
    /// <summary>
    /// Faculty bị xóa (soft delete)
    /// </summary>
    Deleted
}
