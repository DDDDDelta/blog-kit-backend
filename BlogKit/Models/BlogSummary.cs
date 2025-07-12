namespace BlogKit.Models;

/// <summary>
/// Lightweight blog post summary for listing pages
/// </summary>
public class BlogSummary
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
    /// Tags associated with the blog post
    /// </summary>
    public List<Tag> Tags { get; set; } = new();

    /// <summary>
    /// Author of the blog post
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Whether the blog post is featured
    /// </summary>
    public bool IsFeatured { get; set; } = false;

    /// <summary>
    /// Date when the blog post was published
    /// </summary>
    public DateTime PublishDate { get; set; } = DateTime.UtcNow;
} 
