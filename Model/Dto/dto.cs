  // Updated request models
   namespace MyApiProject.Model.Dto
   {
    

    // Response models for service layer
    public class ConversationSummary
    {
        public Guid OtherUserId { get; set; }
        public string OtherUserName { get; set; } = string.Empty;
        public string? LastMessage { get; set; }
        public DateTime? LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
    }


     // DTOs for clean API responses
    public class MessageDto
    {
        public Guid MessageId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string SenderName { get; set; } = string.Empty;
    }

    public class ConversationSummaryDto
    {
        public Guid OtherUserId { get; set; }
        public string OtherUserName { get; set; } = string.Empty;
        public string? LastMessage { get; set; }
        public DateTime? LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
    }

    public class CreateMessageRequest
    {
        public required Guid ReceiverId { get; set; }
        public required string Content { get; set; }
    }
    }
