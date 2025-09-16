using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using FluentAssertions;
using System.Net;
using System.Text.Json;
using System.Text;
using BlogKit.Data;
using BlogKit.Models;
using BlogKit.Services;

namespace BlogKit.Tests.Controllers;

/// <summary>
/// Tests for AdminTagController endpoints
/// </summary>
public class AdminTagControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IBlogRepository> _mockBlogRepository;
    private readonly Mock<ITagRepository> _mockTagRepository;

    public AdminTagControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace real repositories and services with mocks
                var blogDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IBlogRepository));
                if (blogDescriptor != null) services.Remove(blogDescriptor);

                var tagDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ITagRepository));
                if (tagDescriptor != null) services.Remove(tagDescriptor);

                _mockBlogRepository = new Mock<IBlogRepository>();
                _mockTagRepository = new Mock<ITagRepository>();
                
                services.AddSingleton(_mockBlogRepository.Object);
                services.AddSingleton(_mockTagRepository.Object);
            });
        });
    }

    #region GetAllTags Tests

    [Fact]
    public async Task GetAllTags_ReturnsOkWithTags()
    {
        // Arrange
        var client = _factory.CreateClient();
        var testTags = CreateTestTags();

        _mockTagRepository.Setup(x => x.GetTagsWithPostCountAsync())
            .ReturnsAsync(testTags);

        // Act
        var response = await client.GetAsync("/api/admin/tags");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<Tag>>(content);
        
        result.Should().NotBeNull();
        result!.Should().HaveCount(2);
        result.First().Name.Should().Be("Admin Test Tag 1");
    }

    [Fact]
    public async Task GetAllTags_WithEmptyResult_ReturnsOkWithEmptyList()
    {
        // Arrange
        var client = _factory.CreateClient();
        var emptyTags = new List<Tag>();

        _mockTagRepository.Setup(x => x.GetTagsWithPostCountAsync())
            .ReturnsAsync(emptyTags);

        // Act
        var response = await client.GetAsync("/api/admin/tags");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<Tag>>(content);
        
        result.Should().NotBeNull();
        result!.Should().BeEmpty();
    }

    #endregion

    #region GetTagById Tests

    [Fact]
    public async Task GetTagById_WithValidId_ReturnsOkWithTag()
    {
        // Arrange
        var client = _factory.CreateClient();
        var tagId = "admin-tag-1";
        var testTag = CreateTestTag(tagId, "Admin Test Tag 1");

        _mockTagRepository.Setup(x => x.GetByIdAsync(tagId))
            .ReturnsAsync(testTag);

        // Act
        var response = await client.GetAsync($"/api/admin/tags/{tagId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Tag>(content);
        
        result.Should().NotBeNull();
        result!.Id.Should().Be(tagId);
        result.Name.Should().Be("Admin Test Tag 1");
    }

    [Fact]
    public async Task GetTagById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var tagId = "non-existent-admin-tag";

        _mockTagRepository.Setup(x => x.GetByIdAsync(tagId))
            .ReturnsAsync((Tag?)null);

        // Act
        var response = await client.GetAsync($"/api/admin/tags/{tagId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region CreateTag Tests

    [Fact]
    public async Task CreateTag_WithValidTag_ReturnsCreatedWithTag()
    {
        // Arrange
        var client = _factory.CreateClient();
        var newTag = CreateTestTag("new-admin-tag-id", "New Admin Tag");
        
        var json = JsonSerializer.Serialize(newTag);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _mockTagRepository.Setup(x => x.CreateTagAsync(It.IsAny<Tag>()))
            .ReturnsAsync(newTag);

        // Act
        var response = await client.PostAsync("/api/admin/tags", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Tag>(responseContent);
        
        result.Should().NotBeNull();
        result!.Id.Should().Be("new-admin-tag-id");
        result.Name.Should().Be("New Admin Tag");
    }

    [Fact]
    public async Task CreateTag_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var invalidTag = CreateTestTag("", ""); // Empty name
        
        var json = JsonSerializer.Serialize(invalidTag);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/admin/tags", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Tag name is required");
    }

    #endregion

    #region UpdateTag Tests

    [Fact]
    public async Task UpdateTag_WithValidData_ReturnsOkWithUpdatedTag()
    {
        // Arrange
        var client = _factory.CreateClient();
        var tagId = "update-admin-tag-id";
        var existingTag = CreateTestTag(tagId, "Existing Admin Tag");
        var updatedTag = CreateTestTag(tagId, "Updated Admin Tag");
        
        var json = JsonSerializer.Serialize(updatedTag);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _mockTagRepository.Setup(x => x.GetByIdAsync(tagId))
            .ReturnsAsync(existingTag);
        _mockTagRepository.Setup(x => x.UpdateTagAsync(It.IsAny<Tag>()))
            .ReturnsAsync(updatedTag);

        // Act
        var response = await client.PutAsync($"/api/admin/tags/{tagId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Tag>(responseContent);
        
        result.Should().NotBeNull();
        result!.Id.Should().Be(tagId);
        result.Name.Should().Be("Updated Admin Tag");
    }

    [Fact]
    public async Task UpdateTag_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var tagId = "non-existent-admin-tag";
        var updatedTag = CreateTestTag(tagId, "Updated Admin Tag");
        
        var json = JsonSerializer.Serialize(updatedTag);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _mockTagRepository.Setup(x => x.GetByIdAsync(tagId))
            .ReturnsAsync((Tag?)null);

        // Act
        var response = await client.PutAsync($"/api/admin/tags/{tagId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DeleteTag Tests

    [Fact]
    public async Task DeleteTag_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var client = _factory.CreateClient();
        var tagId = "delete-admin-tag-id";
        var existingTag = CreateTestTag(tagId, "Tag To Delete");

        _mockTagRepository.Setup(x => x.GetByIdAsync(tagId))
            .ReturnsAsync(existingTag);
        _mockTagRepository.Setup(x => x.DeleteTagAsync(tagId))
            .ReturnsAsync(true);

        // Act
        var response = await client.DeleteAsync($"/api/admin/tags/{tagId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _mockTagRepository.Verify(x => x.DeleteTagAsync(tagId), Times.Once);
    }

    [Fact]
    public async Task DeleteTag_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var tagId = "non-existent-admin-tag";

        _mockTagRepository.Setup(x => x.GetByIdAsync(tagId))
            .ReturnsAsync((Tag?)null);

        // Act
        var response = await client.DeleteAsync($"/api/admin/tags/{tagId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Helper Methods

    private List<Tag> CreateTestTags()
    {
        return new List<Tag>
        {
            CreateTestTag("admin-tag-1", "Admin Test Tag 1"),
            CreateTestTag("admin-tag-2", "Admin Test Tag 2")
        };
    }

    private Tag CreateTestTag(string id, string name)
    {
        return new Tag
        {
            Id = id,
            Name = name,
            IsActive = true
        };
    }

    #endregion
}
