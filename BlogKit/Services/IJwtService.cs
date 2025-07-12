using BlogKit.Models;

namespace BlogKit.Services;

/// <summary>
/// Interface for JWT token operations
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generate a JWT token for a user
    /// </summary>
    /// <param name="user">User information</param>
    /// <returns>JWT token string</returns>
    string GenerateToken(UserInfo user);

    /// <summary>
    /// Validate a JWT token
    /// </summary>
    /// <param name="token">JWT token string</param>
    /// <returns>User information if valid, null otherwise</returns>
    UserInfo? ValidateToken(string token);

    /// <summary>
    /// Get user information from JWT token without validation
    /// </summary>
    /// <param name="token">JWT token string</param>
    /// <returns>User information if valid, null otherwise</returns>
    UserInfo? GetUserFromToken(string token);
} 