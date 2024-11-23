namespace ApplicationLayer.Dto.AuthDto
{
    public class AuthResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
    }
}