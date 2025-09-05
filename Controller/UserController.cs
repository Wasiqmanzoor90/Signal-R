using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApiProject.Inerface;
using MyApiProject.Model;
using MyApiProject.Model.ViewModel;
using MyApiProject.Service;

namespace MyApiProject.Controller
{
    [ApiController]
    [Route("api/[controller]")]
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
        public async Task<IActionResult> Login([FromBody] LoginModel login)
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
                    SameSite = SameSiteMode.Lax,  // allow cross-origin (Postman)
                    Expires = DateTime.UtcNow.AddHours(10)
                });
                return Ok(new { message = "Login successful", token });


            }
            catch (Exception ex)
            {

                throw new Exception("Server error: " + ex.Message);
            }
        }

[HttpGet("AllUsers")]
[Authorize] // Make sure user is authenticated
public async Task<IActionResult> GetOnlineUsers()
{
    try
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
        {
            return Unauthorized("User not authenticated");
        }

        // Get all users except the current user
        var users = await _dbcontext.Users
            .Where(u => u.UserId != currentUserId.Value)
            .Select(u => new
            {
                id = u.UserId,
                name = u.Name,
                email = u.Email,
                avatar = u.Name.Substring(0, Math.Min(2, u.Name.Length)).ToUpper(),
                status = "online", // You can implement actual online status tracking
                lastSeen = "Online",
                lastMessage = "",
                timestamp = DateTime.Now.ToString("h:mm tt"),
                unreadCount = 0
            })
            .ToListAsync();

        return Ok(new { success = true, users = users });
    }
    catch (Exception)
    {
        return StatusCode(500, new { success = false, error = "An error occurred while fetching users" });
    }
}

private Guid? GetCurrentUserId()
{
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (Guid.TryParse(userIdClaim, out var userId))
    {
        return userId;
    }
    return null;
}

    }
}