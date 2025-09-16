using BlogKit.Models;

namespace BlogKit.Tests.Helpers;

/// <summary>
/// Helper class for creating test data
/// </summary>
public static class TestDataBuilder
{
    /// <summary>
    /// Creates a test blog post with specified parameters
    /// </summary>
    public static BlogPost CreateBlogPost(string id = "test-id", string title = "Test Post", bool isFeatured = false)
    {
        return new BlogPost
        {
            Id = id,
            Title = title,
            Content = "Test content for the blog post",
            Author = "Test Author",
            IsFeatured = isFeatured,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            ViewCount = 0,
            Tags = new List<Tag>
            {
                new Tag { Id = "tag-1", Name = "Test Tag", IsActive = true }
            }
        };
    }

    /// <summary>
    /// Creates a test tag with specified parameters
    /// </summary>
    public static Tag CreateTag(string id = "tag-id", string name = "Test Tag")
    {
        return new Tag
        {
            Id = id,
            Name = name,
            IsActive = true
        };
    }

    /// <summary>
    /// Creates a test login request
    /// </summary>
    public static LoginRequest CreateLoginRequest(string username = "testuser", string password = "testpassword")
    {
        return new LoginRequest
        {
            Username = username,
            Password = password
        };
    }

    /// <summary>
    /// Creates a test login response
    /// </summary>
    public static LoginResponse CreateLoginResponse(string username = "testuser", bool isAdmin = false)
    {
        return new LoginResponse
        {
            Token = $"jwt-token-for-{username}",
            UserInfo = new UserInfo
            {
                Username = username,
                IsAdmin = isAdmin
            }
        };
    }

    /// <summary>
    /// Creates a list of test blog posts
    /// </summary>
    public static List<BlogPost> CreateBlogPosts(int count = 3)
    {
        var posts = new List<BlogPost>();
        for (int i = 1; i <= count; i++)
        {
            posts.Add(CreateBlogPost($"test-post-{i}", $"Test Post {i}", i == 1));
        }
        return posts;
    }

    /// <summary>
    /// Creates a list of test tags
    /// </summary>
    public static List<Tag> CreateTags(int count = 3)
    {
        var tags = new List<Tag>();
        for (int i = 1; i <= count; i++)
        {
            tags.Add(CreateTag($"tag-{i}", $"Test Tag {i}"));
        }
        return tags;
    }
}
