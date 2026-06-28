using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using UniHub.Notification.Application.Abstractions.Notifications;
using UniHub.Notification.Domain.Notifications;
using UniHub.Notification.Domain.NotificationTemplates;
using UniHub.SharedKernel.Results;

namespace UniHub.Notification.Infrastructure.Services.Notifications;

/// <summary>
/// Email notification service implementation using MailKit/MimeKit.
/// Sends email notifications via SMTP.
/// </summary>
public sealed class EmailNotificationService : IEmailNotificationService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailNotificationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailNotificationService"/> class.
    /// </summary>
    /// <param name="settings">Email configuration settings.</param>
    /// <param name="logger">Logger instance.</param>
    public EmailNotificationService(
        IOptions<EmailSettings> settings,
        ILogger<EmailNotificationService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public NotificationChannel Channel => NotificationChannel.Email;

    /// <inheritdoc />
    public async Task<Result> SendAsync(
        Domain.Notifications.Notification notification,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Sending email notification {NotificationId} to recipient {RecipientId}",
                notification.Id.Value,
                notification.RecipientId);

            // In a real implementation, you would need to retrieve the user's email address
            // from a repository/database. For now, this is a placeholder.
            
            // TODO: Implement IUserRepository to retrieve user's email address
            // var user = await _userRepository.GetByIdAsync(
            //     notification.RecipientId, 
            //     cancellationToken);
            
            // if (user is null || string.IsNullOrWhiteSpace(user.Email))
            // {
            //     return Result.Failure(
            //         new Error(
            //             "User.NotFound",
            //             $"No user found with ID {notification.RecipientId}"));
            // }

            // For now, return failure indicating user lookup is not implemented
            return Result.Failure(
                new Error(
                    "Email.NotImplemented",
                    "User repository not yet implemented. Will be added in future iteration."));

            // When user repository is available, uncomment this:
            // return await SendEmailAsync(
            //     user.Email,
            //     notification.Content.Subject,
            //     notification.Content.Body,
            //     cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send email notification {NotificationId}",
                notification.Id.Value);

            return Result.Failure(
                new Error(
                    "Email.SendFailed",
                    $"Failed to send email notification: {ex.Message}"));
        }
    }

    /// <summary>
    /// Sends an email to a specific recipient.
    /// </summary>
    /// <param name="toEmail">Recipient email address.</param>
    /// <param name="subject">Email subject.</param>
    /// <param name="body">Email body (HTML or plain text).</param>
    /// <param name="isHtml">Whether the body is HTML.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    public async Task<Result> SendEmailAsync(
        string toEmail,
        string subject,
        string body,
        bool isHtml = false,
        CancellationToken cancellationToken = default)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            return Result.Failure(
                new Error("Email.ToEmailRequired", "Recipient email is required"));
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            return Result.Failure(
                new Error("Email.SubjectRequired", "Email subject is required"));
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            return Result.Failure(
                new Error("Email.BodyRequired", "Email body is required"));
        }

        // Build email message
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder();
        if (isHtml)
        {
            bodyBuilder.HtmlBody = body;
        }
        else
        {
            bodyBuilder.TextBody = body;
        }
        message.Body = bodyBuilder.ToMessageBody();

        // Send email with retry logic
        for (int attempt = 1; attempt <= _settings.MaxRetryAttempts; attempt++)
        {
            try
            {
                using var client = new SmtpClient();
                client.Timeout = _settings.TimeoutSeconds * 1000;

                _logger.LogInformation(
                    "Connecting to SMTP server {SmtpHost}:{SmtpPort} (attempt {Attempt})",
                    _settings.SmtpHost,
                    _settings.SmtpPort,
                    attempt);

                // Connect to SMTP server
                var secureSocketOptions = _settings.EnableSsl
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.None;

                await client.ConnectAsync(
                    _settings.SmtpHost,
                    _settings.SmtpPort,
                    secureSocketOptions,
                    cancellationToken);

                // Authenticate
                await client.AuthenticateAsync(
                    _settings.Username,
                    _settings.Password,
                    cancellationToken);

                // Send message
                await client.SendAsync(message, cancellationToken);

                // Disconnect
                await client.DisconnectAsync(true, cancellationToken);

                _logger.LogInformation(
                    "Successfully sent email to {ToEmail} on attempt {Attempt}",
                    toEmail,
                    attempt);

                return Result.Success();
            }
            catch (Exception ex) when (attempt < _settings.MaxRetryAttempts)
            {
                _logger.LogWarning(
                    ex,
                    "Email send attempt {Attempt} failed. Retrying...",
                    attempt);

                // Wait before retry (exponential backoff)
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send email after {MaxAttempts} attempts",
                    _settings.MaxRetryAttempts);

                return Result.Failure(
                    new Error(
                        "Email.SendFailed",
                        $"Failed to send email: {ex.Message}"));
            }
        }

        // All retry attempts exhausted
        return Result.Failure(
            new Error(
                "Email.MaxRetriesExceeded",
                $"Failed to send email after {_settings.MaxRetryAttempts} attempts"));
    }
}
