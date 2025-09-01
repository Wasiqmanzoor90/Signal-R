using System.ComponentModel.DataAnnotations;

namespace MyApiProject.Model
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }

        // Navigation properties for sent and received messages
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    }
}