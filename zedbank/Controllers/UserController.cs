using System.Net;
using Microsoft.AspNetCore.Mvc;
using zedbank.Database;
using zedbank.Exceptions;
using zedbank.Models;
using zedbank.Services;
using zedbank.Validators;

namespace zedbank.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly Context _context;
        private readonly ILogger _logger;

        public UserController(Context context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: /User
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserDisplayDto>> PostUser(UserRegistrationDto userRegistrationDto)
        {
            var validator = new UserRegistrationValidator(_context);
            var result = await validator.ValidateAsync(userRegistrationDto);

            if (!result.IsValid)
            {
                return BadRequest(result.Errors);
            }

            try
            {
                var user = await UserService.RegisterUser(userRegistrationDto, _context);
                return StatusCode(201, new UserDisplayDto(user));
            }
            catch (UserRegistrationException e)
            {
                _logger.LogError("error occurred while registering user: {e}", e.ToString());
            }
            return Problem("an unexpected error occurred");
        }
    }
}