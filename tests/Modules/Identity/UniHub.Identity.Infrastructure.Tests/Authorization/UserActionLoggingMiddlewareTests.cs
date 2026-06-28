using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UniHub.API.Middlewares;
using UniHub.API.Observability;

namespace UniHub.Identity.Infrastructure.Tests.Authorization;

public sealed class UserActionLoggingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ShouldAttachCorrelationIdAndLog_ForIncludedPath()
    {
        var logger = new TestLogger<UserActionLoggingMiddleware>();
        var logStore = new TestUserActionLogStore();
        var options = Options.Create(new UserActionLoggingOptions
        {
            Enabled = true,
            CorrelationHeaderName = "X-Correlation-Id",
            ExcludedPathPrefixes = ["/health"],
            PersistToMongo = true
        });

        RequestDelegate next = context =>
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            return Task.CompletedTask;
        };

        var middleware = new UserActionLoggingMiddleware(next, logger, options);
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/users/me";
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("sub", "user-123")
        ], "TestAuth"));

        await middleware.InvokeAsync(context, logStore);

        context.Response.Headers.TryGetValue("X-Correlation-Id", out var correlationId).Should().BeTrue();
        correlationId.ToString().Should().NotBeNullOrWhiteSpace();
        logger.Messages.Should().Contain(message => message.Contains("User action completed"));
        logStore.Entries.Should().ContainSingle();
        logStore.Entries[0].ActorUserId.Should().Be("user-123");
        logStore.Entries[0].Path.Should().Be("/api/v1/users/me");
    }

    [Fact]
    public async Task InvokeAsync_ShouldSkipLogging_ForExcludedPath()
    {
        var logger = new TestLogger<UserActionLoggingMiddleware>();
        var logStore = new TestUserActionLogStore();
        var options = Options.Create(new UserActionLoggingOptions
        {
            Enabled = true,
            CorrelationHeaderName = "X-Correlation-Id",
            ExcludedPathPrefixes = ["/health"],
            PersistToMongo = true
        });

        RequestDelegate next = context =>
        {
            context.Response.StatusCode = StatusCodes.Status204NoContent;
            return Task.CompletedTask;
        };

        var middleware = new UserActionLoggingMiddleware(next, logger, options);
        var context = new DefaultHttpContext();
        context.Request.Path = "/health";

        await middleware.InvokeAsync(context, logStore);

        context.Response.Headers.ContainsKey("X-Correlation-Id").Should().BeFalse();
        logger.Messages.Should().BeEmpty();
        logStore.Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task InvokeAsync_ShouldNotPersist_WhenPersistenceDisabled()
    {
        var logger = new TestLogger<UserActionLoggingMiddleware>();
        var logStore = new TestUserActionLogStore();
        var options = Options.Create(new UserActionLoggingOptions
        {
            Enabled = true,
            PersistToMongo = false,
            CorrelationHeaderName = "X-Correlation-Id",
            ExcludedPathPrefixes = ["/health"]
        });

        RequestDelegate next = context =>
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            return Task.CompletedTask;
        };

        var middleware = new UserActionLoggingMiddleware(next, logger, options);
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/posts";

        await middleware.InvokeAsync(context, logStore);

        logStore.Entries.Should().BeEmpty();
    }

    private sealed class TestLogger<TCategoryName> : ILogger<TCategoryName>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }
    }

    private sealed class TestUserActionLogStore : IUserActionLogStore
    {
        public List<UserActionLogEntry> Entries { get; } = [];

        public Task AppendAsync(UserActionLogEntry entry, CancellationToken cancellationToken = default)
        {
            Entries.Add(entry);
            return Task.CompletedTask;
        }

        public Task<UserActionLogSearchResult> SearchAsync(UserActionLogQuery query, CancellationToken cancellationToken = default)
        {
            var result = new UserActionLogSearchResult(Entries, Entries.Count, query.Page, query.PageSize);
            return Task.FromResult(result);
        }
    }
}
