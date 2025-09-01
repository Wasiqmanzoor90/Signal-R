using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MyApiProject.Model;
using MyApiProject.Service;
using System.Security.Claims;

namespace MyApiProject.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all endpoints
    public class MessageController(MessageService messageService) : ControllerBase
    {
        private readonly MessageService _messageService = messageService;

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] CreateMessageRequest request)
        {
            try
            {
                var senderId = GetCurrentUserId();
                if (senderId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var message = await _messageService.SendMessageAsync(
                    senderId.Value,
                    request.ReceiverId, 
                    request.Content
                );
                
                return Ok(new
                {
                    success = true,
                    message = new
                    {
                        messageId = message.MessageId,
                        senderId = message.SenderId,
                        receiverId = message.ReceiverId,
                        content = message.Content,
                        timestamp = message.Timestamp
                    }
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while sending the message" });
            }
        }

        [HttpGet("conversation/{otherUserId}")]
        public async Task<IActionResult> GetConversation(
            Guid otherUserId, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var messages = await _messageService.GetConversationAsync(
                    currentUserId.Value, 
                    otherUserId, 
                    page, 
                    pageSize
                );

                return Ok(new
                {
                    success = true,
                    messages = messages.Select(m => new
                    {
                        messageId = m.MessageId,
                        senderId = m.SenderId,
                        receiverId = m.ReceiverId,
                        content = m.Content,
                        timestamp = m.Timestamp,
                        isSentByMe = m.SenderId == currentUserId
                    }),
                    page,
                    pageSize
                });
            }
            catch (Exception )
            {
                return StatusCode(500, new { success = false, error = "An error occurred while retrieving messages" });
            }
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetUserConversations()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var conversations = await _messageService.GetUserConversationsAsync(currentUserId.Value);
                
                return Ok(new
                {
                    success = true,
                    conversations = conversations.Select(c => new
                    {
                        otherUserId = c.OtherUserId,
                        otherUserName = c.OtherUserName,
                        lastMessage = c.LastMessage,
                        lastMessageTime = c.LastMessageTime,
                        unreadCount = c.UnreadCount
                    })
                });
            }
            catch (Exception )
            {
                return StatusCode(500, new { success = false, error = "An error occurred while retrieving conversations" });
            }
        }

        [HttpPut("mark-read/{otherUserId}")]
        public async Task<IActionResult> MarkMessagesAsRead(Guid otherUserId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                await _messageService.MarkMessagesAsReadAsync(currentUserId.Value, otherUserId);
                
                return Ok(new { success = true, message = "Messages marked as read" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while marking messages as read" });
            }
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return null;
        }
    }

    // Updated request models
    public class CreateMessageRequest
    {
        public required Guid ReceiverId { get; set; }
        public required string Content { get; set; }
    }

    // Response models for service layer
    public class ConversationSummary
    {
        public Guid OtherUserId { get; set; }
        public string OtherUserName { get; set; } = string.Empty;
        public string? LastMessage { get; set; }
        public DateTime? LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
    }
}