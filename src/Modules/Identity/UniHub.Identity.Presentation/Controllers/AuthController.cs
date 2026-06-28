using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniHub.Contracts;
using UniHub.Identity.Application.Commands.ForgotPassword;
using UniHub.Identity.Application.Commands.Login;
using UniHub.Identity.Application.Commands.RefreshToken;
using UniHub.Identity.Application.Commands.Register;
using UniHub.Identity.Application.Commands.ResetPassword;
using UniHub.Identity.Application.Commands.RevokeRefreshToken;
using UniHub.Identity.Presentation.DTOs.Auth;

namespace UniHub.Identity.Presentation.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
[EnableRateLimiting("auth")]
public class AuthController : BaseApiController
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Registered user information</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<RegisterResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(
            request.Email,
            request.Password,
            request.FullName,
            request.Bio);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        var response = new RegisterResponse(
            result.Value,
            request.Email,
            request.FullName);

        return CreatedAtAction(
            nameof(Register),
            new { id = response.UserId },
            ApiResponses.Success(response, "User registered successfully"));
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Access token and refresh token</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<DTOs.Auth.LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return Unauthorized(ApiResponses.Failure(result.Error.Message));
        }

        var response = new DTOs.Auth.LoginResponse(
            result.Value.AccessToken,
            result.Value.RefreshToken,
            result.Value.AccessTokenExpiresAt,
            result.Value.RefreshTokenExpiresAt);

        return Ok(ApiResponses.Success(response));
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New access token and refresh token</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<DTOs.Auth.LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return Unauthorized(ApiResponses.Failure(result.Error.Message));
        }

        var response = new DTOs.Auth.LoginResponse(
            result.Value.AccessToken,
            result.Value.RefreshToken,
            result.Value.AccessTokenExpiresAt,
            result.Value.RefreshTokenExpiresAt);

        return Ok(ApiResponses.Success(response));
    }

    /// <summary>
    /// Logout current user (revoke refresh token)
    /// </summary>
    /// <returns>Success message</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        
        var command = new RevokeRefreshTokenCommand(userId);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Logged out successfully"));
    }

    /// <summary>
    /// Request password reset token
    /// </summary>
    /// <param name="request">Email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ForgotPasswordCommand(request.Email);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            // Don't reveal if email exists for security
            return Ok(ApiResponses.Success((object)new { message = "If the email exists, a reset link will be sent" }));
        }

        // In production, send email here instead of returning token
        return Ok(ApiResponses.Success((object)new
        {
            token = result.Value.ResetToken,
            expiresAt = result.Value.ExpiresAt
        }, "Password reset token generated"));
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    /// <param name="request">Reset token and new password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ResetPasswordCommand(request.Token, request.NewPassword);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Password reset successfully"));
    }
}
