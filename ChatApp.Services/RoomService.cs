using ChatApp.Core.Dtos;
using ChatApp.Core.Dtos.Requests;
using ChatApp.Core.Dtos.Responses;
using ChatApp.Core.Entities;
using ChatApp.Core.Exceptions;
using ChatApp.Core.Extensions;
using ChatApp.Core.Interfaces;

namespace ChatApp.Services;

public class RoomService(
    IRoomRepository roomRepository,
    IMessageRepository messageRepository,
    IUserRepository userRepository) : IRoomService
{
    public async Task<List<RoomSummaryDto>> GetUserRoomsAsync(int userId)
    {
        // Get all rooms for user
        var rooms = await roomRepository.Query()
            .WithCreator()
            .WithMembers()
            .WhereUserIsMember(userId)
            .ToListAsync();
        
        // Get last message for each room and create dtos
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
            throw new ForbiddenException("User is not a member of this room");
        
        // Get room by id
        var room = await roomRepository.Query()
            .WithCreator()
            .WithMembers()
            .FindByIdAsync(roomId);
    
        return room?.Transform(RoomDto.FromEntity);
    }

    public async Task<RoomDto> CreateRoomAsync(CreateRoomRequest request, int userId)
    {
        var user = await userRepository.FindByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

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

        return RoomDto.FromEntity(room);
    }

    public async Task<RoomDto> UpdateRoomAsync(int roomId, UpdateRoomRequest request, int userId)
    {
        if (!await roomRepository.IsUserRoomAdminAsync(roomId, userId))
            throw new ForbiddenException("User is not an admin of this room");

        // Query WITHOUT includes for the update (better performance)
        var room = await roomRepository.FindByIdAsync(roomId)
            ?? throw new NotFoundException("Room", roomId);

        // Update room properties
        room.Name = request.Name;
        room.Description = request.Description;
        await roomRepository.UpdateAsync(room);

        // Query WITH includes for DTO mapping
        var updatedRoom = await roomRepository.Query()
            .WithCreator()
            .WithMembers()
            .GetByIdAsync(roomId);
        
        return RoomDto.FromEntity(updatedRoom);
    }

    public async Task DeleteRoomAsync(int roomId, int userId)
    {
        if (!await roomRepository.IsUserRoomAdminAsync(roomId, userId))
            throw new ForbiddenException("User is not an admin of this room");

        if (!await roomRepository.ExistsAsync(roomId))
            throw new NotFoundException("Room", roomId);

        await roomRepository.DeleteAsync(roomId);
    }

    public async Task AddMemberAsync(int roomId, AddRoomMemberRequest request, int userId)
    {
        if (!await roomRepository.IsUserRoomAdminAsync(roomId, userId))
            throw new ForbiddenException("User is not an admin of this room");

        if (!await roomRepository.ExistsAsync(roomId))
            throw new NotFoundException("Room", roomId);

        if (!await userRepository.ExistsAsync(request.UserId))
            throw new NotFoundException("User", request.UserId);

        if (await roomRepository.IsUserMemberAsync(roomId, request.UserId))
            throw new BadRequestException("User is already a member");

        // Add room member
        await roomRepository.AddRoomMemberAsync(new RoomMember
        {
            UserId = request.UserId,
            RoomId = roomId,
            Role = request.IsAdmin ? RoomRole.Admin : RoomRole.Member,
            JoinedAt = DateTime.UtcNow
        });
    }

    public async Task RemoveMemberAsync(int roomId, int userIdToRemove, int currentUserId)
    {
        // Users can remove themselves, or admins can remove others
        var isRemovingOther = userIdToRemove != currentUserId;
        var isAdmin = await roomRepository.IsUserRoomAdminAsync(roomId, currentUserId);
        if (isRemovingOther && !isAdmin)
            throw new ForbiddenException("Only admins can remove other members");

        // Check if room exists
        if (!await roomRepository.ExistsAsync(roomId))
            throw new NotFoundException("Room", roomId);

        // Get room member by id
        var member = await roomRepository.FindRoomMemberAsync(roomId, userIdToRemove)
            ?? throw new NotFoundException($"User with ID {userIdToRemove} is not a member of this room");

        // Prevent removing the last admin
        if (member.Role == RoomRole.Admin)
        {
            var roomWithMembers = await roomRepository.Query()
                .WithMembers()
                .FindByIdAsync(roomId);
            var adminCount = roomWithMembers?.Members.Count(m => m.Role == RoomRole.Admin) ?? 0;
            if (adminCount == 1)
                throw new BadRequestException("Cannot remove the last admin");
        }

        await roomRepository.RemoveRoomMemberAsync(roomId, userIdToRemove);
    }

    public async Task<PaginatedResponse<MessageDto>> GetRoomMessagesAsync(int roomId, int userId, int page, int pageSize)
    {
        // Check if user is a member
        if (!await roomRepository.IsUserMemberAsync(roomId, userId))
            throw new ForbiddenException("User is not a member of this room");

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

