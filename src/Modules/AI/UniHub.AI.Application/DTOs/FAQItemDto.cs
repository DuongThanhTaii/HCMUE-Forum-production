namespace UniHub.AI.Application.DTOs;

/// <summary>
/// FAQ item data transfer object
/// </summary>
public class FAQItemDto
{
    public Guid Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public int Priority { get; set; }
    public int UsageCount { get; set; }
    public double? AverageRating { get; set; }
}
