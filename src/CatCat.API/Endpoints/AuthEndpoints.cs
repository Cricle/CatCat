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
using ZiggyCreatures.Caching.Fusion;

namespace CatCat.API.Endpoints;

public static class AuthEndpoints
{
    private static readonly JwtSecurityTokenHandler _jwtTokenHandler = new();
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, SymmetricSecurityKey> _keyCache = new();

    [RequiresUnreferencedCode("Uses JSON serialization which may require unreferenced code")]
    [RequiresDynamicCode("Uses JSON serialization which may require dynamic code")]
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("/send-code", async (
            [FromBody] SendCodeRequest request,
            IFusionCache cache,
            ILogger<Program> logger) =>
        {
            var code = Random.Shared.Next(100000, 999999).ToString();
            await cache.SetAsync($"sms:code:{request.Phone}", code, TimeSpan.FromMinutes(5));

            // TODO: Integrate SMS service (e.g., Twilio, Aliyun SMS)
            logger.LogInformation("SMS code {Code} for {Phone}", code, request.Phone);

            return Results.Ok(new MessageResponse("验证码已发送"));
        })
        .WithName("SendCode")
        .WithSummary("Send SMS verification code");

        group.MapPost("/register", async (
            [FromBody] RegisterRequest request,
            IUserRepository userRepository,
            IFusionCache cache,
            IConfiguration configuration) =>
        {
            var cachedCode = await cache.TryGetAsync<string>($"sms:code:{request.Phone}");
            if (!cachedCode.HasValue || cachedCode.Value != request.Code)
            {
                return Results.BadRequest(new MessageResponse("验证码错误或已过期"));
            }

            var existingUser = await userRepository.GetByPhoneAsync(request.Phone);
            if (existingUser != null)
            {
                return Results.BadRequest(new MessageResponse("该手机号已注册"));
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

            var token = GenerateJwtToken(user, configuration);

            return Results.Ok(new AuthResponse(
                token,
                new UserInfo(user.Id, user.Phone, user.NickName, user.Avatar, user.Role)
            ));
        })
        .WithName("Register")
        .WithSummary("User registration");
        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            IUserRepository userRepository,
            IConfiguration configuration) =>
        {
            var user = await userRepository.GetByPhoneAsync(request.Phone);
            if (user == null || !VerifyPassword(request.Password, user.PasswordHash, configuration))
            {
                return Results.BadRequest(new MessageResponse("手机号或密码错误"));
            }

            if (user.Status != UserStatus.Active)
            {
                return Results.BadRequest(new MessageResponse("账号已被禁用"));
            }

            var token = GenerateJwtToken(user, configuration);

            return Results.Ok(new AuthResponse(
                token,
                new UserInfo(user.Id, user.Phone, user.NickName, user.Avatar, user.Role)
            ));
        })
        .WithName("Login")
        .WithSummary("User login");
    }

    private static string HashPassword(string password, IConfiguration configuration)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password + configuration["Security:PasswordSalt"]);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private static bool VerifyPassword(string password, string hash, IConfiguration configuration)
    {
        return HashPassword(password, configuration) == hash;
    }

    private static string GenerateJwtToken(User user, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.MobilePhone, user.Phone),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = _keyCache.GetOrAdd(jwtSettings["SecretKey"]!, k => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(k)));
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return _jwtTokenHandler.WriteToken(token);
    }
}

