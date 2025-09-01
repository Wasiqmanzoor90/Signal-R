using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApiProject.Model
{
    public class Message
    {
        [Key]
        public Guid MessageId { get; set; }
        public required string Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // FK to User (sender)
        public Guid SenderId { get; set; }
        public User? Sender { get; set; }

        // FK to User (receiver)
        public Guid ReceiverId { get; set; }
        public User? Receiver { get; set; }
    }
}