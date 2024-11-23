using ApplicationLayer.Dto.AuthDto;
using ApplicationLayer.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Task.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto model)
        {
            var result = await _authService.RegisterAsync(model);
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto model)
        {
            var result = await _authService.LoginAsync(model);
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
    }
}