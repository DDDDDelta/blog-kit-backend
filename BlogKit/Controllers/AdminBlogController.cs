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
public class AdminBlogController(
    BlogService blogService, 
    ILogger<AdminBlogController> logger) : ControllerBase
{
    private readonly BlogService _blogService = blogService;
    private readonly ILogger<AdminBlogController> _logger = logger;

    /// <summary>
    /// Get a blog post by ID (admin only)
    /// </summary>
    /// <param name="id">The blog post ID</param>
    /// <param name="incrementViewCount">Whether to increment the view count</param>
    /// <returns>The blog post</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<BlogPost>> GetPost(string id, [FromQuery] bool incrementViewCount = false)
    {
        var post = await _blogService.GetPostByIdAsync(id, incrementViewCount);
        if (post == null)
            return NotFound();

        return Ok(post);
    }

    /// <summary>
    /// Create a new blog post (admin only)
    /// </summary>
    /// <param name="post">The blog post to create</param>
    /// <returns>The created blog post</returns>
    [HttpPost]
    public async Task<ActionResult<BlogPost>> CreatePost([FromBody] BlogPost post)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var createdPost = await _blogService.CreatePostAsync(post);
        return CreatedAtAction(nameof(GetPost), new { id = createdPost.Id }, createdPost);
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
        if (id != post.Id)
            return BadRequest("ID mismatch");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updatedPost = await _blogService.UpdatePostAsync(post);
        if (updatedPost == null)
            return NotFound();

        return Ok(updatedPost);
    }

    /// <summary>
    /// Delete a blog post (admin only)
    /// </summary>
    /// <param name="id">The blog post ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePost(string id)
    {
        var deleted = await _blogService.DeletePostAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
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
        var updated = await _blogService.SetFeaturedAsync(id, isFeatured);
        if (!updated)
            return NotFound();

        return Ok(new { message = $"Post {(isFeatured ? "featured" : "unfeatured")} successfully" });
    }
}
