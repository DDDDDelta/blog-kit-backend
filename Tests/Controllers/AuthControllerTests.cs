using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using FluentAssertions;
using System.Net;
using System.Text.Json;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using BlogKit.Data;
using BlogKit.Models;
using BlogKit.Services;

namespace BlogKit.Tests.Controllers;

/// <summary>
/// Tests for AuthController endpoints
/// </summary>
public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IBlogRepository> _mockBlogRepository;
    private readonly Mock<ITagRepository> _mockTagRepository;
    private readonly Mock<IAuthService> _mockAuthService;

    public AuthControllerTests(WebApplicationFactory<Program> factory)
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

                var authServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IAuthService));
                if (authServiceDescriptor != null) services.Remove(authServiceDescriptor);

                _mockBlogRepository = new Mock<IBlogRepository>();
                _mockTagRepository = new Mock<ITagRepository>();
                _mockAuthService = new Mock<IAuthService>();
                
                services.AddSingleton(_mockBlogRepository.Object);
                services.AddSingleton(_mockTagRepository.Object);
                services.AddSingleton(_mockAuthService.Object);
            });
        });
    }

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var client = _factory.CreateClient();
        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "testpassword"
        };
        var loginResponse = new LoginResponse
        {
            Token = "test-jwt-token",
            UserInfo = new UserInfo
            {
                Username = "testuser",
                IsAdmin = false
            }
        };
        
        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _mockAuthService.Setup(x => x.AuthenticateAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync(loginResponse);

        // Act
        var response = await client.PostAsync("/api/auth/login", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<LoginResponse>(responseContent);
        
        result.Should().NotBeNull();
        result!.Token.Should().Be("test-jwt-token");
        result.UserInfo.Username.Should().Be("testuser");
        result.UserInfo.IsAdmin.Should().BeFalse();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var loginRequest = new LoginRequest
        {
            Username = "invaliduser",
            Password = "wrongpassword"
        };
        
        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _mockAuthService.Setup(x => x.AuthenticateAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync((LoginResponse?)null);

        // Act
        var response = await client.PostAsync("/api/auth/login", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Invalid username or password");
    }

    #endregion

    #region CheckAdminStatus Tests

    [Fact]
    public async Task CheckAdminStatus_WithAuthenticatedAdminUser_ReturnsTrue()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace real repositories and services with mocks
                var blogDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IBlogRepository));
                if (blogDescriptor != null) services.Remove(blogDescriptor);

                var tagDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ITagRepository));
                if (tagDescriptor != null) services.Remove(tagDescriptor);

                var authServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IAuthService));
                if (authServiceDescriptor != null) services.Remove(authServiceDescriptor);

                _mockBlogRepository = new Mock<IBlogRepository>();
                _mockTagRepository = new Mock<ITagRepository>();
                _mockAuthService = new Mock<IAuthService>();
                
                services.AddSingleton(_mockBlogRepository.Object);
                services.AddSingleton(_mockTagRepository.Object);
                services.AddSingleton(_mockAuthService.Object);

                // Add authentication scheme for testing
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
            });
        }).CreateClient();

        _mockAuthService.Setup(x => x.IsAdminAsync("adminuser"))
            .ReturnsAsync(true);

        // Act
        var response = await client.GetAsync("/api/auth/admin-check");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<bool>(content);
        
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckAdminStatus_WithUnauthenticatedUser_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/auth/admin-check");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("User not authenticated");
    }

    #endregion

    #region Helper Methods

    private LoginRequest CreateLoginRequest(string username, string password)
    {
        return new LoginRequest
        {
            Username = username,
            Password = password
        };
    }

    private LoginResponse CreateLoginResponse(string username, bool isAdmin = false)
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

    #endregion
}

