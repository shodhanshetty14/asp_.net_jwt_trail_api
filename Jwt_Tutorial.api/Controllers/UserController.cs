using Jwt_Tutorial.api.Models;
using Jwt_Tutorial.api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        public readonly IEmailService _emailservice;
        public UserController(JwtApiContext dbcontext, IConfiguration config, IEmailService emailservice)
        {
            _dbcontext = dbcontext;
            _config = config;
            _emailservice = emailservice;
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

        [HttpPost("send-notification")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationModel data)
        {
            await _emailservice.SendEmail(data.reciever, data.subject, data.body);
            return Ok("message sent");
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel data)
        {
            var loggedUser = User.FindFirstValue("Email");
            int loggedId;
            int.TryParse(User.FindFirstValue("UserId"), out loggedId); 
            //Console.WriteLine(loggedUser + " "+ loggedId);
            var user = await _dbcontext.Users.FindAsync(loggedId);
            if(user == null)
            {
                return NotFound("User not found");
            }
            if (!BCrypt.Net.BCrypt.Verify(data.oldPassword, user.Password))
            {
                return BadRequest("Invalid Password");
            }
            if(data.newPassword != data.confirmNewPassword)
            {
                return BadRequest("The passwords do not match!");
            }
            
            var HashPass = BCrypt.Net.BCrypt.HashPassword(data.newPassword);
            user.Password = HashPass;
            await _dbcontext.SaveChangesAsync();
            return Ok("Password Changed Successfully!");
        }

    }

    public class NotificationModel
    {
        public string reciever { get; set; } = string.Empty;
        public string subject { get; set; } = string.Empty;
        public string body { get; set; } = string.Empty;
    }

    public class ChangePasswordModel
    {
        public string oldPassword { get; set; } = string.Empty;
        public string newPassword { get; set; } = string.Empty;
        public string confirmNewPassword { get; set; } = string.Empty;
    }

}
