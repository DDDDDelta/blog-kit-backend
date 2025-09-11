using Microsoft.AspNetCore.Mvc;
using BlogKit.Services;
using BlogKit.Models;
using Microsoft.Extensions.Logging;
using BlogKit.Data;

namespace BlogKit.Controllers;

/// <summary>
/// Controller for blog post operations
/// </summary>
[ApiController]
[Route("api/blog")]
public class BlogController : ControllerBase
{
    private readonly BlogService _blogService;
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<BlogController> _logger;

    public BlogController(BlogService blogService, ITagRepository tagRepository, ILogger<BlogController> logger)
    {
        _blogService = blogService;
        _tagRepository = tagRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all blog summaries with optional filtering
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="author">Filter by author</param>
    /// <param name="tag">Filter by tag</param>
    /// <param name="searchTerm">Search term for title and content</param>
    /// <param name="isFeatured">Filter by featured status</param>
    /// <param name="sortBy">Sort field</param>
    /// <param name="sortOrder">Sort order (asc/desc)</param>
    /// <returns>Paginated list of blog summaries</returns>
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<BlogSummary>>> GetPosts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? author = null,
        [FromQuery] string? tag = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool isFeatured = false,
        [FromQuery] string sortBy = "CreatedAt",
        [FromQuery] string sortOrder = "desc")
    {
        var posts = await _blogService.GetPostsAsync(
            page, pageSize, author, tag, searchTerm, isFeatured, sortBy, sortOrder);
        
        // Convert to summaries
        var summaries = new List<BlogSummary>();
        foreach (var p in posts.Items)
        {
            summaries.Add(new BlogSummary
            {
                Id = p.Id,
                Title = p.Title,
                Tags = p.Tags,
                Author = p.Author,
                IsFeatured = p.IsFeatured,
                PublishDate = p.CreatedAt
            });
        }

        var result = new PaginatedResult<BlogSummary>
        {
            Items = summaries,
            TotalCount = posts.TotalCount,
            Page = posts.Page,
            PageSize = posts.PageSize
        };

        return Ok(result);
    }

    /// <summary>
    /// Get a blog post by ID
    /// </summary>
    /// <param name="id">The blog post ID</param>
    /// <returns>The blog post</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<BlogPost>> GetPostById(string id)
    {
        var post = await _blogService.GetPostByIdAsync(id);
        if (post == null)
        {
            return NotFound();
        }

        return Ok(post);
    }

    /// <summary>
    /// Get featured blog summaries
    /// </summary>
    /// <param name="limit">Maximum number of posts to return</param>
    /// <returns>List of featured blog summaries</returns>
    [HttpGet("featured")]
    public async Task<ActionResult<List<BlogSummary>>> GetFeaturedPosts([FromQuery] int limit = 5)
    {
        var posts = await _blogService.GetFeaturedPostsAsync(limit);
        var summaries = new List<BlogSummary>();
        foreach (var p in posts)
        {
            summaries.Add(new BlogSummary
            {
                Id = p.Id,
                Title = p.Title,
                Tags = p.Tags,
                Author = p.Author,
                IsFeatured = p.IsFeatured,
                PublishDate = p.CreatedAt
            });
        }
        return Ok(summaries);
    }

    /// <summary>
    /// Get recent blog summaries
    /// </summary>
    /// <param name="limit">Maximum number of posts to return</param>
    /// <returns>List of recent blog summaries</returns>
    [HttpGet("recent")]
    public async Task<ActionResult<List<BlogSummary>>> GetRecentPosts([FromQuery] int limit = 5)
    {
        var posts = await _blogService.GetRecentPostsAsync(limit);
        var summaries = new List<BlogSummary>();
        foreach (var p in posts)
        {
            summaries.Add(new BlogSummary
            {
                Id = p.Id,
                Title = p.Title,
                Tags = p.Tags,
                Author = p.Author,
                IsFeatured = p.IsFeatured,
                PublishDate = p.CreatedAt
            });
        }
        return Ok(summaries);
    }
}