using BlogKit.Models;

namespace BlogKit.Data;

/// <summary>
/// Abstract interface for tag repository operations
/// </summary>
public interface ITagRepository
{
    /// <summary>
    /// Get a tag by its ID
    /// </summary>
    /// <param name="id">The tag ID</param>
    /// <returns>The tag or null if not found</returns>
    Task<Tag?> GetByIdAsync(string id);

    /// <summary>
    /// Get a tag by its name
    /// </summary>
    /// <param name="name">The tag name</param>
    /// <returns>The tag or null if not found</returns>
    Task<Tag?> GetByNameAsync(string name);

    /// <summary>
    /// Get tags with pagination
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="searchTerm">Search term for name or description</param>
    /// <param name="isActive">Filter by active status</param>
    /// <returns>Paginated list of tags</returns>
    Task<PaginatedResult<Tag>> GetTagsPaginatedAsync(
        int page = 1,
        int pageSize = 10,
        string? searchTerm = null,
        bool? isActive = null);

    /// <summary>
    /// Create a new tag
    /// </summary>
    /// <param name="tag">The tag to create</param>
    /// <returns>The created tag with ID</returns>
    Task<Tag> CreateTagAsync(Tag tag);

    /// <summary>
    /// Update an existing tag
    /// </summary>
    /// <param name="tag">The tag to update</param>
    /// <returns>The updated tag</returns>
    Task<Tag> UpdateTagAsync(Tag tag);

    /// <summary>
    /// Delete a tag
    /// </summary>
    /// <param name="id">The tag ID to delete</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteTagAsync(string id);

    /// <summary>
    /// Check if tag name exists
    /// </summary>
    /// <param name="name">The name to check</param>
    /// <param name="excludeId">ID to exclude from check (for updates)</param>
    /// <returns>True if name exists</returns>
    Task<bool> NameExistsAsync(string name, string? excludeId = null);

    /// <summary>
    /// Check if tag id exists
    /// </summary>
    /// <param name="id">The ID to check</param>
    /// <returns>True if ID exists</returns>
    Task<bool> IdExistsAsync(string id);

    /// <summary>
    /// Get tags with post count
    /// </summary>
    /// <returns>List of tags with post counts</returns>
    Task<List<Tag>> GetTagsWithPostCountAsync();

    /// <summary>
    /// Update post count for a tag
    /// </summary>
    /// <param name="tagId">The tag ID</param>
    /// <returns>True if updated successfully</returns>
    Task<bool> UpdatePostCountAsync(string tagId);

    /// <summary>
    /// Get tags by post ID
    /// </summary>
    /// <param name="postId">The post ID</param>
    /// <returns>List of tags for the post</returns>
    Task<List<Tag>> GetTagsByPostAsync(string postId);

    /// <summary>
    /// Add tags to a post
    /// </summary>
    /// <param name="postId">The post ID</param>
    /// <param name="tagIds">List of tag IDs</param>
    /// <returns>True if added successfully</returns>
    Task<bool> AddTagsToPostAsync(string postId, List<string> tagIds);

    /// <summary>
    /// Remove tags from a post
    /// </summary>
    /// <param name="postId">The post ID</param>
    /// <param name="tagIds">List of tag IDs</param>
    /// <returns>True if removed successfully</returns>
    Task<bool> RemoveTagsFromPostAsync(string postId, List<string> tagIds);
}