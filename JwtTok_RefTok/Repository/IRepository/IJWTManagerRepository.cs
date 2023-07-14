using JwtTok_RefTok.Models;
using System.Security.Claims;

namespace JwtTok_RefTok.Repository.IRepository
{
    public interface IJWTManagerRepository
    {
        Tokens GenerateToken(string userName);
        Tokens GenerateRefreshToken(string userName);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
