using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using MyApiProject.Model;

using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Security.Claims;

[Authorize] // Require authentication
public class ChatHub : Hub
{
    private readonly SqlDbContext _context;
    private static readonly ConcurrentDictionary<string, HashSet<string>> ConnectedUsers = new();
    public ChatHub(SqlDbContext context)
    {
        _context = context;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId == null)
        {
            Context.Abort();
            return;
        }

        ConnectedUsers.AddOrUpdate(
            userId,
            _ => new HashSet<string> { Context.ConnectionId },
            (_, connections) => { connections.Add(Context.ConnectionId); return connections; }
        );

        await Clients.Caller.SendAsync("Connected", $"You are connected as {userId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId != null && ConnectedUsers.TryGetValue(userId, out var connections))
        {
            connections.Remove(Context.ConnectionId);
            if (connections.Count == 0)
                ConnectedUsers.TryRemove(userId, out _);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string receiverUserId, string messageContent)
    {
        try
        {
            var senderUserId = GetUserId();
            if (senderUserId == null)
            {
                await Clients.Caller.SendAsync("Error", "Authentication required");
                return;
            }

            // Validate input
            if (string.IsNullOrWhiteSpace(receiverUserId) || string.IsNullOrWhiteSpace(messageContent))
            {
                await Clients.Caller.SendAsync("Error", "Invalid message data");
                return;
            }

            // Validate receiver exists
            if (!Guid.TryParse(receiverUserId, out var receiverGuid) || 
                !Guid.TryParse(senderUserId, out var senderGuid))
            {
                await Clients.Caller.SendAsync("Error", "Invalid user ID format");
                return;
            }

            var receiverExists = await _context.Users.AnyAsync(u => u.UserId == receiverGuid);
            if (!receiverExists)
            {
                await Clients.Caller.SendAsync("Error", "Receiver not found");
                return;
            }

            // Save message to database
            var message = new Message
            {
                MessageId = Guid.NewGuid(),
                Content = messageContent,
                SenderId = senderGuid,
                ReceiverId = receiverGuid,
                Timestamp = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Send to sender (confirmation)
            await Clients.Caller.SendAsync("MessageSent", new
            {
                messageId = message.MessageId,
                content = message.Content,
                receiverId = receiverUserId,
                timestamp = message.Timestamp
            });

            // Send to receiver if online
            if (ConnectedUsers.TryGetValue(receiverUserId, out var connections))
            {
                foreach (var connId in connections)
                {
                    await Clients.Client(connId).SendAsync("ReceivePrivateMessage", new
                    {
                        messageId = message.MessageId,
                        senderId = senderUserId,
                        content = message.Content,
                        timestamp = message.Timestamp
                    });
                }
            }
        }
        catch (Exception ex)
        {
            // Log the exception (you should use proper logging)
            await Clients.Caller.SendAsync(ex.Message, "Error", "Failed to send message");
        }
    }

    // Get message history between two users
    public async Task GetMessageHistory(string otherUserId, int page = 1, int pageSize = 50)
    {
        try
        {
            var currentUserId = GetUserId();
            if (currentUserId == null)
            {
                await Clients.Caller.SendAsync("Error", "Authentication required");
                return;
            }

            if (!Guid.TryParse(otherUserId, out var otherUserGuid) || 
                !Guid.TryParse(currentUserId, out var currentUserGuid))
            {
                await Clients.Caller.SendAsync("Error", "Invalid user ID format");
                return;
            }

            var messages = await _context.Messages
                .Where(m => (m.SenderId == currentUserGuid && m.ReceiverId == otherUserGuid) ||
                           (m.SenderId == otherUserGuid && m.ReceiverId == currentUserGuid))
                .OrderByDescending(m => m.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new
                {
                    messageId = m.MessageId,
                    senderId = m.SenderId,
                    receiverId = m.ReceiverId,
                    content = m.Content,
                    timestamp = m.Timestamp
                })
                .ToListAsync();

            await Clients.Caller.SendAsync("MessageHistory", messages.OrderBy(m => m.timestamp));
        }
        catch (Exception )
        {
            await Clients.Caller.SendAsync("Error", "Failed to retrieve message history");
        }
    }

    private string? GetUserId()
    {
        // Get user ID from claims (assuming JWT authentication)
        return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}