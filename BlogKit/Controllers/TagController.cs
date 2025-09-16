using Microsoft.AspNetCore.Mvc;
using BlogKit.Services;
using BlogKit.Models;
using BlogKit.Data;

namespace BlogKit.Controllers;

/// <summary>
/// Controller for managing tags
/// </summary>
[ApiController]
[Route("api/tag")]
public class TagController(ITagRepository tagRepository) : ControllerBase
{
    private readonly ITagRepository _tagRepository = tagRepository;

    /// <summary>
    /// Get all tags
    /// </summary>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="includePostCount">Include post count in response</param>
    /// <returns>List of tags</returns>
    [HttpGet]
    public async Task<ActionResult<List<Tag>>> GetTags(
        [FromQuery] bool isActive = true, [FromQuery] bool includePostCount = true)
    {
        var tags = await _tagRepository.GetTagsWithPostCountAsync();
        return Ok(tags);
    }

    /// <summary>
    /// Get tags with pagination
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="searchTerm">Search term</param>
    /// <param name="isActive">Filter by active status</param>
    /// <returns>Paginated list of tags</returns>
    [HttpGet("paginated")]
    public async Task<ActionResult<PaginatedResult<Tag>>> GetTagsPaginated(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null)
    {
        var result = await _tagRepository.GetTagsPaginatedAsync(page, pageSize, searchTerm, isActive);
        return Ok(result);
    }

    /// <summary>
    /// Get a tag by ID
    /// </summary>
    /// <param name="id">The tag ID</param>
    /// <returns>The tag</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Tag>> GetTag(string id)
    {
        var tag = await _tagRepository.GetByIdAsync(id);
        if (tag == null)
            return NotFound();

        return Ok(tag);
    }

    /// <summary>
    /// Get a tag by name
    /// </summary>
    /// <param name="name">The tag name</param>
    /// <returns>The tag</returns>
    [HttpGet("name/{name}")]
    public async Task<ActionResult<Tag>> GetTagByName(string name)
    {
        var tag = await _tagRepository.GetByNameAsync(name);
        if (tag == null)
            return NotFound();

        return Ok(tag);
    }

    /// <summary>
    /// Get popular tags
    /// </summary>
    /// <param name="limit">Maximum number of tags</param>
    /// <returns>List of popular tags</returns>
    [HttpGet("popular")]
    public async Task<ActionResult<List<Tag>>> GetPopularTags([FromQuery] int limit = 10)
    {
        var tags = await _tagRepository.GetTagsWithPostCountAsync();
        return Ok(tags);
    }

    /// <summary>
    /// Create a new tag
    /// </summary>
    /// <param name="tag">The tag to create</param>
    /// <returns>The created tag</returns>
    [HttpPost]
    public async Task<ActionResult<Tag>> CreateTag([FromBody] Tag tag)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var createdTag = await _tagRepository.CreateTagAsync(tag);
        return CreatedAtAction(nameof(GetTag), new { id = createdTag.Id }, createdTag);
    }

    /// <summary>
    /// Update an existing tag
    /// </summary>
    /// <param name="id">The tag ID</param>
    /// <param name="tag">The updated tag</param>
    /// <returns>The updated tag</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<Tag>> UpdateTag(string id, [FromBody] Tag tag)
    {
        if (id != tag.Id)
            return BadRequest("ID mismatch");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updatedTag = await _tagRepository.UpdateTagAsync(tag);
        return Ok(updatedTag);
    }

    /// <summary>
    /// Delete a tag
    /// </summary>
    /// <param name="id">The tag ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTag(string id)
    {
        var deleted = await _tagRepository.DeleteTagAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Get tags with post count
    /// </summary>
    /// <returns>List of tags with post counts</returns>
    [HttpGet("with-post-count")]
    public async Task<ActionResult<List<Tag>>> GetTagsWithPostCount()
    {
        var tags = await _tagRepository.GetTagsWithPostCountAsync();
        return Ok(tags);
    }

    /// <summary>
    /// Get tags by post ID
    /// </summary>
    /// <param name="postId">The post ID</param>
    /// <returns>List of tags for the post</returns>
    [HttpGet("post/{postId}")]
    public async Task<ActionResult<List<Tag>>> GetTagsByPost(string postId)
    {
        var tags = await _tagRepository.GetTagsByPostAsync(postId);
        return Ok(tags);
    }

    /// <summary>
    /// Check if tag name exists
    /// </summary>
    /// <param name="name">The name to check</param>
    /// <param name="excludeId">ID to exclude from check (for updates)</param>
    /// <returns>True if name exists</returns>
    [HttpGet("check-name")]
    public async Task<ActionResult<bool>> CheckNameExists([FromQuery] string name, [FromQuery] string? excludeId = null)
    {
        var exists = await _tagRepository.NameExistsAsync(name, excludeId);
        return Ok(exists);
    }
}
