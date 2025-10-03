using CatCat.API.Models;
using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Yitter.IdGenerator;
using ZiggyCreatures.Caching.Fusion;

namespace CatCat.API.Endpoints;

public static class AuthEndpoints
{
    private static readonly JwtSecurityTokenHandler _jwtTokenHandler = new();
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, SymmetricSecurityKey> _keyCache = new();

    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("/send-code", SendCode)
            .WithName("SendCode")
            .WithSummary("Send SMS verification code");

        group.MapPost("/register", Register)
            .WithName("Register")
            .WithSummary("User registration");

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("User login");

        group.MapPost("/refresh", RefreshToken)
            .WithName("RefreshToken")
            .WithSummary("Refresh access token");

        group.MapPost("/logout", Logout)
            .WithName("Logout")
            .WithSummary("Logout and revoke refresh tokens")
            .RequireAuthorization();
    }

    private static async Task<IResult> SendCode(
        [FromBody] SendCodeRequest request,
        IFusionCache cache,
        ILogger<Program> logger)
    {
        var code = Random.Shared.Next(100000, 999999).ToString();
        await cache.SetAsync($"sms:code:{request.Phone}", code, TimeSpan.FromMinutes(5));

        // TODO: Integrate SMS service (e.g., Twilio, Aliyun SMS)
        logger.LogInformation("SMS code {Code} for {Phone}", code, request.Phone);

        return Results.Ok(new MessageResponse("Verification code sent"));
    }

    private static async Task<IResult> Register(
        [FromBody] RegisterRequest request,
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IFusionCache cache,
        IConfiguration configuration,
        HttpContext httpContext)
    {
        var cachedCode = await cache.TryGetAsync<string>($"sms:code:{request.Phone}");
        if (!cachedCode.HasValue || cachedCode.Value != request.Code)
        {
            return Results.BadRequest(new MessageResponse("Invalid or expired verification code"));
        }

        var existingUser = await userRepository.GetByPhoneAsync(request.Phone);
        if (existingUser != null)
        {
            return Results.BadRequest(new MessageResponse("Phone number already registered"));
        }

        var user = new User
        {
            Phone = request.Phone,
            NickName = request.NickName ?? $"User{request.Phone[^4..]}",
            PasswordHash = HashPassword(request.Password, configuration),
            Role = UserRole.Customer,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var userId = await userRepository.CreateAsync(user);
        user.Id = userId;

        await cache.RemoveAsync($"sms:code:{request.Phone}");

        var (accessToken, refreshToken) = await GenerateTokenPair(user, refreshTokenRepository, configuration, httpContext);

        return Results.Ok(new AuthResponse(
            accessToken,
            refreshToken,
            new UserInfo(user.Id, user.Phone, user.NickName, user.Avatar, user.Role)
        ));
    }

    private static async Task<IResult> Login(
        [FromBody] LoginRequest request,
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IConfiguration configuration,
        HttpContext httpContext)
    {
        var user = await userRepository.GetByPhoneAsync(request.Phone);
        if (user == null || !VerifyPassword(request.Password, user.PasswordHash, configuration))
        {
            return Results.BadRequest(new MessageResponse("Invalid phone number or password"));
        }

        if (user.Status != UserStatus.Active)
        {
            return Results.BadRequest(new MessageResponse("Account has been disabled"));
        }

        var (accessToken, refreshToken) = await GenerateTokenPair(user, refreshTokenRepository, configuration, httpContext);

        return Results.Ok(new AuthResponse(
            accessToken,
            refreshToken,
            new UserInfo(user.Id, user.Phone, user.NickName, user.Avatar, user.Role)
        ));
    }

    private static async Task<IResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IConfiguration configuration,
        HttpContext httpContext)
    {
        var refreshToken = await refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
        if (refreshToken == null || !refreshToken.IsActive)
        {
            return Results.Unauthorized();
        }

        var user = await userRepository.GetByIdAsync(refreshToken.UserId);
        if (user == null || user.Status != UserStatus.Active)
        {
            return Results.Unauthorized();
        }

        // Generate new token pair
        var (newAccessToken, newRefreshToken) = await GenerateTokenPair(user, refreshTokenRepository, configuration, httpContext);

        // Revoke old refresh token (token rotation)
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString();
        await refreshTokenRepository.RevokeAndReplaceAsync(
            request.RefreshToken,
            DateTime.UtcNow,
            clientIp,
            "Replaced by new token",
            newRefreshToken
        );

        return Results.Ok(new AuthResponse(
            newAccessToken,
            newRefreshToken,
            new UserInfo(user.Id, user.Phone, user.NickName, user.Avatar, user.Role)
        ));
    }

    private static async Task<IResult> Logout(
        ClaimsPrincipal userPrincipal,
        IRefreshTokenRepository refreshTokenRepository)
    {
        var userIdClaim = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        // Revoke all active refresh tokens for the user
        await refreshTokenRepository.RevokeAllByUserIdAsync(userId, DateTime.UtcNow);

        return Results.Ok(new MessageResponse("Logged out successfully"));
    }

    private static async Task<(string AccessToken, string RefreshToken)> GenerateTokenPair(
        User user,
        IRefreshTokenRepository refreshTokenRepository,
        IConfiguration configuration,
        HttpContext httpContext)
    {
        // Generate access token (short-lived: 15 minutes)
        var accessToken = GenerateJwtToken(user, configuration, TimeSpan.FromMinutes(15));

        // Generate refresh token (long-lived: 7 days)
        var refreshTokenValue = GenerateRefreshTokenValue();
        var refreshTokenEntity = new RefreshToken
        {
            Id = YitIdHelper.NextId(),
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = httpContext.Connection.RemoteIpAddress?.ToString()
        };

        await refreshTokenRepository.CreateAsync(refreshTokenEntity);

        return (accessToken, refreshTokenValue);
    }

    private static string GenerateJwtToken(User user, IConfiguration configuration, TimeSpan? expiration = null)
    {
        var securityKey = GetOrCreateSecurityKey(configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey is not configured"));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Phone),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("NickName", user.NickName ?? string.Empty)
        };

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"] ?? "CatCat.API",
            audience: configuration["Jwt:Audience"] ?? "CatCat.Web",
            claims: claims,
            expires: DateTime.UtcNow.Add(expiration ?? TimeSpan.FromHours(24)),
            signingCredentials: credentials
        );

        return _jwtTokenHandler.WriteToken(token);
    }

    private static string GenerateRefreshTokenValue()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private static string HashPassword(string password, IConfiguration configuration)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password + configuration["Security:PasswordSalt"]);
        return Convert.ToBase64String(sha256.ComputeHash(bytes));
    }

    private static bool VerifyPassword(string password, string hash, IConfiguration configuration)
    {
        return HashPassword(password, configuration) == hash;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Security key is always a string")]
    private static SymmetricSecurityKey GetOrCreateSecurityKey(string key)
    {
        return _keyCache.GetOrAdd(key, k => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(k)));
    }
}
