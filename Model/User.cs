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

          // A user can join many chatrooms
        public ICollection<ChatRoom> ChatRooms { get; set; } = new List<ChatRoom>();

            // A user can send many messages
        public ICollection<Message> Messages { get; set; } = new List<Message>();

     }
}