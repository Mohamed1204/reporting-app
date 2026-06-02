using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReportingApi1.DTOs;
using ReportingApi1.Services;

namespace ReportingApi1.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private const string RefreshCookieName = "refreshToken";

        private readonly IAuthService _authService;
        private readonly IWebHostEnvironment _env;

        public AuthController(IAuthService authService, IWebHostEnvironment env)
        {
            _authService = authService;
            _env = env;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto loginUserDto)
        {
            var result = await _authService.LoginAsync(loginUserDto);
            if (result == null) return UnauthorizedProblem();

            SetRefreshCookie(result.RefreshToken, result.RefreshExpiresAt);
            return Ok(result.Response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUserDto)
        {
            await _authService.RegisterAsync(registerUserDto);
            return Created();
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var raw = Request.Cookies[RefreshCookieName];
            if (string.IsNullOrEmpty(raw)) return UnauthorizedProblem();

            var result = await _authService.RefreshAsync(raw);
            if (result == null)
            {
                ClearRefreshCookie();
                return UnauthorizedProblem();
            }

            SetRefreshCookie(result.RefreshToken, result.RefreshExpiresAt);
            return Ok(result.Response);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var raw = Request.Cookies[RefreshCookieName];
            if (!string.IsNullOrEmpty(raw))
            {
                await _authService.LogoutAsync(raw);
            }
            ClearRefreshCookie();
            return NoContent();
        }

        private void SetRefreshCookie(string token, DateTime expiresAt)
        {
            Response.Cookies.Append(RefreshCookieName, token, new CookieOptions
            {
                HttpOnly = true,
                Secure = !_env.IsDevelopment(),
                SameSite = SameSiteMode.Strict,
                Expires = expiresAt
            });
        }

        private void ClearRefreshCookie()
        {
            Response.Cookies.Delete(RefreshCookieName, new CookieOptions
            {
                HttpOnly = true,
                Secure = !_env.IsDevelopment(),
                SameSite = SameSiteMode.Strict
            });
        }

        private UnauthorizedObjectResult UnauthorizedProblem() => Unauthorized(new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = "Invalid credentials",
            Type = "https://httpstatuses.com/401"
        });
    }
}
