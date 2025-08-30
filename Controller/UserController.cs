using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApiProject.Model;
using MyApiProject.Service;

namespace MyApiProject.Controller
{
    public class UserController(SqlDbContext dbContext) : ControllerBase
    {
        private readonly SqlDbContext _dbcontext = dbContext;


        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
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
            catch (Exception)
            {

                return NotFound(new { message = "Server error" });
            }

        }


     }
}