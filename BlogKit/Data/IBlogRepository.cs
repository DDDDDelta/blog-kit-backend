using BlogKit.Models;

namespace BlogKit.Data;

/// <summary>
/// Abstract interface for blog post repository operations
/// </summary>
public interface IBlogRepository
{
    /// <summary>
    /// Get a blog post by its ID
    /// </summary>
    /// <param name="id">The blog post ID</param>
    /// <returns>The blog post or null if not found</returns>
    Task<BlogPost?> GetByIdAsync(string id);

    /// <summary>
    /// Get all blog posts with optional filtering
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="author">Filter by author</param>
    /// <param name="tag">Filter by tag</param>
    /// <param name="searchTerm">Search term for title and content</param>
    /// <param name="isFeatured">Filter by featured status</param>
    /// <param name="sortBy">Sort field</param>
    /// <param name="sortOrder">Sort order (asc/desc)</param>
    /// <returns>Paginated list of blog posts</returns>
    Task<PaginatedResult<BlogPost>> GetPostsAsync(
        int page = 1,
        int pageSize = 10,
        string? author = null,
        string? tag = null,
        string? searchTerm = null,
        bool isFeatured = false,
        string? sortBy = "CreatedAt",
        string? sortOrder = "desc");

    /// <summary>
    /// Get featured blog posts
    /// </summary>
    /// <param name="limit">Maximum number of posts to return</param>
    /// <returns>List of featured blog posts</returns>
    Task<List<BlogPost>> GetFeaturedPostsAsync(int limit = 5);

    /// <summary>
    /// Get recent blog posts
    /// </summary>
    /// <param name="limit">Maximum number of posts to return</param>
    /// <returns>List of recent blog posts</returns>
    Task<List<BlogPost>> GetRecentPostsAsync(int limit = 5);

    /// <summary>
    /// Get blog posts by author
    /// </summary>
    /// <param name="author">Author name</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paginated list of blog posts by author</returns>
    Task<PaginatedResult<BlogPost>> GetPostsByAuthorAsync(string author, int page = 1, int pageSize = 10);

    /// <summary>
    /// Search blog posts
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paginated list of search results</returns>
    Task<PaginatedResult<BlogPost>> SearchPostsAsync(string searchTerm, int page = 1, int pageSize = 10);

    /// <summary>
    /// Create a new blog post
    /// </summary>
    /// <param name="post">The blog post to create</param>
    /// <returns>The created blog post with ID</returns>
    Task<BlogPost> CreatePostAsync(BlogPost post);

    /// <summary>
    /// Update an existing blog post
    /// </summary>
    /// <param name="post">The blog post to update</param>
    /// <returns>The updated blog post</returns>
    Task<BlogPost> UpdatePostAsync(BlogPost post);

    /// <summary>
    /// Delete a blog post
    /// </summary>
    /// <param name="id">The blog post ID to delete</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeletePostAsync(string id);

    /// <summary>
    /// Increment the view count for a blog post
    /// </summary>
    /// <param name="id">The blog post ID</param>
    /// <returns>True if updated successfully</returns>
    Task<bool> IncrementViewCountAsync(string id);
}