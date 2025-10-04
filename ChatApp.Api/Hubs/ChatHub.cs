using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using ChatApp.Core.Interfaces;
using ChatApp.Core.Entities;
using System.Security.Claims;
using ChatApp.Core.Dtos;

namespace ChatApp.Api.Hubs;

[Authorize]
public class ChatHub(
    IMessageRepository messageRepository,
    IRoomRepository roomRepository,
    IUserRepository userRepository,
    ILogger<ChatHub> logger) : Hub
{
    private readonly IMessageRepository _messageRepository = messageRepository;
    private readonly IRoomRepository _roomRepository = roomRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ILogger<ChatHub> _logger = logger;

    private static class EventNames
    {
        public const string
            UserJoinedRoom = "UserJoinedRoom",
            UserLeftRoom = "UserLeftRoom",
            ReceiveMessage = "ReceiveMessage",
            MessageEdited = "MessageEdited",
            MessageDeleted = "MessageDeleted",
            UserStartedTyping = "UserStartedTyping",
            UserStoppedTyping = "UserStoppedTyping",
            UserStatusChanged = "UserStatusChanged";
    }

    // Connection Management
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId > 0)
        {
            await UpdateUserStatus(userId, true);
            _logger.LogInformation("User {userId} connected: {connectionId}", userId, Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId > 0)
        {
            await UpdateUserStatus(userId, false);
            _logger.LogInformation("User {userId} disconnected: {connectionId}", userId, Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Room Operations
    public async Task JoinRoom(int roomId)
    {
        var userId = GetUserId();

        // Verify user has access to this room
        var isMember = await _roomRepository.IsUserMemberAsync(roomId, userId);
        if (!isMember)
            throw new HubException("You are not a member of this room");

        var groupName = GetGroupName(roomId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        // Notify others in the room
        await Clients.OthersInGroup(groupName).SendAsync(
            EventNames.UserJoinedRoom,
            new RoomEventDto { UserId = userId, RoomId = roomId, Timestamp = DateTime.UtcNow }
        );

        _logger.LogInformation("User {userId} joined room {roomId}", userId, roomId);
    }

    public async Task LeaveRoom(int roomId)
    {
        var userId = GetUserId();

        var groupName = GetGroupName(roomId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        // Notify others in the room
        await Clients.OthersInGroup(groupName).SendAsync(
            EventNames.UserLeftRoom,
            new RoomEventDto { UserId = userId, RoomId = roomId, Timestamp = DateTime.UtcNow }
        );

        _logger.LogInformation("User {userId} left room {roomId}", userId, roomId);
    }

    // Message Operations
    public async Task SendMessage(int roomId, string content)
    {
        var userId = GetUserId();

        // Verify user has access to this room
        var isMember = await _roomRepository.IsUserMemberAsync(roomId, userId);
        if (!isMember)
            throw new HubException("You are not a member of this room");

        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new HubException("User not found");

        // Create and save message
        var message = new Message
        {
            Content = content,
            UserId = userId,
            RoomId = roomId,
            Type = MessageType.Text,
            CreatedAt = DateTime.UtcNow,
            User = user
        };
        await _messageRepository.CreateAsync(message);

        // Send to all clients in the room (including sender)
        await Clients.Group(GetGroupName(roomId)).SendAsync(
            EventNames.ReceiveMessage,
            MessageDto.FromEntity(message)
        );

        _logger.LogInformation("User {userId} sent message to room {roomId}", userId, roomId);
    }

    public async Task EditMessage(int messageId, string newContent)
    {
        var userId = GetUserId();
        var message = await _messageRepository.GetByIdAsync(messageId)
            ?? throw new HubException("Message not found");

        if (message.UserId != userId)
            throw new HubException("You can only edit your own messages");

        message.Content = newContent;
        message.EditedAt = DateTime.UtcNow;

        await _messageRepository.UpdateAsync(message);

        // Notify all clients in the room
        await Clients.Group(GetGroupName(message.RoomId)).SendAsync(
            EventNames.MessageEdited,
            new MessageEditedDto
            {
                Id = messageId,
                Content = newContent,
                EditedAt = message.EditedAt.Value
            }
        );
    }

    public async Task DeleteMessage(int messageId)
    {
        var userId = GetUserId();
        var message = await _messageRepository.GetByIdAsync(messageId)
            ?? throw new HubException("Message not found");

        if (message.UserId != userId)
            throw new HubException("You can only delete your own messages");

        await _messageRepository.DeleteAsync(messageId);

        // Notify all clients in the room
        await Clients.Group(GetGroupName(message.RoomId)).SendAsync(
            EventNames.MessageDeleted,
            new MessageDeletedDto { Id = messageId, RoomId = message.RoomId }
        );
    }

    // Typing Indicators
    public async Task StartTyping(int roomId)
    {
        var userId = GetUserId();
        var user = await _userRepository.GetByIdAsync(userId);

        await Clients.OthersInGroup(GetGroupName(roomId)).SendAsync(
            EventNames.UserStartedTyping,
            new TypingIndicatorDto { UserId = userId, Username = user?.Username ?? "Unknown", RoomId = roomId }
        );
    }

    public async Task StopTyping(int roomId)
    {
        var userId = GetUserId();

        await Clients.OthersInGroup(GetGroupName(roomId)).SendAsync(
            EventNames.UserStoppedTyping,
            new TypingIndicatorDto { UserId = userId, RoomId = roomId }
        );
    }

    // Helper Methods
    private static string GetGroupName(int roomId) => $"room_{roomId}";

    private int GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private async Task UpdateUserStatus(int userId, bool isOnline)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user != null)
        {
            user.IsOnline = isOnline;
            user.LastSeen = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            // Broadcast status change to all connected clients
            await Clients.All.SendAsync(
                EventNames.UserStatusChanged,
                new UserStatusChangedDto { UserId = userId, IsOnline = isOnline, LastSeen = user.LastSeen }
            );
        }
    }
}