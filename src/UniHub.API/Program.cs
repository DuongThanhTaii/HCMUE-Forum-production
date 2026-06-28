using System.Threading.RateLimiting;
using Serilog;
using Scalar.AspNetCore;
using FluentValidation;
using UniHub.Infrastructure.Behaviors;
using UniHub.Identity.Infrastructure;
using UniHub.Infrastructure;
using UniHub.Forum.Infrastructure;
using UniHub.Learning.Infrastructure;
using UniHub.Chat.Infrastructure;
using UniHub.Chat.Presentation;
using UniHub.Chat.Presentation.Hubs;
using UniHub.Career.Infrastructure;
using UniHub.Notification.Infrastructure;
using UniHub.Notification.Presentation.Hubs;
using UniHub.AI.Infrastructure;
using UniHub.API.Middlewares;
using UniHub.API.Integrations;
using UniHub.Forum.Presentation.Services;
using UniHub.Career.Presentation.Services;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .Build())
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProcessId()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/unihub-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
        retainedFileCountLimit: 30)
    .CreateLogger();

try
{
    Log.Information("Starting UniHub API");

    var builder = WebApplication.CreateBuilder(args);

    if (!builder.Configuration.GetValue<bool>("Authentication:AzureAd:Enabled"))
    {
        Log.Warning(
            "Authentication:AzureAd:Enabled is false. Microsoft access tokens will not validate on protected APIs or SignalR hubs; enable Azure AD and set TenantId, ClientId (API app), and Audience to match the SPA scope.");
    }

    // Use Serilog for logging
    builder.Host.UseSerilog();

    // Add services to the container.
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer((document, context, ct) =>
        {
            document.Info = new Microsoft.OpenApi.OpenApiInfo
            {
                Title = "UniHub API",
                Version = "v1",
                Description = "UniHub - University Hub for Students & Lecturers"
            };
            return Task.CompletedTask;
        });
    });

    // Add Controllers (for module API endpoints)
    builder.Services.AddControllers()
        .AddApplicationPart(typeof(UniHub.Identity.Presentation.Controllers.AuthController).Assembly)
        .AddApplicationPart(typeof(UniHub.Forum.Presentation.Controllers.PostsController).Assembly)
        .AddApplicationPart(typeof(UniHub.Learning.Presentation.Controllers.DocumentsController).Assembly)
        .AddApplicationPart(typeof(UniHub.Chat.Presentation.Controllers.ConversationsController).Assembly)
        .AddApplicationPart(typeof(UniHub.Career.Presentation.Controllers.JobPostingsController).Assembly)
        .AddApplicationPart(typeof(UniHub.Notification.Presentation.Controllers.NotificationsController).Assembly)
        .AddApplicationPart(typeof(UniHub.AI.Presentation.Controllers.AIChatController).Assembly);

    // Add CORS policy
    var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? new[] { "http://localhost:3000", "http://localhost:5173" };

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DefaultCorsPolicy", policy =>
        {
            policy.WithOrigins(corsOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Required for SignalR
        });
    });

    // Add MediatR for CQRS
    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssemblyContaining<UniHub.Identity.Application.Commands.Register.RegisterUserCommand>();
        cfg.RegisterServicesFromAssemblyContaining<UniHub.Forum.Application.Commands.CreatePost.CreatePostCommand>();
        cfg.RegisterServicesFromAssemblyContaining<UniHub.Learning.Application.Commands.UploadDocument.UploadDocumentCommand>();
        cfg.RegisterServicesFromAssemblyContaining<UniHub.Chat.Application.Commands.CreateDirectConversation.CreateDirectConversationCommand>();
        cfg.RegisterServicesFromAssemblyContaining<UniHub.Career.Application.Commands.JobPostings.CreateJobPosting.CreateJobPostingCommand>();
        cfg.RegisterServicesFromAssemblyContaining<UniHub.Notification.Application.EventHandlers.UserRegisteredEventHandler>();

        // Register pipeline behaviors (order matters: validation → logging → performance → unhandled → unit of work)
        cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
        cfg.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
        cfg.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
    });

    // Register FluentValidation validators from all module assemblies
    builder.Services.AddValidatorsFromAssemblyContaining<UniHub.Identity.Application.Commands.Register.RegisterUserCommand>();
    builder.Services.AddValidatorsFromAssemblyContaining<UniHub.Forum.Application.Commands.CreatePost.CreatePostCommand>();
    builder.Services.AddValidatorsFromAssemblyContaining<UniHub.Learning.Application.Commands.UploadDocument.UploadDocumentCommand>();
    builder.Services.AddValidatorsFromAssemblyContaining<UniHub.Chat.Application.Commands.CreateDirectConversation.CreateDirectConversationCommand>();
    builder.Services.AddValidatorsFromAssemblyContaining<UniHub.Career.Application.Commands.JobPostings.CreateJobPosting.CreateJobPostingCommand>();

    // Add Infrastructure (PostgreSQL, MongoDB, Redis)
    builder.Services.AddInfrastructure(builder.Configuration);

    // Add Identity module
    builder.Services.AddIdentityInfrastructure(builder.Configuration);

    // Add Forum module
    builder.Services.AddForumInfrastructure();
    builder.Services.Configure<ForumCloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));
    builder.Services.AddScoped<IForumAttachmentStorageService, ForumAttachmentStorageService>();

    // Add Learning module
    builder.Services.AddLearningInfrastructure();

    // Add Chat module (repositories + SignalR with Redis backplane)
    builder.Services.AddChatInfrastructure();
    builder.Services.AddChatPresentation(builder.Configuration);

    // Add Career module
    builder.Services.AddCareerInfrastructure();
    builder.Services.Configure<CareerCloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));
    builder.Services.AddScoped<ICareerLogoStorageService, CareerLogoStorageService>();

    // Add Notification module
    builder.Services.AddNotificationInfrastructure(builder.Configuration);
    builder.Services.AddScoped<
        UniHub.Notification.Application.Abstractions.Notifications.INotificationPusher,
        UniHub.Notification.Presentation.Services.SignalRNotificationPusher>();
    builder.Services.AddScoped<
        UniHub.Notification.Application.Abstractions.IPostAuthorLookup,
        UniHub.API.Services.PostAuthorLookup>();
    builder.Services.AddScoped<
        UniHub.Notification.Application.Abstractions.INotificationRecipientResolver,
        UniHub.API.Services.NotificationRecipientResolver>();

    // Add AI module
    builder.Services.AddAIInfrastructure(builder.Configuration);

    // Add exception handler
    builder.Services.AddExceptionHandler<UniHub.API.Middlewares.GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();
    builder.Services.Configure<UserActionLoggingOptions>(
        builder.Configuration.GetSection(UserActionLoggingOptions.SectionName));
    builder.Services.Configure<UEBotIntegrationOptions>(
        builder.Configuration.GetSection(UEBotIntegrationOptions.SectionName));
    builder.Services.AddSingleton<IUserActionLogStore, MongoUserActionLogStore>();

    // Add response compression
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
    });

    // Add rate limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        // Global default: 100 requests per minute per IP (OPTIONS preflight is not limited)
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            if (HttpMethods.IsOptions(context.Request.Method))
            {
                return RateLimitPartition.GetNoLimiter("preflight");
            }

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1)
                });
        });

        // Auth endpoints: 10 requests per minute (anti brute-force)
        options.AddPolicy("auth", context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1)
                }));

        // AI endpoints: 20 requests per minute (expensive calls)
        options.AddPolicy("ai", context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 20,
                    Window = TimeSpan.FromMinutes(1)
                }));

        // Integration endpoints: stricter per authenticated user (fallback to IP)
        options.AddPolicy("integrations", context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey:
                    context.User?.Identity?.IsAuthenticated == true
                        ? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anon-user"
                        : context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 30,
                    Window = TimeSpan.FromMinutes(1)
                }));

        // Forum write endpoints: reduce comment/reply spam bursts
        options.AddPolicy("forum-write", context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey:
                    context.User?.Identity?.IsAuthenticated == true
                        ? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anon-user"
                        : context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 15,
                    Window = TimeSpan.FromMinutes(1)
                }));
    });

    var app = builder.Build();

    // Use Serilog request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.FirstOrDefault());
            diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress);
        };
    });

    // Use exception handler
    app.UseExceptionHandler();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.Title = "UniHub API";
            options.Theme = ScalarTheme.BluePlanet;
            options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
            options.Authentication = new ScalarAuthenticationOptions
            {
                PreferredSecurityScheme = "Bearer"
            };
        });
    }

    app.UseResponseCompression();

    // Avoid redirecting browser preflight (http→https) during local dev — breaks CORS for SPA on http://localhost:5173
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    // Use CORS
    app.UseCors("DefaultCorsPolicy");

    // wwwroot/uploads/chat (and other public assets) — required for <audio>/<img> URLs returned by chat upload
    app.UseStaticFiles();

    // Use rate limiting
    app.UseRateLimiter();

    // Authentication & Authorization
    app.UseAuthentication();
    app.UseMiddleware<AzureUserProvisioningMiddleware>();
    app.UseMiddleware<UserActionLoggingMiddleware>();
    app.UseMiddleware<UniHub.API.Middlewares.EndpointToggleMiddleware>();
    app.UseAuthorization();

    // Map API controllers
    app.MapControllers();

    // Map SignalR Hubs
    app.MapHub<ChatHub>("/hubs/chat");
    app.MapHub<NotificationHub>("/hubs/notifications");

    // Health check endpoint
    app.MapHealthChecks("/health");

    // JWT test endpoint
    app.MapGet("/auth/test", () => Results.Ok(new { Message = "JWT Authentication is working!", Timestamp = DateTime.UtcNow }))
        .RequireAuthorization()
        .WithName("TestJwtAuth");

    // Connection test endpoint
    app.MapGet("/health/connections", (IConfiguration config) =>
    {
        var connections = new
        {
            PostgreSQL = !string.IsNullOrEmpty(config.GetConnectionString("DefaultConnection")),
            MongoDB = !string.IsNullOrEmpty(config.GetConnectionString("MongoDB")),
            Redis = !string.IsNullOrEmpty(config.GetConnectionString("Redis"))
        };

        return Results.Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            ConnectionsConfigured = connections
        });
    })
    .WithName("GetConnectionsHealth");

    // Seed database in development
    if (app.Environment.IsDevelopment())
    {
        await UniHub.Infrastructure.Persistence.Seeding.DatabaseSeeder.SeedAsync(app.Services);

        var exitAfterSeeding = app.Configuration.GetValue<bool>("Seeding:HighVolume:ExitAfterSeeding");
        if (exitAfterSeeding)
        {
            Log.Information("Seeding completed with ExitAfterSeeding=true. Shutting down host.");
            return;
        }
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
