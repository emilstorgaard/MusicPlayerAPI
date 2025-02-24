using Microsoft.IdentityModel.Tokens;
using MusicPlayerAPI.Models.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace MusicPlayerAPI.Services;

public class AuthService
{
    private readonly UserService _userService;
    private readonly string _jwtSecret;

    public AuthService(UserService userService, IConfiguration configuration)
    {
        _userService = userService;
        _jwtSecret = configuration["JwtSettings:Secret"] ?? throw new ArgumentNullException(nameof(configuration), "JWT Secret is missing");
    }

    public async Task<StatusResult<TokenRespDto>> Login(string email, string password)
    {
        var user = await _userService.GetUserByEmailAsync(email);
        if (user == null || !VerifyPassword(password, user.PasswordHash)) return StatusResult<TokenRespDto>.Failure(401, "Invalid email or password.");

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("Uid", user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
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

    private bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
