using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using FluentAssertions;
using System.Net;
using System.Text.Json;
using BlogKit.Data;
using BlogKit.Models;
using BlogKit.Services;

namespace BlogKit.Tests.Controllers;

/// <summary>
/// Tests for BlogController endpoints
/// </summary>
public class BlogControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IBlogRepository> _mockBlogRepository;
    private readonly Mock<ITagRepository> _mockTagRepository;

    public BlogControllerTests(WebApplicationFactory<Program> factory)
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

    #region GetPosts Tests

    [Fact]
    public async Task GetPosts_WithValidParameters_ReturnsOkWithPaginatedResults()
    {
        // Arrange
        var client = _factory.CreateClient();
        var testData = CreateTestBlogPosts();
        var paginatedResult = new PaginatedResult<BlogPost>
        {
            Items = testData,
            TotalCount = testData.Count,
            Page = 1,
            PageSize = 10
        };

        _mockBlogRepository.Setup(x => x.GetPostsAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), 
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), 
            It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(paginatedResult);

        // Act
        var response = await client.GetAsync("/api/blog?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PaginatedResult<BlogSummary>>(content);
        
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetPosts_WithSearchTerm_ReturnsFilteredResults()
    {
        // Arrange
        var client = _factory.CreateClient();
        var searchTerm = "test";
        var testData = CreateTestBlogPosts().Where(p => p.Title.Contains(searchTerm)).ToList();
        var paginatedResult = new PaginatedResult<BlogPost>
        {
            Items = testData,
            TotalCount = testData.Count,
            Page = 1,
            PageSize = 10
        };

        _mockBlogRepository.Setup(x => x.GetPostsAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), 
            It.IsAny<string>(), searchTerm, It.IsAny<bool>(), 
            It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(paginatedResult);

        // Act
        var response = await client.GetAsync($"/api/blog?searchTerm={searchTerm}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PaginatedResult<BlogSummary>>(content);
        
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.Items.First().Title.Should().Contain(searchTerm);
    }

    #endregion

    #region GetPostById Tests

    [Fact]
    public async Task GetPostById_WithValidId_ReturnsOkWithBlogPost()
    {
        // Arrange
        var client = _factory.CreateClient();
        var postId = "test-post-1";
        var testPost = CreateTestBlogPost(postId, "Test Post 1");

        _mockBlogRepository.Setup(x => x.GetByIdAsync(postId))
            .ReturnsAsync(testPost);

        // Act
        var response = await client.GetAsync($"/api/blog/{postId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<BlogPost>(content);
        
        result.Should().NotBeNull();
        result!.Id.Should().Be(postId);
        result.Title.Should().Be("Test Post 1");
    }

    [Fact]
    public async Task GetPostById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var postId = "non-existent";

        _mockBlogRepository.Setup(x => x.GetByIdAsync(postId))
            .ReturnsAsync((BlogPost?)null);

        // Act
        var response = await client.GetAsync($"/api/blog/{postId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GetFeaturedPosts Tests

    [Fact]
    public async Task GetFeaturedPosts_WithValidLimit_ReturnsOkWithFeaturedPosts()
    {
        // Arrange
        var client = _factory.CreateClient();
        var limit = 3;
        var testData = CreateTestBlogPosts().Where(p => p.IsFeatured).Take(limit).ToList();

        _mockBlogRepository.Setup(x => x.GetFeaturedPostsAsync(limit))
            .ReturnsAsync(testData);

        // Act
        var response = await client.GetAsync($"/api/blog/featured?limit={limit}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<BlogSummary>>(content);
        
        result.Should().NotBeNull();
        result!.Should().HaveCount(1); // Only one featured post in test data
        result.First().IsFeatured.Should().BeTrue();
    }

    [Fact]
    public async Task GetFeaturedPosts_WithDefaultLimit_ReturnsOkWithDefaultLimit()
    {
        // Arrange
        var client = _factory.CreateClient();
        var testData = CreateTestBlogPosts().Where(p => p.IsFeatured).Take(5).ToList();

        _mockBlogRepository.Setup(x => x.GetFeaturedPostsAsync(5))
            .ReturnsAsync(testData);

        // Act
        var response = await client.GetAsync("/api/blog/featured");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<BlogSummary>>(content);
        
        result.Should().NotBeNull();
        result!.Should().HaveCount(1);
    }

    #endregion

    #region GetRecentPosts Tests

    [Fact]
    public async Task GetRecentPosts_WithValidLimit_ReturnsOkWithRecentPosts()
    {
        // Arrange
        var client = _factory.CreateClient();
        var limit = 2;
        var testData = CreateTestBlogPosts().OrderByDescending(p => p.CreatedAt).Take(limit).ToList();

        _mockBlogRepository.Setup(x => x.GetRecentPostsAsync(limit))
            .ReturnsAsync(testData);

        // Act
        var response = await client.GetAsync($"/api/blog/recent?limit={limit}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<BlogSummary>>(content);
        
        result.Should().NotBeNull();
        result!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRecentPosts_WithDefaultLimit_ReturnsOkWithDefaultLimit()
    {
        // Arrange
        var client = _factory.CreateClient();
        var testData = CreateTestBlogPosts().OrderByDescending(p => p.CreatedAt).Take(5).ToList();

        _mockBlogRepository.Setup(x => x.GetRecentPostsAsync(5))
            .ReturnsAsync(testData);

        // Act
        var response = await client.GetAsync("/api/blog/recent");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<BlogSummary>>(content);
        
        result.Should().NotBeNull();
        result!.Should().HaveCount(2);
    }

    #endregion

    #region Helper Methods

    private List<BlogPost> CreateTestBlogPosts()
    {
        return new List<BlogPost>
        {
            CreateTestBlogPost("test-post-1", "Test Post 1", isFeatured: true),
            CreateTestBlogPost("test-post-2", "Another Post", isFeatured: false)
        };
    }

    private BlogPost CreateTestBlogPost(string id, string title, bool isFeatured = false)
    {
        return new BlogPost
        {
            Id = id,
            Title = title,
            Content = "Test content",
            Author = "Test Author",
            IsFeatured = isFeatured,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            Tags = new List<Tag>
            {
                new Tag { Id = "tag-1", Name = "Test Tag", IsActive = true }
            }
        };
    }

    #endregion
}
