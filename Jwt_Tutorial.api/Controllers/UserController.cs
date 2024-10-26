using Jwt_Tutorial.api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Jwt_Tutorial.api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public readonly JwtApiContext _dbcontext;
        public readonly IConfiguration _config;
        public UserController(JwtApiContext dbcontext, IConfiguration config)
        {
            _dbcontext = dbcontext;
            _config = config;
        }


        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            return Ok(await _dbcontext.Users.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _dbcontext.Users.FindAsync(id);
            if(user == null)
            {
                return NotFound("User not found");
            }
            return Ok(user);
        }


    }
}
