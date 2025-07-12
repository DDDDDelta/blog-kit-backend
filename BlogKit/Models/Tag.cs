namespace BlogKit.Models;

/// <summary>
/// Represents a tag for categorizing blog posts
/// </summary>
public class Tag
{
    /// <summary>
    /// Unique identifier for the tag
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Name of the tag
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Color associated with the tag (hex code)
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Date when the tag was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the tag was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Number of posts using this tag
    /// </summary>
    public int PostCount { get; set; } = 0;
} 