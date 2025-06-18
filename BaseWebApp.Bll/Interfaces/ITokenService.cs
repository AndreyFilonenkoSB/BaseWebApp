using BaseWebApp.Dal.Entities;

namespace BaseWebApp.Bll.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}