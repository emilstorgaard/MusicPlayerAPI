using MusicPlayerAPI.Entities;

namespace MusicPlayerAPI.Services.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
