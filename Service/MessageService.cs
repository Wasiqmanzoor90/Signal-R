using Microsoft.EntityFrameworkCore;
using MyApiProject.Model;


namespace MyApiProject.Service
{
    public class MessageService(SqlDbContext dbContext)
    {
        private readonly SqlDbContext _dbContext = dbContext;

        public async Task<Message> SendMessageAsync(Guid senderId, Guid receiverId, string content)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Message content cannot be empty");
            }

            if (senderId == receiverId)
            {
                throw new ArgumentException("Cannot send message to yourself");
            }

            // Validate that both users exist
            var usersExist = await _dbContext.Users
                .Where(u => u.UserId == senderId || u.UserId == receiverId)
                .CountAsync();

            if (usersExist != 2)
            {
                var senderExists = await _dbContext.Users.AnyAsync(u => u.UserId == senderId);
                var receiverExists = await _dbContext.Users.AnyAsync(u => u.UserId == receiverId);

                if (!senderExists)
                    throw new ArgumentException($"Sender with ID {senderId} not found");
                if (!receiverExists)
                    throw new ArgumentException($"Receiver with ID {receiverId} not found");
            }

            var message = new Message
            {
                MessageId = Guid.NewGuid(),
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content.Trim(),
                Timestamp = DateTime.UtcNow
            };

            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();
            
            return message;
        }

        public async Task<List<MessageDto>> GetConversationAsync(Guid userId1, Guid userId2, int page = 1, int pageSize = 50)
        {
            // Validate page parameters
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            var messages = await _dbContext.Messages
                .Where(m => (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                           (m.SenderId == userId2 && m.ReceiverId == userId1))
                .Include(m => m.Sender)
                .OrderByDescending(m => m.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MessageDto
                {
                    MessageId = m.MessageId,
                    Content = m.Content,
                    Timestamp = m.Timestamp,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    SenderName = m.Sender != null ? m.Sender.Name : "Unknown"
                })
                .ToListAsync();
            
            // Return in chronological order (oldest first)
            return messages.OrderBy(m => m.Timestamp).ToList();
        }

        public async Task<List<ConversationSummaryDto>> GetUserConversationsAsync(Guid userId)
        {
            // Get the latest message for each unique conversation partner
            var conversations = await _dbContext.Messages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g => new
                {
                    OtherUserId = g.Key,
                    LastMessage = g.OrderByDescending(m => m.Timestamp).First()
                })
                .ToListAsync();

            var result = new List<ConversationSummaryDto>();

            foreach (var conv in conversations)
            {
                var otherUser = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.UserId == conv.OtherUserId);

                if (otherUser != null)
                {
                    // Count unread messages (messages sent to current user that don't have a read timestamp)
                    var unreadCount = await _dbContext.Messages
                        .CountAsync(m => m.SenderId == conv.OtherUserId && 
                                        m.ReceiverId == userId);
                    // Note: You'd need to add IsRead or ReadAt property to Message model for proper unread tracking

                    result.Add(new ConversationSummaryDto
                    {
                        OtherUserId = conv.OtherUserId,
                        OtherUserName = otherUser.Name,
                        LastMessage = conv.LastMessage.Content,
                        LastMessageTime = conv.LastMessage.Timestamp,
                        UnreadCount = unreadCount // This is simplified - you might want proper read tracking
                    });
                }
            }

            return result.OrderByDescending(c => c.LastMessageTime).ToList();
        }

        public async Task<List<MessageDto>> GetUserMessagesAsync(Guid userId, int page = 1, int pageSize = 50)
        {
            // Get all messages sent to or from a user
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            var messages = await _dbContext.Messages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderByDescending(m => m.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MessageDto
                {
                    MessageId = m.MessageId,
                    Content = m.Content,
                    Timestamp = m.Timestamp,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    SenderName = m.Sender != null ? m.Sender.Name : "Unknown"
                })
                .ToListAsync();

            return messages;
        }

        public async Task<bool> DeleteMessageAsync(Guid messageId, Guid userId)
        {
            var message = await _dbContext.Messages
                .FirstOrDefaultAsync(m => m.MessageId == messageId && m.SenderId == userId);

            if (message == null)
            {
                return false; // Message not found or user doesn't own it
            }

            _dbContext.Messages.Remove(message);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<Message?> GetMessageByIdAsync(Guid messageId, Guid userId)
        {
            // Only return message if user is sender or receiver
            return await _dbContext.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .FirstOrDefaultAsync(m => m.MessageId == messageId && 
                                        (m.SenderId == userId || m.ReceiverId == userId));
        }

        public async Task MarkMessagesAsReadAsync(Guid currentUserId, Guid otherUserId)
        {
            // This is a placeholder implementation since your Message model doesn't have IsRead property
            // You would need to add IsRead or ReadAt properties to your Message model to implement this properly
            
            // For now, this method exists to prevent compilation errors
            // To implement properly, you would:
            // 1. Add IsRead (bool) or ReadAt (DateTime?) property to Message model
            // 2. Update unread messages from otherUserId to currentUserId
            
            /*
            Example implementation if you add IsRead property:
            
            var unreadMessages = await _dbContext.Messages
                .Where(m => m.SenderId == otherUserId && 
                           m.ReceiverId == currentUserId && 
                           !m.IsRead)
                .ToListAsync();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }

            await _dbContext.SaveChangesAsync();
            */
            
            await Task.CompletedTask; // Placeholder to make method async
        }

        private Guid? GetCurrentUserId()
        {
            // This would typically be injected via IHttpContextAccessor
            // For now, this is handled in the controller
            return null;
        }
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