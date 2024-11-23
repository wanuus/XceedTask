using ApplicationLayer.Dto.AuthDto;

namespace ApplicationLayer.Services.Interface
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto model);
        Task<AuthResponseDto> LoginAsync(LoginDto model);
    }
}