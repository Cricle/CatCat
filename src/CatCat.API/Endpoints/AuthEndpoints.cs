using CatCat.API.Models;
using CatCat.Domain.Entities;
using CatCat.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ZiggyCreatures.Caching.Fusion;

namespace CatCat.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        // 发送验证码
        group.MapPost("/send-code", async (
            [FromBody] SendCodeRequest request,
            IFusionCache cache,
            ILogger<Program> logger) =>
        {
            // 生成6位验证码
            var code = Random.Shared.Next(100000, 999999).ToString();

            // 存储到缓存，5分钟过期
            await cache.SetAsync($"sms:code:{request.Phone}", code, TimeSpan.FromMinutes(5));

            // TODO: 调用短信服务发送验证码
            logger.LogInformation("Send verification code {Code} to {Phone}", code, request.Phone);

            return Results.Ok(new { message = "验证码已发送" });
        })
        .WithName("SendCode")
        .WithSummary("发送短信验证码");

        // 用户注册
        group.MapPost("/register", async (
            [FromBody] RegisterRequest request,
            IUserRepository userRepository,
            IFusionCache cache,
            IConfiguration configuration) =>
        {
            // 验证验证码
            var cachedCode = await cache.TryGetAsync<string>($"sms:code:{request.Phone}");
            if (!cachedCode.HasValue || cachedCode.Value != request.Code)
            {
                return Results.BadRequest(new { message = "验证码错误或已过期" });
            }

            // 检查手机号是否已注册
            var existingUser = await userRepository.GetByPhoneAsync(request.Phone);
            if (existingUser != null)
            {
                return Results.BadRequest(new { message = "该手机号已注册" });
            }

            // 创建用户
            var user = new User
            {
                Phone = request.Phone,
                NickName = request.NickName ?? $"用户{request.Phone[^4..]}",
                PasswordHash = HashPassword(request.Password, configuration),
                Role = UserRole.Customer,
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            var userId = await userRepository.CreateAsync(user);
            user.Id = userId;

            // 删除验证码
            await cache.RemoveAsync($"sms:code:{request.Phone}");

            // 生成JWT Token
            var token = GenerateJwtToken(user, configuration);

            return Results.Ok(new
            {
                token,
                user = new
                {
                    user.Id,
                    user.Phone,
                    user.NickName,
                    user.Avatar,
                    user.Role
                }
            });
        })
        .WithName("Register")
        .WithSummary("用户注册");

        // 用户登录
        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            IUserRepository userRepository,
            IConfiguration configuration) =>
        {
            var user = await userRepository.GetByPhoneAsync(request.Phone);
            if (user == null || !VerifyPassword(request.Password, user.PasswordHash, configuration))
            {
                return Results.BadRequest(new { message = "手机号或密码错误" });
            }

            if (user.Status != UserStatus.Active)
            {
                return Results.BadRequest(new { message = "账号已被禁用" });
            }

            var token = GenerateJwtToken(user, configuration);

            return Results.Ok(new
            {
                token,
                user = new
                {
                    user.Id,
                    user.Phone,
                    user.NickName,
                    user.Avatar,
                    user.Role
                }
            });
        })
        .WithName("Login")
        .WithSummary("用户登录");
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
        var secretKey = jwtSettings["SecretKey"]!;
        var issuer = jwtSettings["Issuer"]!;
        var audience = jwtSettings["Audience"]!;

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.MobilePhone, user.Phone),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

