using BlogKit.Models;
using BlogKit.Data;

namespace BlogKit.Services;

/// <summary>
/// Service for managing blog tags with business logic and validation
/// </summary>
public class TagService
{
    private readonly ITagRepository _tagRepository;

    /// <summary>
    /// Initializes a new instance of the TagService
    /// </summary>
    /// <param name="tagRepository">The tag repository for data access</param>
    public TagService(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
    }

    /// <summary>
    /// Gets tags with pagination and optional filtering
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="searchTerm">Search term for name or description</param>
    /// <returns>Paginated list of tags</returns>
    public async Task<PaginatedResult<Tag>> GetTagsPaginatedAsync(
        int page = 1,
        int pageSize = 10,
        string? searchTerm = null)
    {
        return await _tagRepository.GetTagsPaginatedAsync(page, pageSize, searchTerm);
    }

    /// <summary>
    /// Gets a tag by its unique identifier
    /// </summary>
    /// <param name="id">The tag ID</param>
    /// <returns>The tag or null if not found</returns>
    public async Task<Tag?> GetTagByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        return await _tagRepository.GetByIdAsync(id);
    }

    /// <summary>
    /// Gets a tag by its name
    /// </summary>
    /// <param name="name">The tag name</param>
    /// <returns>The tag or null if not found</returns>
    public async Task<Tag?> GetTagByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return await _tagRepository.GetByNameAsync(name);
    }

    /// <summary>
    /// Creates a new tag
    /// </summary>
    /// <param name="tag">The tag to create</param>
    /// <returns>The created tag with generated ID</returns>
    public async Task<Tag> CreateTagAsync(Tag tag)
    {
        if (string.IsNullOrWhiteSpace(tag.Name))
            throw new ArgumentException("Tag name is required", nameof(tag));

        if (await _tagRepository.NameExistsAsync(tag.Name))
            throw new InvalidOperationException($"Tag with name '{tag.Name}' already exists");

        tag.CreatedAt = DateTime.UtcNow;
        tag.UpdatedAt = DateTime.UtcNow;

        return await _tagRepository.CreateTagAsync(tag);
    }

    /// <summary>
    /// Updates an existing tag
    /// </summary>
    /// <param name="tag">The tag to update</param>
    /// <returns>The updated tag</returns>
    public async Task<Tag> UpdateTagAsync(Tag tag)
    {
        if (string.IsNullOrWhiteSpace(tag.Id))
            throw new ArgumentException("Tag ID is required", nameof(tag));

        if (string.IsNullOrWhiteSpace(tag.Name))
            throw new ArgumentException("Tag name is required", nameof(tag));

        if (!await _tagRepository.IdExistsAsync(tag.Id))
            throw new InvalidOperationException($"Tag with ID '{tag.Id}' not found");

        if (await _tagRepository.NameExistsAsync(tag.Name, tag.Id))
            throw new InvalidOperationException($"Tag with name '{tag.Name}' already exists");

        tag.UpdatedAt = DateTime.UtcNow;

        return await _tagRepository.UpdateTagAsync(tag);
    }

    /// <summary>
    /// Deletes a tag by its ID
    /// </summary>
    /// <param name="id">The tag ID to delete</param>
    /// <returns>True if deleted successfully, false if tag not found</returns>
    public async Task<bool> DeleteTagAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        return await _tagRepository.DeleteTagAsync(id);
    }

    /// <summary>
    /// Checks if a tag exists by its ID
    /// </summary>
    /// <param name="id">The tag ID to check</param>
    /// <returns>True if tag exists, false otherwise</returns>
    public async Task<bool> TagExistsAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        return await _tagRepository.IdExistsAsync(id);
    }

    /// <summary>
    /// Checks if a tag name already exists
    /// </summary>
    /// <param name="name">The tag name to check</param>
    /// <param name="excludeId">ID to exclude from check (for updates)</param>
    /// <returns>True if name exists, false otherwise</returns>
    public async Task<bool> TagNameExistsAsync(string name, string? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        return await _tagRepository.NameExistsAsync(name, excludeId);
    }

    /// <summary>
    /// Gets tags with post count information
    /// </summary>
    /// <returns>List of tags with post counts</returns>
    public async Task<List<Tag>> GetTagsWithPostCountAsync()
    {
        return await _tagRepository.GetTagsWithPostCountAsync();
    }

    /// <summary>
    /// Gets tags associated with a specific post
    /// </summary>
    /// <param name="postId">The post ID</param>
    /// <returns>List of tags for the post</returns>
    public async Task<List<Tag>> GetTagsByPostAsync(string postId)
    {
        if (string.IsNullOrWhiteSpace(postId))
            return [];

        return await _tagRepository.GetTagsByPostAsync(postId);
    }

    /// <summary>
    /// Adds tags to a post
    /// </summary>
    /// <param name="postId">The post ID</param>
    /// <param name="tagIds">List of tag IDs to add</param>
    /// <returns>True if added successfully</returns>
    public async Task<bool> AddTagsToPostAsync(string postId, List<string> tagIds)
    {
        if (string.IsNullOrWhiteSpace(postId))
            return false;

        return await _tagRepository.AddTagsToPostAsync(postId, tagIds);
    }

    /// <summary>
    /// Removes tags from a post
    /// </summary>
    /// <param name="postId">The post ID</param>
    /// <param name="tagIds">List of tag IDs to remove</param>
    /// <returns>True if removed successfully</returns>
    public async Task<bool> RemoveTagsFromPostAsync(string postId, List<string> tagIds)
    {
        if (string.IsNullOrWhiteSpace(postId))
            return false;

        return await _tagRepository.RemoveTagsFromPostAsync(postId, tagIds);
    }
} 
