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
/// Tests for AdminBlogController endpoints
/// </summary>
public class AdminBlogControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IBlogRepository> _mockBlogRepository;
    private readonly Mock<ITagRepository> _mockTagRepository;

    public AdminBlogControllerTests(WebApplicationFactory<Program> factory)
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

    #region GetPost Tests

    [Fact]
    public async Task GetPost_WithValidId_ReturnsOkWithBlogPost()
    {
        // Arrange
        var client = _factory.CreateClient();
        var postId = "admin-test-post-1";
        var testPost = CreateTestBlogPost(postId, "Admin Test Post 1");

        _mockBlogRepository.Setup(x => x.GetByIdAsync(postId))
            .ReturnsAsync(testPost);

        // Act
        var response = await client.GetAsync($"/api/admin/blog/{postId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<BlogPost>(content);
        
        result.Should().NotBeNull();
        result!.Id.Should().Be(postId);
        result.Title.Should().Be("Admin Test Post 1");
    }

    [Fact]
    public async Task GetPost_WithIncrementViewCount_CallsIncrementMethod()
    {
        // Arrange
        var client = _factory.CreateClient();
        var postId = "admin-test-post-1";
        var testPost = CreateTestBlogPost(postId, "Admin Test Post 1");

        _mockBlogRepository.Setup(x => x.GetByIdAsync(postId))
            .ReturnsAsync(testPost);

        // Act
        var response = await client.GetAsync($"/api/admin/blog/{postId}?incrementViewCount=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _mockBlogRepository.Verify(x => x.IncrementViewCountAsync(postId), Times.Once);
    }

    #endregion

    #region CreatePost Tests

    [Fact]
    public async Task CreatePost_WithValidBlogPost_ReturnsCreatedWithBlogPost()
    {
        // Arrange
        var client = _factory.CreateClient();
        var newPost = CreateTestBlogPost("new-post-id", "New Admin Post");
        var createdPost = CreateTestBlogPost("new-post-id", "New Admin Post");
        
        var json = JsonSerializer.Serialize(newPost);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _mockBlogRepository.Setup(x => x.CreatePostAsync(It.IsAny<BlogPost>()))
            .ReturnsAsync(createdPost);

        // Act
        var response = await client.PostAsync("/api/admin/blog", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<BlogPost>(responseContent);
        
        result.Should().NotBeNull();
        result!.Id.Should().Be("new-post-id");
        result.Title.Should().Be("New Admin Post");
    }

    [Fact]
    public async Task CreatePost_WithInvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var invalidPost = new { }; // Invalid post without required fields
        
        var json = JsonSerializer.Serialize(invalidPost);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/admin/blog", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region UpdatePost Tests

    [Fact]
    public async Task UpdatePost_WithValidData_ReturnsOkWithUpdatedPost()
    {
        // Arrange
        var client = _factory.CreateClient();
        var postId = "update-test-post";
        var updatedPost = CreateTestBlogPost(postId, "Updated Admin Post");
        
        var json = JsonSerializer.Serialize(updatedPost);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _mockBlogRepository.Setup(x => x.UpdatePostAsync(It.IsAny<BlogPost>()))
            .ReturnsAsync(updatedPost);

        // Act
        var response = await client.PutAsync($"/api/admin/blog/{postId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<BlogPost>(responseContent);
        
        result.Should().NotBeNull();
        result!.Id.Should().Be(postId);
        result.Title.Should().Be("Updated Admin Post");
    }

    [Fact]
    public async Task UpdatePost_WithIdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var postId = "different-id";
        var updatedPost = CreateTestBlogPost("another-id", "Updated Admin Post");
        
        var json = JsonSerializer.Serialize(updatedPost);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PutAsync($"/api/admin/blog/{postId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("ID mismatch");
    }

    #endregion

    #region DeletePost Tests

    [Fact]
    public async Task DeletePost_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var client = _factory.CreateClient();
        var postId = "delete-test-post";

        _mockBlogRepository.Setup(x => x.DeletePostAsync(postId))
            .ReturnsAsync(true);

        // Act
        var response = await client.DeleteAsync($"/api/admin/blog/{postId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeletePost_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var postId = "non-existent-post";

        _mockBlogRepository.Setup(x => x.DeletePostAsync(postId))
            .ReturnsAsync(false);

        // Act
        var response = await client.DeleteAsync($"/api/admin/blog/{postId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region SetFeatured Tests

    [Fact]
    public async Task SetFeatured_WithValidId_ReturnsOkWithSuccessMessage()
    {
        // Arrange
        var client = _factory.CreateClient();
        var postId = "feature-test-post";
        var isFeatured = true;
        
        var json = JsonSerializer.Serialize(isFeatured);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _mockBlogRepository.Setup(x => x.SetFeaturedAsync(postId, isFeatured))
            .ReturnsAsync(true);

        // Act
        var response = await client.PostAsync($"/api/admin/blog/{postId}/feature", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<dynamic>(responseContent);
        
        responseContent.Should().Contain("featured successfully");
    }

    [Fact]
    public async Task SetFeatured_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var postId = "non-existent-post";
        var isFeatured = true;
        
        var json = JsonSerializer.Serialize(isFeatured);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _mockBlogRepository.Setup(x => x.SetFeaturedAsync(postId, isFeatured))
            .ReturnsAsync(false);

        // Act
        var response = await client.PostAsync($"/api/admin/blog/{postId}/feature", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Helper Methods

    private BlogPost CreateTestBlogPost(string id, string title, bool isFeatured = false)
    {
        return new BlogPost
        {
            Id = id,
            Title = title,
            Content = "Admin test content",
            Author = "Admin User",
            IsFeatured = isFeatured,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            Tags = new List<Tag>
            {
                new Tag { Id = "admin-tag-1", Name = "Admin Tag", IsActive = true }
            }
        };
    }

    #endregion
}
