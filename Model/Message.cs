using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MyApiProject.Model
{
    public class Message()
    {
        [Key]
        public Guid MessageId { get; set; }
        public required string Content { get; set; }





        public DateTime Timestamp { get; set; } = DateTime.UtcNow;


//Fk to user(sender)
        public Guid SenderId { get; set; }
        [ForeignKey("UserId")]
        public User? Sender { get; set; }
        

         // FK to ChatRoom
        public Guid ChatRoomId { get; set; }
        [ForeignKey("ChatRoomId")]
        public ChatRoom? ChatRoom { get; set; }
     }
}
