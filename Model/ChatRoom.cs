using System.ComponentModel.DataAnnotations;

namespace MyApiProject.Model
{
    public class ChatRoom
    {
        [Key]
        public Guid ChatId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}