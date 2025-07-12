using BlogKit.Models;

namespace BlogKit.Services;

/// <summary>
/// Interface for authentication operations
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticate a user and generate login response
    /// </summary>
    /// <param name="request">Login request</param>
    /// <returns>Login response with token if successful, null otherwise</returns>
    Task<LoginResponse?> AuthenticateAsync(LoginRequest request);

    /// <summary>
    /// Validate if a user has admin role
    /// </summary>
    /// <param name="username">Username</param>
    /// <returns>True if user is admin, false otherwise</returns>
    Task<bool> IsAdminAsync(string username);

    /// <summary>
    /// Get user information by username
    /// </summary>
    /// <param name="username">Username</param>
    /// <returns>User information if found, null otherwise</returns>
    Task<UserInfo?> GetUserAsync(string username);
} 