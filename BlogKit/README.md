# BlogKit - .NET Core Blog Backend Library

A comprehensive, database-agnostic .NET Core library for building blog platforms with abstract interfaces, models, and controllers. BlogKit provides the foundation - you implement the data layer.

## Features

### 🚀 Core Features
- **Abstract Interfaces**: Database-agnostic design with interfaces for all operations
- **Complete Models**: Full data models for blog posts, tags, and authentication
- **Ready Controllers**: RESTful API controllers with public/admin separation
- **JWT Authentication**: Authentication models and controllers
- **Separated APIs**: Public and admin endpoints with different access levels

### 📊 Models
- **BlogPost**: Complete blog post with SEO, metadata, and engagement tracking
- **BlogSummary**: Lightweight blog post summary for listing pages (id, title, tags, author, publishDate)
- **Tag**: Flexible tagging system
- **AuthModels**: JWT authentication models (LoginRequest, LoginResponse, UserInfo, JwtSettings)

### 🔧 Architecture
- **Interface Pattern**: Abstract interfaces for all data operations
- **Model Layer**: Complete data models with validation
- **Controller Layer**: RESTful endpoints with public/admin separation
- **JWT Authentication**: Authentication models and controllers
- **Dependency Injection**: IoC container support
- **Async/Await**: Full asynchronous operations

## Installation

### NuGet Package
```bash
dotnet add package BlogKit
```

### Source Code
```bash
git clone <repository-url>
cd blog-kit-backend/BlogKit
dotnet build
```

## Quick Start

### 1. Register BlogKit

```csharp
// Program.cs or Startup.cs
using BlogKit;

var builder = WebApplication.CreateBuilder(args);

// Register BlogKit (provides interfaces and models only)
builder.Services.AddBlogKit();

// Add controllers
builder.Services.AddControllers();
```

### 2. Implement Required Interfaces

BlogKit provides interfaces that you must implement for your specific database:

```csharp
// Implement ITagService
public class YourTagService : ITagService
{
    public async Task<IEnumerable<Tag>> GetAllTagsAsync()
    {
        // Your database implementation
        return await _context.Tags.ToListAsync();
    }
    
    public async Task<Tag> CreateTagAsync(Tag tag)
    {
        // Your database implementation
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();
        return tag;
    }
    
    // Implement other methods...
}
```

// Implement IAuthService
public class YourAuthService : IAuthService
{
    public async Task<LoginResponse?> AuthenticateAsync(LoginRequest request)
    {
        // Your authentication implementation
        var user = await _userService.ValidateUserAsync(request.Username, request.Password);
        if (user == null) return null;
        
        var token = _jwtService.GenerateToken(user);
        return new LoginResponse { Token = token, User = user };
    }
    
    // Implement other methods...
}
```

### 3. Register Your Implementations

```csharp
// Register your implementations
builder.Services.AddScoped<ITagService, YourTagService>();
builder.Services.AddScoped<IAuthService, YourAuthService>();
```

### 4. Use the API Controllers

The library provides ready-to-use API controllers that accept Tag models directly:

- `TagController` - Public tag operations (read-only)
- `AuthController` - Authentication operations
- `AdminTagController` - Admin tag operations (requires authentication)

**Note**: The controllers use your implementations through dependency injection and accept Tag models directly without separate request objects. 