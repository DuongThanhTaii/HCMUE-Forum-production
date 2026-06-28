using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Logging.Abstractions;
using UniHub.API.Middlewares;
using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Authorization;

namespace UniHub.Identity.Infrastructure.Tests.Authorization
{
    public sealed class EndpointToggleMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_ShouldBlockImmediately_WhenEndpointToggleIsDisabled()
        {
            var endpointKey = "Api.Identity.AuthorizationAdmin.GetEndpointToggles";
            var repository = new FakeEndpointToggleRepository();
            repository.Set(endpointKey, EndpointToggle.Create(endpointKey, false, "tester", "Maintenance window").Value);

            var nextCalled = false;
            RequestDelegate next = _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new EndpointToggleMiddleware(next, NullLogger<EndpointToggleMiddleware>.Instance);
            var context = BuildContext(endpointKey);

            await middleware.InvokeAsync(context, repository);

            context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
            nextCalled.Should().BeFalse();
        }

        [Fact]
        public async Task InvokeAsync_ShouldContinue_WhenEndpointToggleIsEnabled()
        {
            var endpointKey = "Api.Identity.AuthorizationAdmin.GetEndpointToggles";
            var repository = new FakeEndpointToggleRepository();
            repository.Set(endpointKey, EndpointToggle.Create(endpointKey, true, "tester").Value);

            var nextCalled = false;
            RequestDelegate next = context =>
            {
                nextCalled = true;
                context.Response.StatusCode = StatusCodes.Status204NoContent;
                return Task.CompletedTask;
            };

            var middleware = new EndpointToggleMiddleware(next, NullLogger<EndpointToggleMiddleware>.Instance);
            var context = BuildContext(endpointKey);

            await middleware.InvokeAsync(context, repository);

            nextCalled.Should().BeTrue();
            context.Response.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        }

        private static DefaultHttpContext BuildContext(string endpointKey)
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var actionDescriptor = new ControllerActionDescriptor
            {
                ControllerName = "AuthorizationAdmin",
                ActionName = "GetEndpointToggles",
                ControllerTypeInfo = typeof(UniHub.Modules.Identity.Test.DummyController).GetTypeInfo()
            };

            var endpoint = new RouteEndpoint(
                _ => Task.CompletedTask,
                RoutePatternFactory.Parse("/api/v1/admin/authorization/toggles"),
                0,
                new EndpointMetadataCollection(new AuthorizeAttribute(), actionDescriptor),
                endpointKey);

            context.SetEndpoint(endpoint);
            return context;
        }

        private sealed class FakeEndpointToggleRepository : IEndpointToggleRepository
        {
            private readonly Dictionary<string, EndpointToggle> _items = new(StringComparer.OrdinalIgnoreCase);

            public void Set(string endpointKey, EndpointToggle toggle)
            {
                _items[endpointKey] = toggle;
            }

            public Task<EndpointToggle?> GetByEndpointKeyAsync(string endpointKey, CancellationToken cancellationToken = default)
            {
                _items.TryGetValue(endpointKey, out var toggle);
                return Task.FromResult(toggle);
            }

            public Task<List<EndpointToggle>> GetAllAsync(CancellationToken cancellationToken = default)
            {
                return Task.FromResult(_items.Values.ToList());
            }

            public Task AddAsync(EndpointToggle endpointToggle, CancellationToken cancellationToken = default)
            {
                _items[endpointToggle.EndpointKey] = endpointToggle;
                return Task.CompletedTask;
            }

            public Task UpdateAsync(EndpointToggle endpointToggle, CancellationToken cancellationToken = default)
            {
                _items[endpointToggle.EndpointKey] = endpointToggle;
                return Task.CompletedTask;
            }
        }
    }
}

namespace UniHub.Modules.Identity.Test
{
    public sealed class DummyController;
}
