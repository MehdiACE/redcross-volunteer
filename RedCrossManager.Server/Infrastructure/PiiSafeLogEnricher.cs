using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace RedCrossManager.Server.Infrastructure;

public sealed class PiiSafeLogEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PiiSafeLogEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null)
        {
            return;
        }

        var correlationId = context.Request.Headers.TryGetValue("X-Correlation-Id", out var headerValue)
            ? headerValue.ToString()
            : context.TraceIdentifier;

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CorrelationId", correlationId));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceId", Activity.Current?.TraceId.ToString() ?? string.Empty));

        var principal = context.User;
        var isAuthenticated = principal?.Identity?.IsAuthenticated == true;
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("IsAuthenticated", isAuthenticated));

        if (!isAuthenticated)
        {
            return;
        }

        var userId = principal?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal?.FindFirstValue("sub");
        if (!string.IsNullOrWhiteSpace(userId))
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserHash", Hash(userId)));
        }

        var roles = principal?.FindAll(ClaimTypes.Role).Select(r => r.Value).Distinct().ToArray() ?? Array.Empty<string>();
        if (roles.Length > 0)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserRoles", roles));
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
