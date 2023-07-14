namespace JwtTok_RefTok.Models
{
    public class AuthResponse
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string accessToken { get; set; } = null!;
        public string refreshToken { get; set; } = null!;
    }
}
