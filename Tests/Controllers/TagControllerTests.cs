using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using FluentAssertions;
using System.Net;
using System.Text.Json;
using System.Text;
using BlogKit.Data;
using BlogKit.Models;

namespace BlogKit.Tests.Controllers;

/// <summary>
/// Tests for TagController endpoints
/// </summary>
public class TagControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IBlogRepository> _mockBlogRepository;
    private readonly Mock<ITagRepository> _mockTagRepository;

    public TagControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace real repositories with mocks
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

    #region GetTags Tests

    [Fact]
    public async Task GetTags_WithDefaultParameters_ReturnsOkWithTags()
    {
        // Arrange
        var client = _factory.CreateClient();
        var testTags = CreateTestTags();

        _mockTagRepository.Setup(x => x.GetTagsWithPostCountAsync())
            .ReturnsAsync(testTags);

        // Act
        var response = await client.GetAsync("/api/tag");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<Tag>>(content);
        
        result.Should().NotBeNull();
        result!.Should().HaveCount(2);
        result.First().Name.Should().Be("Test Tag 1");
    }

    [Fact]
    public async Task GetTags_WithCustomParameters_ReturnsOkWithTags()
    {
        // Arrange
        var client = _factory.CreateClient();
        var testTags = CreateTestTags();

        _mockTagRepository.Setup(x => x.GetTagsWithPostCountAsync())
            .ReturnsAsync(testTags);

        // Act
        var response = await client.GetAsync("/api/tag?isActive=true&includePostCount=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<Tag>>(content);
        
        result.Should().NotBeNull();
        result!.Should().HaveCount(2);
    }

    #endregion

    #region GetTagsPaginated Tests

    [Fact]
    public async Task GetTagsPaginated_WithValidParameters_ReturnsOkWithPaginatedResults()
    {
        // Arrange
        var client = _factory.CreateClient();
        var testTags = CreateTestTags();
        var paginatedResult = new PaginatedResult<Tag>
        {
            Items = testTags,
            TotalCount = testTags.Count,
            Page = 1,
            PageSize = 10
        };

        _mockTagRepository.Setup(x => x.GetTagsPaginatedAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool?>()))
            .ReturnsAsync(paginatedResult);

        // Act
        var response = await client.GetAsync("/api/tag/paginated?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PaginatedResult<Tag>>(content);
        
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetTagsPaginated_WithSearchTerm_ReturnsFilteredResults()
    {
        // Arrange
        var client = _factory.CreateClient();
        var searchTerm = "test";
        var testTags = CreateTestTags().Where(t => t.Name.Contains(searchTerm)).ToList();
        var paginatedResult = new PaginatedResult<Tag>
        {
            Items = testTags,
            TotalCount = testTags.Count,
            Page = 1,
            PageSize = 10
        };

        _mockTagRepository.Setup(x => x.GetTagsPaginatedAsync(
            It.IsAny<int>(), It.IsAny<int>(), searchTerm, It.IsAny<bool?>()))
            .ReturnsAsync(paginatedResult);

        // Act
        var response = await client.GetAsync($"/api/tag/paginated?searchTerm={searchTerm}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PaginatedResult<Tag>>(content);
        
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Contain(searchTerm);
    }

    #endregion

    #region GetTag Tests

    [Fact]
    public async Task GetTag_WithValidId_ReturnsOkWithTag()
    {
        // Arrange
        var client = _factory.CreateClient();
        var tagId = "tag-1";
        var testTag = CreateTestTag(tagId, "Test Tag 1");

        _mockTagRepository.Setup(x => x.GetByIdAsync(tagId))
            .ReturnsAsync(testTag);

        // Act
        var response = await client.GetAsync($"/api/tag/{tagId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Tag>(content);
        
        result.Should().NotBeNull();
        result!.Id.Should().Be(tagId);
        result.Name.Should().Be("Test Tag 1");
    }

    [Fact]
    public async Task GetTag_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var tagId = "non-existent";

        _mockTagRepository.Setup(x => x.GetByIdAsync(tagId))
            .ReturnsAsync((Tag?)null);

        // Act
        var response = await client.GetAsync($"/api/tag/{tagId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GetTagByName Tests

    [Fact]
    public async Task GetTagByName_WithValidName_ReturnsOkWithTag()
    {
        // Arrange
        var client = _factory.CreateClient();
        var tagName = "test-tag";
        var testTag = CreateTestTag("tag-1", tagName);

        _mockTagRepository.Setup(x => x.GetByNameAsync(tagName))
            .ReturnsAsync(testTag);

        // Act
        var response = await client.GetAsync($"/api/tag/name/{tagName}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Tag>(content);
        
        result.Should().NotBeNull();
        result!.Name.Should().Be(tagName);
    }

    [Fact]
    public async Task GetTagByName_WithNonExistentName_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var tagName = "non-existent-tag";

        _mockTagRepository.Setup(x => x.GetByNameAsync(tagName))
            .ReturnsAsync((Tag?)null);

        // Act
        var response = await client.GetAsync($"/api/tag/name/{tagName}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GetPopularTags Tests

    [Fact]
    public async Task GetPopularTags_WithValidLimit_ReturnsOkWithTags()
    {
        // Arrange
        var client = _factory.CreateClient();
        var limit = 5;
        var testTags = CreateTestTags();

        _mockTagRepository.Setup(x => x.GetTagsWithPostCountAsync())
            .ReturnsAsync(testTags);

        // Act
        var response = await client.GetAsync($"/api/tag/popular?limit={limit}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<Tag>>(content);
        
        result.Should().NotBeNull();
        result!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPopularTags_WithDefaultLimit_ReturnsOkWithDefaultLimit()
    {
        // Arrange
        var client = _factory.CreateClient();
        var testTags = CreateTestTags();

        _mockTagRepository.Setup(x => x.GetTagsWithPostCountAsync())
            .ReturnsAsync(testTags);

        // Act
        var response = await client.GetAsync("/api/tag/popular");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<Tag>>(content);
        
        result.Should().NotBeNull();
        result!.Should().HaveCount(2);
    }

    #endregion

    #region CreateTag Tests

    [Fact]
    public async Task CreateTag_WithValidTag_ReturnsCreatedWithTag()
    {
        // Arrange
        var client = _factory.CreateClient();
        var newTag = CreateTestTag("new-tag-id", "New Tag");
        
        var json = JsonSerializer.Serialize(newTag);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _mockTagRepository.Setup(x => x.CreateTagAsync(It.IsAny<Tag>()))
            .ReturnsAsync(newTag);

        // Act
        var response = await client.PostAsync("/api/tag", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Tag>(responseContent);
        
        result.Should().NotBeNull();
        result!.Id.Should().Be("new-tag-id");
        result.Name.Should().Be("New Tag");
    }

    [Fact]
    public async Task CreateTag_WithInvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var invalidTag = new { }; // Invalid tag without required fields
        
        var json = JsonSerializer.Serialize(invalidTag);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/tag", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region UpdateTag Tests

    [Fact]
    public async Task UpdateTag_WithValidData_ReturnsOkWithUpdatedTag()
    {
        // Arrange
        var client = _factory.CreateClient();
        var tagId = "update-tag-id";
        var updatedTag = CreateTestTag(tagId, "Updated Tag");
        
        var json = JsonSerializer.Serialize(updatedTag);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _mockTagRepository.Setup(x => x.UpdateTagAsync(It.IsAny<Tag>()))
            .ReturnsAsync(updatedTag);

        // Act
        var response = await client.PutAsync($"/api/tag/{tagId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Tag>(responseContent);
        
        result.Should().NotBeNull();
        result!.Id.Should().Be(tagId);
        result.Name.Should().Be("Updated Tag");
    }

    [Fact]
    public async Task UpdateTag_WithIdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var tagId = "different-id";
        var updatedTag = CreateTestTag("another-id", "Updated Tag");
        
        var json = JsonSerializer.Serialize(updatedTag);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PutAsync($"/api/tag/{tagId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("ID mismatch");
    }

    #endregion

    #region DeleteTag Tests

    [Fact]
    public async Task DeleteTag_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var client = _factory.CreateClient();
        var tagId = "delete-tag-id";

        _mockTagRepository.Setup(x => x.DeleteTagAsync(tagId))
            .ReturnsAsync(true);

        // Act
        var response = await client.DeleteAsync($"/api/tag/{tagId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteTag_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var tagId = "non-existent-tag";

        _mockTagRepository.Setup(x => x.DeleteTagAsync(tagId))
            .ReturnsAsync(false);

        // Act
        var response = await client.DeleteAsync($"/api/tag/{tagId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GetTagsWithPostCount Tests

    [Fact]
    public async Task GetTagsWithPostCount_ReturnsOkWithTags()
    {
        // Arrange
        var client = _factory.CreateClient();
        var testTags = CreateTestTags();

        _mockTagRepository.Setup(x => x.GetTagsWithPostCountAsync())
            .ReturnsAsync(testTags);

        // Act
        var response = await client.GetAsync("/api/tag/with-post-count");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<Tag>>(content);
        
        result.Should().NotBeNull();
        result!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTagsWithPostCount_WithEmptyResult_ReturnsOkWithEmptyList()
    {
        // Arrange
        var client = _factory.CreateClient();
        var emptyTags = new List<Tag>();

        _mockTagRepository.Setup(x => x.GetTagsWithPostCountAsync())
            .ReturnsAsync(emptyTags);

        // Act
        var response = await client.GetAsync("/api/tag/with-post-count");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<Tag>>(content);
        
        result.Should().NotBeNull();
        result!.Should().BeEmpty();
    }

    #endregion

    #region GetTagsByPost Tests

    [Fact]
    public async Task GetTagsByPost_WithValidPostId_ReturnsOkWithTags()
    {
        // Arrange
        var client = _factory.CreateClient();
        var postId = "test-post-1";
        var testTags = CreateTestTags();

        _mockTagRepository.Setup(x => x.GetTagsByPostAsync(postId))
            .ReturnsAsync(testTags);

        // Act
        var response = await client.GetAsync($"/api/tag/post/{postId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<Tag>>(content);
        
        result.Should().NotBeNull();
        result!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTagsByPost_WithNonExistentPostId_ReturnsOkWithEmptyList()
    {
        // Arrange
        var client = _factory.CreateClient();
        var postId = "non-existent-post";
        var emptyTags = new List<Tag>();

        _mockTagRepository.Setup(x => x.GetTagsByPostAsync(postId))
            .ReturnsAsync(emptyTags);

        // Act
        var response = await client.GetAsync($"/api/tag/post/{postId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<Tag>>(content);
        
        result.Should().NotBeNull();
        result!.Should().BeEmpty();
    }

    #endregion

    #region CheckNameExists Tests

    [Fact]
    public async Task CheckNameExists_WithExistingName_ReturnsTrue()
    {
        // Arrange
        var client = _factory.CreateClient();
        var tagName = "existing-tag";

        _mockTagRepository.Setup(x => x.NameExistsAsync(tagName, null))
            .ReturnsAsync(true);

        // Act
        var response = await client.GetAsync($"/api/tag/check-name?name={tagName}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<bool>(content);
        
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckNameExists_WithNonExistentName_ReturnsFalse()
    {
        // Arrange
        var client = _factory.CreateClient();
        var tagName = "non-existent-tag";

        _mockTagRepository.Setup(x => x.NameExistsAsync(tagName, null))
            .ReturnsAsync(false);

        // Act
        var response = await client.GetAsync($"/api/tag/check-name?name={tagName}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<bool>(content);
        
        result.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private List<Tag> CreateTestTags()
    {
        return new List<Tag>
        {
            CreateTestTag("tag-1", "Test Tag 1"),
            CreateTestTag("tag-2", "Test Tag 2")
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
