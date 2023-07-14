using JwtTok_RefTok.Models;

namespace JwtTok_RefTok.Repository.IRepository
{
    public interface IUserServiceRepository
    {
        Task<bool> IsValidUserAsync(LoginViewModel users);

        UserRefreshTokens AddUserRefreshTokens(UserRefreshTokens user);

        UserRefreshTokens GetSavedRefreshTokens(string username, string refreshtoken);

        void DeleteUserRefreshTokens(string username, string refreshToken);

        int SaveCommit();
    }
}
