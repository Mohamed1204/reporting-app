using Microsoft.AspNetCore.Mvc;
using ReportingApi1.DTOs;
using ReportingApi1.Services;
namespace ReportingApi1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto loginUserDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _authService.LoginAsync(loginUserDto);
            if (user == null) return Unauthorized();

            // TODO: return token instead of user info in production
            return Ok(new { user.Id, user.UserName, user.CompanyId });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUserDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _authService.RegisterAsync(registerUserDto);
            if (!created) return BadRequest("Unable to register user (username taken or invalid company)");

            return Ok();
        }
    }
}
