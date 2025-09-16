using Microsoft.AspNetCore.Mvc;
using BlogKit.Models;
using BlogKit.Services;
using Microsoft.Extensions.Logging;

namespace BlogKit.Controllers;

/// <summary>
/// Controller for authentication operations
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    /// <summary>
    /// Authenticate a user and get JWT token
    /// </summary>
    /// <param name="request">Login request</param>
    /// <returns>Login response with token</returns>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _authService.AuthenticateAsync(request);
        
        if (response == null)
            return Unauthorized("Invalid username or password");

        return Ok(response);
    }   

    /// <summary>
    /// Validate if current user is admin
    /// </summary>
    /// <returns>True if user is admin</returns>
    [HttpGet("admin-check")]
    public async Task<ActionResult<bool>> CheckAdminStatus()
    {
        var username = User.Identity?.Name;
        
        if (string.IsNullOrEmpty(username))
            return Unauthorized("User not authenticated");

        var isAdmin = await _authService.IsAdminAsync(username);
        return Ok(isAdmin);
    }
} 
