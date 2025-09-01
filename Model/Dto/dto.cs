// DTOs/MessageDto.cs
public class MessageDto
{
    public Guid MessageId { get; set; }
    public required string Content { get; set; }
    public DateTime Timestamp { get; set; }
    public Guid SenderId { get; set; }
    public required string SenderName { get; set; }
    public Guid ChatRoomId { get; set; }
}

// DTOs/UserDto.cs
public class UserDto
{
    public Guid UserId { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    // Don't include password, chatRooms, or messages
}


// DTOs/CreateMessageRequest.cs
public class CreateMessageRequest
{
    public required string Content { get; set; }
    public Guid SenderId { get; set; }
    public Guid ChatRoomId { get; set; }
}