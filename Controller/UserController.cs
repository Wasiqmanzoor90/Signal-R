using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApiProject.Inerface;
using MyApiProject.Model;
using MyApiProject.Model.ViewModel;
using MyApiProject.Service;

namespace MyApiProject.Controller
{
    public class UserController(SqlDbContext dbContext, IJsonToken tokenService) : ControllerBase
    {
        private readonly SqlDbContext _dbcontext = dbContext;
        private readonly IJsonToken _tokenService = tokenService;

      [HttpPost("register")]
public async Task<IActionResult> Register([FromBody] User user)
{

if (string.IsNullOrWhiteSpace(user.Email))
    return BadRequest("Email is required");
if (string.IsNullOrWhiteSpace(user.Password))
    return BadRequest("Password is required");


            try
            {
                // if(string.IsNullOrWhiteSpace(user.Name)|| string.IsNullOrWhiteSpace(user.Email))
                var findUser = await _dbcontext.Users.FirstOrDefaultAsync(e => e.Email == user.Email);
                if (findUser != null)
                {
                    return BadRequest("User already exists!");
                }
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

                await _dbcontext.Users.AddAsync(user);
                await _dbcontext.SaveChangesAsync();
                return Ok(new { message = "User created sucessfullt" });


            }
            catch (Exception ex)
            {

                throw new Exception("Server error: " + ex.Message);
            }

        }



        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]LoginModel login)
        {
            try
            {
                var findUser = await _dbcontext.Users.FirstOrDefaultAsync(e => e.Email == login.Email);
                if (findUser == null)
                {
                    return BadRequest("User doesnt exists");
                }
                bool verify = BCrypt.Net.BCrypt.Verify(login.Password, findUser.Password);
                if (!verify)
                {
                    return BadRequest("Invalid Email or password!");
                }

                var token = _tokenService.CreateToken(findUser.UserId, findUser.Name, findUser.Email);
                HttpContext.Response.Cookies.Append("token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,           // false for local HTTP
                    SameSite = SameSiteMode.None,  // allow cross-origin (Postman)
                    Expires = DateTime.UtcNow.AddHours(10)
                });
                return Ok(new { message = "Login successful", token });


            }
            catch (Exception ex)
            {

                throw new Exception("Server error: " + ex.Message);
            }
        }


    }
}