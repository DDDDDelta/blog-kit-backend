using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BlogKit.Services;
using BlogKit.Models;

namespace BlogKit.Controllers;

/// <summary>
/// Admin controller for tag operations (requires authentication)
/// </summary>
[ApiController]
[Route("api/admin/tags")]
[Authorize(Roles = "Admin")]
public class AdminTagController(TagService tagService) : ControllerBase
{
    private readonly TagService _tagService = tagService;

    /// <summary>
    /// Get all tags (admin only)
    /// </summary>
    /// <returns>List of tags</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Tag>>> GetAllTags()
    {
        var tags = await _tagService.GetTagsWithPostCountAsync();
        return Ok(tags);
    }

    /// <summary>
    /// Get a tag by ID (admin only)
    /// </summary>
    /// <param name="id">The tag ID</param>
    /// <returns>The tag</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Tag>> GetTagById(string id)
    {
        var tag = await _tagService.GetTagByIdAsync(id);
        if (tag == null)
        {
            return NotFound();
        }
        return Ok(tag);
    }

    /// <summary>
    /// Create a new tag (admin only)
    /// </summary>
    /// <param name="request">Request containing tag name and description</param>
    /// <returns>The created tag</returns>
    [HttpPost]
    public async Task<ActionResult<Tag>> CreateTag([FromBody] Tag tag)
    {
        if (string.IsNullOrWhiteSpace(tag.Name))
        {
            return BadRequest("Tag name is required.");
        }

        tag.Name = tag.Name.Trim();

        var createdTag = await _tagService.CreateTagAsync(tag);
        return CreatedAtAction(nameof(GetTagById), new { id = createdTag.Id }, createdTag);
    }

    /// <summary>
    /// Update an existing tag (admin only)
    /// </summary>
    /// <param name="id">The tag ID</param>
    /// <param name="request">Request containing updated tag name and description</param>
    /// <returns>The updated tag</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<Tag>> UpdateTag(string id, [FromBody] Tag tag)
    {
        if (string.IsNullOrWhiteSpace(tag.Name))
        {
            return BadRequest("Tag name is required.");
        }

        var existingTag = await _tagService.GetTagByIdAsync(id);
        if (existingTag == null)
        {
            return NotFound();
        }

        tag.Id = id; // Ensure the ID matches the route
        tag.Name = tag.Name.Trim();

        var updatedTag = await _tagService.UpdateTagAsync(tag);
        return Ok(updatedTag);
    }

    /// <summary>
    /// Delete a tag (admin only)
    /// </summary>
    /// <param name="id">The tag ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTag(string id)
    {
        var tag = await _tagService.GetTagByIdAsync(id);
        if (tag == null)
        {
            return NotFound();
        }

        await _tagService.DeleteTagAsync(id);
        return NoContent();
    }
}
