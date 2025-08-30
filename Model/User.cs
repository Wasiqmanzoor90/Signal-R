using System.ComponentModel.DataAnnotations;

namespace MyApiProject.Model
{
    public class User
    {

         [Key]
        public Guid UserId { get; set; }
        [EmailAddress]
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }

     }
}