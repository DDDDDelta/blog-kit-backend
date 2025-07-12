using Microsoft.Extensions.DependencyInjection;
using BlogKit.Services;

namespace BlogKit;

/// <summary>
/// Extension methods for configuring BlogKit services
/// </summary>
public static class BlogKitExtensions
{
    /// <summary>
    /// Add BlogKit services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddBlogKit(this IServiceCollection services)
    {
        services.AddScoped<TagService>();
        services.AddScoped<BlogService>();
        
        return services;
    }
} 
