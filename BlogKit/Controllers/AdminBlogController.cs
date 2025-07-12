using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BlogKit.Services;
using BlogKit.Models;
using Microsoft.Extensions.Logging;
using BlogKit.Data;

namespace BlogKit.Controllers;

/// <summary>
/// Admin controller for blog post operations (requires authentication)
/// </summary>
[ApiController]
[Route("api/admin/blog")]
[Authorize(Roles = "Admin")]
public class AdminBlogController : ControllerBase
{
    private readonly BlogService _blogService;
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<AdminBlogController> _logger;

    public AdminBlogController(BlogService blogService, ITagRepository tagRepository, ILogger<AdminBlogController> logger)
    {
        _blogService = blogService;
        _tagRepository = tagRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get a blog post by ID (admin only)
    /// </summary>
    /// <param name="id">The blog post ID</param>
    /// <param name="incrementViewCount">Whether to increment the view count</param>
    /// <returns>The blog post</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<BlogPost>> GetPost(string id, [FromQuery] bool incrementViewCount = false)
    {
        try
        {
            var post = await _blogService.GetPostByIdAsync(id, incrementViewCount);
            if (post == null)
                return NotFound();

            return Ok(post);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blog post with ID: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new blog post (admin only)
    /// </summary>
    /// <param name="post">The blog post to create</param>
    /// <returns>The created blog post</returns>
    [HttpPost]
    public async Task<ActionResult<BlogPost>> CreatePost([FromBody] BlogPost post)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdPost = await _blogService.CreatePostAsync(post);
            return CreatedAtAction(nameof(GetPost), new { id = createdPost.Id }, createdPost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating blog post");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update an existing blog post (admin only)
    /// </summary>
    /// <param name="id">The blog post ID</param>
    /// <param name="post">The updated blog post</param>
    /// <returns>The updated blog post</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<BlogPost>> UpdatePost(string id, [FromBody] BlogPost post)
    {
        try
        {
            if (id != post.Id)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedPost = await _blogService.UpdatePostAsync(post);
            if (updatedPost == null)
                return NotFound();

            return Ok(updatedPost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating blog post with ID: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a blog post (admin only)
    /// </summary>
    /// <param name="id">The blog post ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePost(string id)
    {
        try
        {
            var deleted = await _blogService.DeletePostAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting blog post with ID: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Feature or unfeature a blog post (admin only)
    /// </summary>
    /// <param name="id">The blog post ID</param>
    /// <param name="isFeatured">Whether to feature the post</param>
    /// <returns>Success response</returns>
    [HttpPost("{id}/feature")]
    public async Task<ActionResult> SetFeatured(string id, [FromBody] bool isFeatured)
    {
        try
        {
            var updated = await _blogService.SetFeaturedAsync(id, isFeatured);
            if (!updated)
                return NotFound();

            return Ok(new { message = $"Post {(isFeatured ? "featured" : "unfeatured")} successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting featured status for post with ID: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Generate a unique slug for a blog post (admin only)
    /// </summary>
    /// <param name="title">The post title</param>
    /// <param name="excludeId">ID to exclude from check (for updates)</param>
    /// <returns>A unique slug</returns>
    [HttpGet("generate-slug")]
    public async Task<ActionResult<string>> GenerateSlug([FromQuery] string title, [FromQuery] string? excludeId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(title))
                return BadRequest("Title is required");

            var slug = await _blogService.GenerateSlugAsync(title, excludeId);
            return Ok(slug);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating slug for title: {Title}", title);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Calculate reading time for content (admin only)
    /// </summary>
    /// <param name="content">The content to analyze</param>
    /// <returns>Reading time in minutes</returns>
    [HttpPost("reading-time")]
    public ActionResult<int> CalculateReadingTime([FromBody] string content)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(content))
                return BadRequest("Content is required");

            var readingTime = _blogService.CalculateReadingTime(content);
            return Ok(readingTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating reading time");
            return StatusCode(500, "Internal server error");
        }
    }
} 