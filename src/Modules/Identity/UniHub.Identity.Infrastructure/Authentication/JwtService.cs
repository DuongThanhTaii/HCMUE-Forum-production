using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Tokens;
using UniHub.Identity.Domain.Users;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Infrastructure.Authentication;

/// <summary>
/// JWT token service implementation
/// </summary>
public sealed class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly SigningCredentials _signingCredentials;

    public TimeSpan AccessTokenExpiry => TimeSpan.FromMinutes(_jwtSettings.AccessTokenExpiryMinutes);
    public TimeSpan RefreshTokenExpiry => TimeSpan.FromDays(_jwtSettings.RefreshTokenExpiryDays);

    public JwtService(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
        _tokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
        _signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256);
    }

    public Result<string> GenerateAccessToken(User user, IEnumerable<string>? roleNames = null)
    {
        try
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.Value.ToString()),
                new(ClaimTypes.Email, user.Email.Value),
                new("user_id", user.Id.Value.ToString()),
                new("email", user.Email.Value),
                new("profile_name", user.Profile.FullName),
                new("jti", Guid.NewGuid().ToString()) // JWT ID for token uniqueness
            };

            // Add role claims by name so FE can read them
            if (roleNames is not null)
            {
                foreach (var roleName in roleNames)
                {
                    claims.Add(new Claim(ClaimTypes.Role, roleName));
                }
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(AccessTokenExpiry),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = _signingCredentials
            };

            var token = _tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = _tokenHandler.WriteToken(token);

            return Result.Success(tokenString);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(
                new Error("JwtService.TokenGeneration.Failed", 
                $"Failed to generate JWT token: {ex.Message}"));
        }
    }

    public Result<UserId> ValidateToken(string token)
    {
        try
        {
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // No tolerance for token expiry
            };

            var principal = _tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return Result.Failure<UserId>(
                    new Error("JwtService.TokenValidation.InvalidAlgorithm", 
                    "Token algorithm is not valid"));
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? principal.FindFirst("user_id")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userIdGuid))
            {
                return Result.Failure<UserId>(
                    new Error("JwtService.TokenValidation.InvalidUserId", 
                    "Token does not contain valid user ID"));
            }

            var userId = new UserId(userIdGuid);
            return Result.Success(userId);
        }
        catch (SecurityTokenExpiredException)
        {
            return Result.Failure<UserId>(
                new Error("JwtService.TokenValidation.Expired", 
                "Token has expired"));
        }
        catch (SecurityTokenException ex)
        {
            return Result.Failure<UserId>(
                new Error("JwtService.TokenValidation.Invalid", 
                $"Token validation failed: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return Result.Failure<UserId>(
                new Error("JwtService.TokenValidation.Failed", 
                $"Unexpected error during token validation: {ex.Message}"));
        }
    }

    public RefreshToken GenerateRefreshToken(UserId userId, string? ipAddress = null)
    {
        var randomBytes = new byte[64];
        RandomNumberGenerator.Fill(randomBytes);
        var token = Convert.ToBase64String(randomBytes);

        var expiresAt = DateTime.UtcNow.Add(RefreshTokenExpiry);

        return RefreshToken.Create(userId, token, expiresAt, ipAddress);
    }
}