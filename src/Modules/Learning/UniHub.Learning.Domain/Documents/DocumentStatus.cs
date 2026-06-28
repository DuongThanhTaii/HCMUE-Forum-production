namespace UniHub.Learning.Domain.Documents;

/// <summary>
/// Trạng thái phê duyệt của tài liệu
/// </summary>
public enum DocumentStatus
{
    /// <summary>
    /// Chưa gửi duyệt - Draft
    /// </summary>
    Draft = 1,
    
    /// <summary>
    /// Đang chờ phê duyệt (Pending Approval)
    /// </summary>
    PendingApproval = 2,
    
    /// <summary>
    /// Đã được duyệt và xuất bản (Approved)
    /// </summary>
    Approved = 3,
    
    /// <summary>
    /// Bị từ chối (Rejected)
    /// </summary>
    Rejected = 4,
    
    /// <summary>
    /// Đã bị xóa
    /// </summary>
    Deleted = 5
}
