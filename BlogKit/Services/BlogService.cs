using BlogKit.Data;
using BlogKit.Models;
using System.Text.RegularExpressions;

namespace BlogKit.Services;

/// <summary>
/// Concrete service for blog operations
/// </summary>
public class BlogService
{
    private readonly IBlogRepository _blogRepository;

    public BlogService(IBlogRepository blogRepository)
    {
        _blogRepository = blogRepository;
    }

    /// <summary>
    /// Get a blog post by its ID
    /// </summary>
    /// <param name="id">The blog post ID</param>
    /// <param name="incrementViewCount">Whether to increment the view count</param>
    /// <returns>The blog post or null if not found</returns>
    public async Task<BlogPost?> GetPostByIdAsync(string id, bool incrementViewCount = false)
    {
        var post = await _blogRepository.GetByIdAsync(id);
        
        if (post != null && incrementViewCount)
        {
            await _blogRepository.IncrementViewCountAsync(id);
        }
        
        return post;
    }

    /// <summary>
    /// Get a blog post by its slug
    /// </summary>
    /// <param name="slug">The blog post slug</param>
    /// <param name="incrementViewCount">Whether to increment the view count</param>
    /// <returns>The blog post or null if not found</returns>
    public async Task<BlogPost?> GetPostBySlugAsync(string slug, bool incrementViewCount = false)
    {
        var post = await _blogRepository.GetBySlugAsync(slug);
        
        if (post != null && incrementViewCount)
        {
            await _blogRepository.IncrementViewCountAsync(post.Id);
        }
        
        return post;
    }

    /// <summary>
    /// Get all blog posts with optional filtering
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="author">Filter by author</param>
    /// <param name="tag">Filter by tag</param>
    /// <param name="searchTerm">Search term for title and content</param>
    /// <param name="sortBy">Sort field</param>
    /// <param name="sortOrder">Sort order (asc/desc)</param>
    /// <returns>Paginated list of blog posts</returns>
    public async Task<PaginatedResult<BlogPost>> GetPostsAsync(
        int page = 1,
        int pageSize = 10,
        string? author = null,
        string? tag = null,
        string? searchTerm = null,
        bool isFeatured = false,
        string? sortBy = "CreatedAt",
        string? sortOrder = "desc")
    {
        return await _blogRepository.GetPostsAsync(
            page, pageSize, author, tag, searchTerm, isFeatured, sortBy, sortOrder);
    }

    /// <summary>
    /// Get featured blog posts
    /// </summary>
    /// <param name="limit">Maximum number of posts to return</param>
    /// <returns>List of featured blog posts</returns>
    public async Task<List<BlogPost>> GetFeaturedPostsAsync(int limit = 5)
    {
        return await _blogRepository.GetFeaturedPostsAsync(limit);
    }

    /// <summary>
    /// Get recent blog posts
    /// </summary>
    /// <param name="limit">Maximum number of posts to return</param>
    /// <returns>List of recent blog posts</returns>
    public async Task<List<BlogPost>> GetRecentPostsAsync(int limit = 5)
    {
        return await _blogRepository.GetRecentPostsAsync(limit);
    }

    /// <summary>
    /// Get blog posts by author
    /// </summary>
    /// <param name="author">Author name</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paginated list of blog posts by author</returns>
    public async Task<PaginatedResult<BlogPost>> GetPostsByAuthorAsync(string author, int page = 1, int pageSize = 10)
    {
        return await _blogRepository.GetPostsByAuthorAsync(author, page, pageSize);
    }

    /// <summary>
    /// Get blog posts by tag
    /// </summary>
    /// <param name="tagSlug">Tag slug</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paginated list of blog posts by tag</returns>
    public async Task<PaginatedResult<BlogPost>> GetPostsByTagAsync(string tagId, int page = 1, int pageSize = 10)
    {
        return await _blogRepository.GetPostsByTagAsync(tagId, page, pageSize);
    }

    /// <summary>
    /// Search blog posts
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paginated list of search results</returns>
    public async Task<PaginatedResult<BlogPost>> SearchPostsAsync(string searchTerm, int page = 1, int pageSize = 10)
    {
        return await _blogRepository.SearchPostsAsync(searchTerm, page, pageSize);
    }

    /// <summary>
    /// Create a new blog post
    /// </summary>
    /// <param name="post">The blog post to create</param>
    /// <returns>The created blog post with ID</returns>
    public async Task<BlogPost> CreatePostAsync(BlogPost post)
    {
        post.CreatedAt = DateTime.UtcNow;
        post.UpdatedAt = DateTime.UtcNow;

        return await _blogRepository.CreatePostAsync(post);
    }

    /// <summary>
    /// Update an existing blog post
    /// </summary>
    /// <param name="post">The blog post to update</param>
    /// <returns>The updated blog post</returns>
    public async Task<BlogPost> UpdatePostAsync(BlogPost post)
    {
        if (string.IsNullOrWhiteSpace(post.Id))
            throw new ArgumentException("Post ID is required", nameof(post));

        if (string.IsNullOrWhiteSpace(post.Title))
            throw new ArgumentException("Post title is required", nameof(post));

        post.UpdatedAt = DateTime.UtcNow;

        return await _blogRepository.UpdatePostAsync(post);
    }

    /// <summary>
    /// Delete a blog post
    /// </summary>
    /// <param name="id">The blog post ID to delete</param>
    /// <returns>True if deleted successfully</returns>
    public async Task<bool> DeletePostAsync(string id)
    {
        return await _blogRepository.DeletePostAsync(id);
    }

    /// <summary>
    /// Feature or unfeature a blog post
    /// </summary>
    /// <param name="id">The blog post ID</param>
    /// <param name="isFeatured">Whether to feature the post</param>
    /// <returns>True if updated successfully</returns>
    public async Task<bool> SetFeaturedAsync(string id, bool isFeatured)
    {
        var post = await _blogRepository.GetByIdAsync(id);
        if (post == null)
            return false;

        post.IsFeatured = isFeatured;
        post.UpdatedAt = DateTime.UtcNow;

        await _blogRepository.UpdatePostAsync(post);
        return true;
    }
} 
