using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace RedCrossManager.Server.Infrastructure;

public sealed class LoggingActionFilter : IAsyncActionFilter
{
    private readonly ILogger<LoggingActionFilter> _logger;

    public LoggingActionFilter(ILogger<LoggingActionFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var httpContext = context.HttpContext;
        var stopwatch = Stopwatch.StartNew();
        var method = httpContext.Request.Method;
        var path = httpContext.Request.Path.Value ?? string.Empty;
        var actionName = context.ActionDescriptor.DisplayName ?? "unknown";
        var principal = httpContext.User;
        var isAuthenticated = principal?.Identity?.IsAuthenticated == true;
        var userId = isAuthenticated
            ? principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal?.FindFirstValue("sub")
            : null;
        var userHash = isAuthenticated && !string.IsNullOrWhiteSpace(userId)
            ? Hash(userId)
            : (isAuthenticated ? "authenticated" : "anonymous");
        var roles = isAuthenticated && principal is not null
            ? string.Join(',', principal.FindAll(ClaimTypes.Role).Select(r => r.Value).Distinct())
            : string.Empty;

        _logger.LogDebug("Request started {Method} {Path} | Action: {Action} | UserHash: {UserHash} | Roles: {Roles} | Authenticated: {IsAuthenticated}",
            method, path, actionName, userHash, roles, isAuthenticated);

        try
        {
            var resultContext = await next();
            stopwatch.Stop();

            var statusCode = httpContext.Response?.StatusCode;
            _logger.LogDebug("Request finished {Method} {Path} | Status: {StatusCode} | ElapsedMs: {ElapsedMs}",
                method, path, statusCode, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Request failed {Method} {Path} | ElapsedMs: {ElapsedMs}",
                method, path, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private static string Hash(string value)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
        var builder = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }
        return builder.ToString();
    }
}
