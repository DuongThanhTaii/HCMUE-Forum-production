namespace UniHub.Learning.Domain.Documents;

/// <summary>
/// Loại tài liệu học tập
/// </summary>
public enum DocumentType
{
    /// <summary>
    /// Slide bài giảng
    /// </summary>
    Slide = 1,
    
    /// <summary>
    /// Đề thi, đề cương ôn tập
    /// </summary>
    Exam = 2,
    
    /// <summary>
    /// Tóm tắt bài học
    /// </summary>
    Summary = 3,
    
    /// <summary>
    /// Source code mẫu
    /// </summary>
    SourceCode = 4,
    
    /// <summary>
    /// Video bài giảng
    /// </summary>
    Video = 5,
    
    /// <summary>
    /// Loại tài liệu khác
    /// </summary>
    Other = 6
}
