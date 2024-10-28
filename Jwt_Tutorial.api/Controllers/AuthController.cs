using Jwt_Tutorial.api.Models;
using Jwt_Tutorial.api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;



namespace Jwt_Tutorial.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtApiContext _dbcontext;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailservice;

        public AuthController(JwtApiContext dbcontext, IConfiguration config, IEmailService emailService)
        {
            _dbcontext = dbcontext;
            _config = config;
            _emailservice = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationModel Data)
        {
            if(await _dbcontext.Users.AnyAsync(e=>e.Email == Data.Email))
            {
                return BadRequest("user with the Email Already Exists!");
            }

            if(Data.Password != Data.ConfirmPassword)
            {
                return BadRequest("Passwords did not match");
            }
            var HashPass = BCrypt.Net.BCrypt.HashPassword(Data.Password);

            var NewUser = new User
            {
                Name = Data.Name,
                Email = Data.Email,
                Password = HashPass
            };

            await _dbcontext.Users.AddAsync(NewUser);
            await _dbcontext.SaveChangesAsync();

            return Created("user created", NewUser);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel Data)
        {
            var user = await _dbcontext.Users.FirstOrDefaultAsync(e => Data.Email == e.Email);
            if (user == null)
            {
                return NotFound("user not found");
            }
            if(!BCrypt.Net.BCrypt.Verify(Data.Password, user.Password))
            {
                return BadRequest("Invalid Password");
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, _config["Jwt:Subject"]),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("UserId", user.Id.ToString()),
                new Claim("Email", user.Email),
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddSeconds(60),
                signingCredentials: signIn
                );
            string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(new
            {
                Token = tokenValue,
                User = user
            });

        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel data)
        {
            var user = await _dbcontext.Users.FirstOrDefaultAsync(e => e.Email == data.email);
            if(user == null)
            {
                return BadRequest("The user does not exists");
            }
            var newPass = HelperFunctions.CreatePassword(12);

            var HashNewPass = BCrypt.Net.BCrypt.HashPassword(newPass);

            user.Password = HashNewPass;

            string UserEmail = user.Email;
            await _dbcontext.SaveChangesAsync();

            await _emailservice.SendEmail(UserEmail, "New Password for login", $"Your New Password is {newPass}");
            return Ok("New Password has been sent to your Email");
 
        }

    }

    public class HelperFunctions
    {
        public static string CreatePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890@#$%";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }


    }


    public class RegistrationModel
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ForgotPasswordModel
    {
        public string email { get; set; } = string.Empty;
    }

}
