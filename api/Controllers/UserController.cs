using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Model;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System.Text.RegularExpressions;

namespace api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly TodosAppContext _context;
        private readonly IConfiguration _configuration;
        public UserController(TodosAppContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        // GET: /user/{username}
        [HttpGet("{username}")]
        public async Task<ActionResult<IEnumerable<Todo>>> GetTodos([FromRoute] string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
            if (user == null)
            {
                return NotFound();
            }
            return Ok();
        }


        [HttpPost("login")]
        public async Task<ActionResult<Todo>> Login([FromBody] UserDto userDto)
        { 
            var areCredentialsValid = ValidateUsername(userDto.Username) && ValidatePassword(userDto.Password);
            if (!areCredentialsValid)
            {
                return StatusCode(412, "Precondition Failed");
            }
            Console.WriteLine(userDto.Username + " " + userDto.Password);
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == userDto.Username);
            if (user == null)
            {
                return NotFound();
            }
            if (user.Password == userDto.Password)
            {
                // JWT
                var token = CreateToken(user);
                var response = new
                {
                    token = token,
                    username = user.Username,
                    userId = user.Id
                };
                return Ok(response);
            } 
            else
            {
                return BadRequest();
            }
        }


        [HttpPost("register")]
        public async Task<ActionResult<Todo>> Register([FromBody] UserDto userDto)
        {
            // WALIDACJA DANYCH

            var areCredentialsValid = ValidateUsername(userDto.Username) && ValidatePassword(userDto.Password);
            if (!areCredentialsValid)
            {
                return StatusCode(412, "Precondition Failed");
            }

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == userDto.Username);
            if (user != null)
            {
                return BadRequest();
            }

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = userDto.Username,
                Password = userDto.Password
            };

            await _context.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("JwtSettings:Key").Value!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(15),
                audience: _configuration.GetSection("JwtSettings:Audience").Value!,
                issuer: _configuration.GetSection("JwtSettings:Issuer").Value!,
                signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private bool ValidateUsername(string username)
        {
            string pattern = @"^[A-Za-z0-9]{3,32}$";
            Regex regex = new Regex(pattern);

            return regex.IsMatch(username);
        }

        private bool ValidatePassword(string password)
        {
            string pattern = @"^.{8,32}$";
            Regex regex = new Regex(pattern);

            return regex.IsMatch(password);
        }
    }
}
