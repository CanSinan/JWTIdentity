namespace JwtTok_RefTok.Models.Dto
{
    public class ResponseDto
    {
        public bool IsSuccess { get; set; } = true;
        public object Data { get; set; }
        public string DisplayMessage { get; set; } = "";
        public string ErrorMessage { get; set; } = null;
    }
}
