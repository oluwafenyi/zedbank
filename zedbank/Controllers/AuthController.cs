using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zedbank.Database;
using zedbank.Models;
using zedbank.Services;

namespace zedbank.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly Context _context;
        
        public AuthController(Context context)
        {
            _context = context;
        }

        [HttpPost, Route("token")]
        public async Task<ActionResult<Token>> GenerateAuthToken(UserAuthDto userAuthDto)
        {
            var user = await AuthService.Authenticate(userAuthDto, _context);
            if (user == null)
            {
                return Unauthorized(new { Detail = "no account found with the supplied credentials"});
            }

            var token = AuthService.GenerateToken(user);
            return Ok(token);
        }
        
        [Authorize]
        [HttpGet, Route("user")]
        public async Task<ActionResult<UserDisplayDto>> GetAuthUser()
        {
            var user = await AuthService.GetAuthUser(User, _context);
            return Ok(new UserDisplayDto(user));
        }
    }
}
