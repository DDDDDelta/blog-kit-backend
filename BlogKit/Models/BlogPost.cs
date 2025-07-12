namespace BlogKit.Models;

/// <summary>
/// Represents a blog post in the system
/// </summary>
public class BlogPost
{
    /// <summary>
    /// Unique identifier for the blog post
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Title of the blog post
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Content of the blog post (can be markdown, HTML, or plain text)
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Brief description or excerpt of the blog post
    /// </summary>
    public string? Excerpt { get; set; }

    /// <summary>
    /// Author of the blog post
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Tags associated with the blog post
    /// </summary>
    public List<Tag> Tags { get; set; } = [];

    /// <summary>
    /// Whether the blog post is featured
    /// </summary>
    public bool IsFeatured { get; set; } = false;

    /// <summary>
    /// Date when the blog post was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the blog post was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// View count
    /// </summary>
    public int ViewCount { get; set; } = 0;
} 