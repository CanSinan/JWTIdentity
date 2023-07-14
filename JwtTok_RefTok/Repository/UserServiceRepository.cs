using JwtTok_RefTok.DbContexts;
using JwtTok_RefTok.Models;
using JwtTok_RefTok.Repository.IRepository;
using Microsoft.AspNetCore.Identity;

namespace JwtTok_RefTok.Repository
{
    public class UserServiceRepository : IUserServiceRepository
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _db;

        public UserServiceRepository(UserManager<IdentityUser> userManager, AppDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public UserRefreshTokens AddUserRefreshTokens(UserRefreshTokens user)
        {
            _db.UserRefreshToken.Add(user);
            return user;
        }

        public void DeleteUserRefreshTokens(string username, string refreshToken)
        {
            var item = _db.UserRefreshToken.FirstOrDefault(x => x.UserName == username && x.RefreshToken == refreshToken);
            if (item != null)
            {
                _db.UserRefreshToken.Remove(item);
            }
        }

        public UserRefreshTokens GetSavedRefreshTokens(string username, string refreshToken)
        {
            return _db.UserRefreshToken.FirstOrDefault(x => x.UserName == username && x.RefreshToken == refreshToken && x.IsActive == true);
        }

        public int SaveCommit()
        {
            return _db.SaveChanges();
        }

        public async Task<bool> IsValidUserAsync(LoginViewModel users)
        {
            var u = _userManager.Users.FirstOrDefault(o => o.UserName == users.UserName);
            var result = await _userManager.CheckPasswordAsync(u, users.Password);
            return result;

        }
    }
}
