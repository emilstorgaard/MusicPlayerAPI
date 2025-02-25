using Microsoft.IdentityModel.Tokens;
using MusicPlayerAPI.Helpers;
using MusicPlayerAPI.Models.Dtos.Response;
using MusicPlayerAPI.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MusicPlayerAPI.Services;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly string _jwtSecret;
    private readonly int _jwtExpiryHours;

    public AuthService(IUserService userService, IConfiguration configuration)
    {
        _userService = userService;
        _jwtSecret = configuration["JwtSettings:Secret"] ?? throw new ArgumentNullException(nameof(configuration), "JWT Secret is missing");
        _jwtExpiryHours = int.TryParse(configuration["JwtSettings:ExpiryHours"], out var expiry) ? expiry : throw new ArgumentNullException(nameof(configuration), "JWT ExpiryHours is missing");
    }

    public async Task<StatusResult<TokenRespDto>> Login(string email, string password)
    {
        var user = await _userService.GetUserByEmailAsync(email);
        if (user == null || !PasswordHelper.VerifyPassword(password, user.PasswordHash))
            return StatusResult<TokenRespDto>.Failure(401, "Invalid email or password.");

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim("Uid", user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            }),
                Expires = DateTime.UtcNow.AddHours(_jwtExpiryHours),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            var tokenRespDto = new TokenRespDto
            {
                Token = tokenString
            };

            return StatusResult<TokenRespDto>.Success(tokenRespDto, 200);
        }
        catch (Exception ex)
        {
            return StatusResult<TokenRespDto>.Failure(500, "An error occurred while generating the token.");
        }
    }
}
