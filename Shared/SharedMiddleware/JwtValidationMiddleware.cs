using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Shared.Helpers;

namespace Shared.SharedMiddleware
{
    public class JwtValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string? _secretKey;
        private readonly string? _issuer;
        private readonly string? _audience;

        // Define public (unauthenticated) endpoints here
        private static readonly string[] PublicRoutes = new[]
        {
            "/api/authentication/login",
            "/api/user/register",
            "/api/user/reset-password",          // Forgot Password endpoint
            "/api/user/send-otp",          // Reset Password endpoint (if needed)
            "/api/user/verify-email",
        };

        public JwtValidationMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _secretKey = configuration["Jwt:Secret"]!;
            _issuer = configuration["Jwt:Issuer"]!;
            _audience = configuration["Jwt:Audience"]!;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var path = httpContext.Request.Path.Value?.ToLower();

            // Skip token validation for public routes
            if (!string.IsNullOrEmpty(path) && PublicRoutes.Contains(path, StringComparer.OrdinalIgnoreCase))
            {
                await _next(httpContext);
                return;
            }

            var token = httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await httpContext.Response.WriteAsync("Missing authentication token.");
                return;
            }

            var validationResult = JwtHelper.ValidateToken(token, _secretKey!, _issuer!, _audience!);

            if (validationResult == null)
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await httpContext.Response.WriteAsync("Invalid or expired JWT token.");
                return;
            }

            // Attach validated user info to HttpContext.User
            httpContext.User = validationResult;

            await _next(httpContext);
        }
    }
}
