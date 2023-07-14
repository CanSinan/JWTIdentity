using JwtTok_RefTok.DbContexts;
using JwtTok_RefTok.Models;
using JwtTok_RefTok.Models.Dto;
using JwtTok_RefTok.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JwtTok_RefTok.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IJWTManagerRepository jWTManager;
        private readonly IUserServiceRepository userServiceRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _context;
        public UsersController(IJWTManagerRepository jWTManager, IUserServiceRepository userServiceRepository, UserManager<IdentityUser> userManager, AppDbContext context)
        {
            this.jWTManager = jWTManager;
            _userManager = userManager;
            _context = context;
            this.userServiceRepository = userServiceRepository;
        }

        [HttpGet]
        public List<string> Get()
        {
            var users = new List<string>
            {
                "Sinan Can",
            };

            return users;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (_context.Users.Any(x => x.UserName.ToLower() == model.UserName.ToLower())) // aynı kullanıcı adıyla kayıtı engeller.
            {
                ModelState.AddModelError(nameof(model.UserName), "Username kullanılmaktadır.");
                return BadRequest(ModelState); // swaggerda 400 hata kodu gösterdik.
            }
            var result = await _userManager.CreateAsync(
                new IdentityUser
                {
                    UserName = model.UserName,
                    Email = model.Mail
                },
                model.Password
            );
            if (result.Succeeded)
            {
                model.Password = "";
                return CreatedAtAction(nameof(Register), new { email = model.Mail }, model);
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
            return BadRequest(ModelState);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("authenticate")]
        public async Task<IActionResult> AuthenticateAsync(LoginViewModel usersdata)
        {
            var validUser = await userServiceRepository.IsValidUserAsync(usersdata);

            if (!validUser)
            {
                return Unauthorized("Yanlış kullanıcı adı veya parola!");
            }
            
            var accessToken = jWTManager.GenerateToken(usersdata.UserName);

            if (accessToken == null)
            {
                return Unauthorized("Geçersiz giriş!");
            }
            var userInDb = _context.Users.FirstOrDefault(u => u.UserName == usersdata.UserName);
            if (userInDb is null)
                return Unauthorized();
            // saving refresh token to the db
            UserRefreshTokens obj = new UserRefreshTokens
            {
                RefreshToken = accessToken.Refresh_Token,
                UserName = usersdata.UserName
            };

            userServiceRepository.AddUserRefreshTokens(obj);
            userServiceRepository.SaveCommit();
            var responsedto = new ResponseDto
            {
                Data = new AuthResponse
                {
                    Username = usersdata.UserName,
                    Email = userInDb.Email,
                    accessToken = accessToken.Access_Token,
                    refreshToken = accessToken.Refresh_Token
                },
                IsSuccess = true,
                DisplayMessage = "Success"
            };
            return Ok(responsedto);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("refresh")]
        public IActionResult Refresh(Tokens token)
        {
            var principal = jWTManager.GetPrincipalFromExpiredToken(token.Access_Token);
            var username = principal.Identity?.Name;

            //retrieve the saved refresh token from database
            var savedRefreshToken = userServiceRepository.GetSavedRefreshTokens(username, token.Refresh_Token);

            if (savedRefreshToken.RefreshToken != token.Refresh_Token)
            {
                return Unauthorized("Geçersiz Token!");
            }

            var newJwtToken = jWTManager.GenerateRefreshToken(username);

            if (newJwtToken == null)
            {
                return Unauthorized("Geçersiz Token!");
            }

            // saving refresh token to the db
            UserRefreshTokens obj = new UserRefreshTokens
            {
                RefreshToken = newJwtToken.Refresh_Token,
                UserName = username
            };

            userServiceRepository.DeleteUserRefreshTokens(username, token.Refresh_Token);
            userServiceRepository.AddUserRefreshTokens(obj);
            userServiceRepository.SaveCommit();

            return Ok(newJwtToken);
        }
        [HttpPost]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                // Kullanıcının refresh tokenlarını veritabanından silin
                var tokens = new Tokens();
                userServiceRepository.DeleteUserRefreshTokens(user.UserName, tokens.Refresh_Token);
                userServiceRepository.SaveCommit();
            }

            return Ok("Çıkış Yapıldı");
        }
    }
}
