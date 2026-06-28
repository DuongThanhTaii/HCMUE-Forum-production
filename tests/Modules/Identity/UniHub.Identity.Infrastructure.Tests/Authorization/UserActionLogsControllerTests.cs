using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UniHub.Contracts;
using UniHub.API.Controllers;
using UniHub.API.Middlewares;
using UniHub.API.Observability;

namespace UniHub.Identity.Infrastructure.Tests.Authorization;

public sealed class UserActionLogsControllerTests
{
    [Fact]
    public async Task Search_ShouldClampPageSizeByOptionsMax()
    {
        var store = new FakeUserActionLogStore();
        var options = Options.Create(new UserActionLoggingOptions
        {
            PersistToMongo = true,
            MongoCollectionName = "user_action_logs",
            DefaultQueryPageSize = 100,
            MaxQueryPageSize = 200
        });

        var controller = new UserActionLogsController(store, options);

        var result = await controller.Search(
            actorUserId: null,
            correlationId: null,
            traceId: null,
            method: null,
            pathContains: null,
            minStatusCode: null,
            maxStatusCode: null,
            fromUtc: null,
            toUtc: null,
            viewType: UserActionLogViewType.Developer,
            page: 1,
            pageSize: 999,
            cancellationToken: CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        store.LastQuery.Should().NotBeNull();
        store.LastQuery!.PageSize.Should().Be(200);
        store.LastQuery!.ViewType.Should().Be(UserActionLogViewType.Developer);

        var envelope = okResult.Value.Should().BeOfType<ApiResponse<UserActionLogSearchResponse>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        var payload = envelope.Data!;
        payload.ViewType.Should().Be("Developer");
        payload.Items.Should().ContainSingle();
        payload.Items[0].TerminalLine.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Search_WhenAdministratorView_ShouldHideSensitiveFields()
    {
        var store = new FakeUserActionLogStore();
        var options = Options.Create(new UserActionLoggingOptions
        {
            PersistToMongo = true,
            MongoCollectionName = "user_action_logs",
            DefaultQueryPageSize = 100,
            MaxQueryPageSize = 200
        });

        var controller = new UserActionLogsController(store, options);

        var result = await controller.Search(
            actorUserId: null,
            correlationId: null,
            traceId: null,
            method: null,
            pathContains: null,
            minStatusCode: null,
            maxStatusCode: null,
            fromUtc: null,
            toUtc: null,
            viewType: UserActionLogViewType.Administrator,
            page: 1,
            pageSize: 50,
            cancellationToken: CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<ApiResponse<UserActionLogSearchResponse>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        var payload = envelope.Data!;

        payload.ViewType.Should().Be("Administrator");
        payload.Items.Should().ContainSingle();
        payload.Items[0].QueryString.Should().BeEmpty();
        payload.Items[0].RemoteIp.Should().BeEmpty();
        payload.Items[0].UserAgent.Should().BeEmpty();
        payload.Items[0].ExceptionMessage.Should().BeNull();
        payload.Items[0].RequestHeadersJson.Should().Be("{}");
        payload.Items[0].RequestBodyPreview.Should().BeNull();
        payload.Items[0].ResponseHeadersJson.Should().Be("{}");
        payload.Items[0].ResponseBodyPreview.Should().BeNull();
    }

    private sealed class FakeUserActionLogStore : IUserActionLogStore
    {
        public UserActionLogQuery? LastQuery { get; private set; }

        public Task AppendAsync(UserActionLogEntry entry, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<UserActionLogSearchResult> SearchAsync(UserActionLogQuery query, CancellationToken cancellationToken = default)
        {
            LastQuery = query;
            var item = new UserActionLogEntry
            {
                Id = "67e0a6a5b58d674d0c8f8fbe",
                ActorUserId = "user-123",
                Method = "GET",
                Path = "/api/v1/users/me",
                StatusCode = 200,
                DurationMs = 10,
                TraceId = "trace-1",
                CorrelationId = "corr-1",
                StartedAtUtc = DateTime.UtcNow.AddMilliseconds(-10),
                CompletedAtUtc = DateTime.UtcNow,
                Result = "Success",
                ExceptionType = "InvalidOperationException",
                ExceptionMessage = "sample exception"
            };

            var result = new UserActionLogSearchResult([item], 1, query.Page, query.PageSize);
            return Task.FromResult(result);
        }
    }
}
