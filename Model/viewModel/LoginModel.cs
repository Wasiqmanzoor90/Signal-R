
using System.ComponentModel.DataAnnotations;

namespace MyApiProject.Model.ViewModel
{

    public class LoginModel()
    {

        [EmailAddress]
        public required string Email { get; set; }
            
        public required string Password{ get; set; }
    }
}