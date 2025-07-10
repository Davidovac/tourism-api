using Microsoft.AspNetCore.Mvc;
using tourism_api.Domain;
using tourism_api.Repositories;

namespace tourism_api.Controllers;

[Route("api/users")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly UserRepository _userRepo;

    public UserController(IConfiguration configuration)
    {
        _userRepo = new UserRepository(configuration);
    }

    [HttpPost("login")]
    public ActionResult<User> Login([FromBody] User credentials)
    {
        if (!credentials.IsValid())
        {
            return BadRequest("Invalid data.");
        }

        try
        {
            User user = _userRepo.Get(credentials.Username, credentials.Password);
            if (user == null)
            {
                return NotFound("Invalid username or password.");
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            return Problem("An unexpected error occurred while processing login request.");
        }
    }
}
