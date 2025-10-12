using ChatApp.Core.Dtos;
using ChatApp.Core.Dtos.Requests;
using ChatApp.Core.Dtos.Responses;
using ChatApp.Core.Entities;
using ChatApp.Core.Interfaces;

namespace ChatApp.Services;

public class RoomService(
    IRoomRepository roomRepository,
    IMessageRepository messageRepository,
    IUserRepository userRepository) : IRoomService
{
    public async Task<List<RoomSummaryDto>> GetUserRoomsAsync(int userId)
    {
        var rooms = await roomRepository.Query()
            .WithCreator()
            .WithMembers()
            .WhereUserIsMember(userId)
            .ToListAsync();
        
        var roomDtos = new List<RoomSummaryDto>();
        foreach (var room in rooms)
        {
            var lastMessage = await messageRepository.GetLastMessageInRoomAsync(room.Id);
            if (lastMessage != null)
                room.Messages.Add(lastMessage);
            var roomDto = RoomSummaryDto.FromEntity(room);
            roomDtos.Add(roomDto);
        }
        
        return roomDtos;
    }

    public async Task<RoomDto?> GetRoomAsync(int roomId, int userId)
    {
        // Check if user is a member
        if (!await roomRepository.IsUserMemberAsync(roomId, userId))
            throw new UnauthorizedAccessException("User is not a member of this room");
        
        var room = await roomRepository.Query()
            .WithCreator()
            .WithMembers()
            .FindByIdAsync(roomId);
        
        if (room == null)
            return null;

        return RoomDto.FromEntity(room);
    }

    public async Task<RoomDto> CreateRoomAsync(CreateRoomRequest request, int userId)
    {
        var user = await userRepository.FindByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        // Create room
        var room = new Room
        {
            Name = request.Name,
            Description = request.Description,
            IsPrivate = request.IsPrivate,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            Creator = user
        };
        room = await roomRepository.CreateAsync(room);

        // Add creator as admin member
        var roomMember = new RoomMember
        {
            UserId = userId,
            RoomId = room.Id,
            Role = RoomRole.Admin,
            JoinedAt = DateTime.UtcNow
        };
        await roomRepository.AddRoomMemberAsync(roomMember);
        room.Members.Add(roomMember);

        return RoomDto.FromEntity(room);
    }

    public async Task<RoomDto?> UpdateRoomAsync(int roomId, UpdateRoomRequest request, int userId)
    {
        // Check if user is admin
        if (!await roomRepository.IsUserRoomAdminAsync(roomId, userId))
            throw new UnauthorizedAccessException("User is not an admin of this room");

        var room = await roomRepository.FindByIdAsync(roomId);
        if (room == null)
            return null;

        room.Name = request.Name;
        room.Description = request.Description;
        
        await roomRepository.UpdateAsync(room);

        room = await roomRepository.Query()
            .WithCreator()
            .WithMembers()
            .GetByIdAsync(room.Id);
        
        return RoomDto.FromEntity(room);
    }

    public async Task<bool> DeleteRoomAsync(int roomId, int userId)
    {
        // Check if user is admin
        if (!await roomRepository.IsUserRoomAdminAsync(roomId, userId))
            throw new UnauthorizedAccessException("User is not an admin of this room");

        var room = await roomRepository.FindByIdAsync(roomId);
        if (room == null)
            return false;

        await roomRepository.DeleteAsync(roomId);
        
        return true;
    }

    public async Task<string> AddMemberAsync(int roomId, AddRoomMemberRequest request, int userId)
    {
        // Check if user is admin
        if (!await roomRepository.IsUserRoomAdminAsync(roomId, userId))
            throw new UnauthorizedAccessException("User is not an admin of this room");

        var room = await roomRepository.FindByIdAsync(roomId);
        if (room == null)
            throw new InvalidOperationException("Room not found");

        // Check if user to add exists
        var userToAdd = await userRepository.FindByIdAsync(request.UserId);
        if (userToAdd == null)
            throw new InvalidOperationException("User not found");

        // Check if already a member
        if (await roomRepository.IsUserMemberAsync(roomId, request.UserId))
            throw new InvalidOperationException("User is already a member");

        await roomRepository.AddRoomMemberAsync(new RoomMember
        {
            UserId = request.UserId,
            RoomId = roomId,
            Role = request.IsAdmin ? RoomRole.Admin : RoomRole.Member,
            JoinedAt = DateTime.UtcNow
        });

        return $"User {userToAdd.Username} added to room";
    }

    public async Task<bool> RemoveMemberAsync(int roomId, int userIdToRemove, int currentUserId)
    {
        // Users can remove themselves, or admins can remove others
        var isRemovingOther = userIdToRemove != currentUserId;
        var isAdmin = await roomRepository.IsUserRoomAdminAsync(roomId, currentUserId);
        if (isRemovingOther && !isAdmin)
            throw new UnauthorizedAccessException("Only admins can remove other members");

        var room = await roomRepository.FindByIdAsync(roomId);
        if (room == null)
            return false;

        var member = await roomRepository.GetRoomMemberAsync(roomId, userIdToRemove);
        if (member == null)
            throw new InvalidOperationException("User is not a member of this room");

        // Prevent removing the last admin
        if (member.Role == RoomRole.Admin)
        {
            var roomWithMembers = await roomRepository.Query().WithMembers().FindByIdAsync(roomId);
            var adminCount = roomWithMembers?.Members.Count(m => m.Role == RoomRole.Admin) ?? 0;
            if (adminCount == 1)
                throw new InvalidOperationException("Cannot remove the last admin");
        }

        await roomRepository.RemoveRoomMemberAsync(roomId, userIdToRemove);
        
        return true;
    }

    public async Task<PaginatedResponse<MessageDto>> GetRoomMessagesAsync(int roomId, int userId, int page, int pageSize)
    {
        // Check if user is a member
        if (!await roomRepository.IsUserMemberAsync(roomId, userId))
            throw new UnauthorizedAccessException("User is not a member of this room");

        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 50;

        var (messages, totalCount) = await messageRepository.Query()
            .WithUser()
            .WhereRoomId(roomId)
            .ToPagedListAsync(page, pageSize);

        return new PaginatedResponse<MessageDto>
        {
            Items = messages.Select(MessageDto.FromEntity).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    // Not yet implemented methods
    public Task<IEnumerable<RoomSummaryDto>> GetPublicRoomsAsync()
    {
        throw new NotImplementedException();
    }

    public Task JoinRoomAsync(int roomId, int userId)
    {
        throw new NotImplementedException();
    }

    public Task LeaveRoomAsync(int roomId, int userId)
    {
        throw new NotImplementedException();
    }
}

