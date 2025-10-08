using ChatApp.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Data;

public static class ChatDbSeeder
{
    public static async Task SeedAsync(ChatDbContext context)
    {
        // Check if we already have data
        if (await context.Users.AnyAsync())
        {
            return; // Database has been seeded
        }

        // Create test users
        var users = new List<User>
        {
            new User
            {
                Username = "aya",
                Email = "aya@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("test123"),
                IsOnline = false,
                CreatedAt = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow
            },
            new User
            {
                Username = "bobby",
                Email = "bobby@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("test123"),
                IsOnline = false,
                CreatedAt = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow
            },
            new User
            {
                Username = "carlos",
                Email = "carlos@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("test123"),
                IsOnline = false,
                CreatedAt = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Create test rooms
        var rooms = new List<Room>
        {
            new Room
            {
                Name = "General",
                Description = "General discussion room",
                IsPrivate = false,
                CreatedBy = users[0].Id,
                CreatedAt = DateTime.UtcNow
            },
            new Room
            {
                Name = "Bachata",
                Description = "Bachata chat and fun",
                IsPrivate = false,
                CreatedBy = users[0].Id,
                CreatedAt = DateTime.UtcNow
            },
            new Room
            {
                Name = "Gym bros",
                Description = "Chat about gym and fitness",
                IsPrivate = true,
                CreatedBy = users[0].Id,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Rooms.AddRangeAsync(rooms);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Add users to rooms
        var roomMembers = new List<RoomMember>
        {
            // Alice in all rooms (creator)
            new RoomMember { UserId = users[0].Id, RoomId = rooms[0].Id, Role = RoomRole.Admin, JoinedAt = DateTime.UtcNow },
            new RoomMember { UserId = users[0].Id, RoomId = rooms[1].Id, Role = RoomRole.Admin, JoinedAt = DateTime.UtcNow },
            new RoomMember { UserId = users[0].Id, RoomId = rooms[2].Id, Role = RoomRole.Admin, JoinedAt = DateTime.UtcNow },
            
            // Bob in General and Random
            new RoomMember { UserId = users[1].Id, RoomId = rooms[0].Id, Role = RoomRole.Member, JoinedAt = DateTime.UtcNow },
            new RoomMember { UserId = users[1].Id, RoomId = rooms[1].Id, Role = RoomRole.Member, JoinedAt = DateTime.UtcNow },
            
            // Charlie in all rooms
            new RoomMember { UserId = users[2].Id, RoomId = rooms[0].Id, Role = RoomRole.Member, JoinedAt = DateTime.UtcNow },
            new RoomMember { UserId = users[2].Id, RoomId = rooms[1].Id, Role = RoomRole.Moderator, JoinedAt = DateTime.UtcNow },
            new RoomMember { UserId = users[2].Id, RoomId = rooms[2].Id, Role = RoomRole.Member, JoinedAt = DateTime.UtcNow }
        };

        await context.RoomMembers.AddRangeAsync(roomMembers);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Add some initial messages
        var messages = new List<Message>
        {
            new Message
            {
                Content = "Welcome to the General room! 👋",
                UserId = users[0].Id,
                RoomId = rooms[0].Id,
                Type = MessageType.Text,
                CreatedAt = DateTime.UtcNow.AddMinutes(-30)
            },
            new Message
            {
                Content = "Hey everyone! 🤗",
                UserId = users[1].Id,
                RoomId = rooms[0].Id,
                Type = MessageType.Text,
                CreatedAt = DateTime.UtcNow.AddMinutes(-25)
            },
            new Message
            {
                Content = "Holaaa",
                UserId = users[2].Id,
                RoomId = rooms[0].Id,
                Type = MessageType.Text,
                CreatedAt = DateTime.UtcNow.AddMinutes(-20)
            },
            new Message
            {
                Content = "Welcome to the Bachata room! 😎",
                UserId = users[1].Id,
                RoomId = rooms[1].Id,
                Type = MessageType.Text,
                CreatedAt = DateTime.UtcNow.AddMinutes(-15)
            },
            new Message
            {
                Content = "Lightweighttttt",
                UserId = users[0].Id,
                RoomId = rooms[2].Id,
                Type = MessageType.Text,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10)
            }
        };

        await context.Messages.AddRangeAsync(messages);
        await context.SaveChangesAsync();
    }
}