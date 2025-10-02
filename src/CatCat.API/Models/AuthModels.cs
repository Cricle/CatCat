namespace CatCat.API.Models;

public record SendCodeRequest(string Phone);

public record RegisterRequest(string Phone, string Code, string Password, string? NickName);

public record LoginRequest(string Phone, string Password);

