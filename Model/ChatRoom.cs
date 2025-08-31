using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApiProject.Model
{
    public class ChatRoom()
    {
        [Key]
        public Guid ChatId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Messages in the chatroom
        public ICollection<Message> Messages { get; set; } = new List<Message>();

        // Users in the chatroom (Many-to-Many)
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}