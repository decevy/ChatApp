using Microsoft.EntityFrameworkCore;
using ChatApp.Core.Entities;

namespace ChatApp.Infrastructure.Data;

public class ChatDbContext(DbContextOptions<ChatDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<RoomMember> RoomMembers { get; set; }
    public DbSet<MessageReaction> MessageReactions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSeeding((context, _) => 
            SeedDataAsync(context, CancellationToken.None).GetAwaiter().GetResult());
        
        optionsBuilder.UseAsyncSeeding(async (context, _, cancellationToken) => 
            await SeedDataAsync(context, cancellationToken));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.Username).HasMaxLength(50);
            entity.Property(u => u.Email).HasMaxLength(255);
            entity.Property(u => u.PasswordHash).HasMaxLength(255);
        });

        // Room configuration
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).HasMaxLength(100);
            entity.Property(r => r.Description).HasMaxLength(500);
            
            entity.HasOne(r => r.Creator)
                  .WithMany()
                  .HasForeignKey(r => r.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Message configuration
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Content).HasMaxLength(2000);
            entity.Property(m => m.AttachmentFileName).HasMaxLength(255);
            entity.Property(m => m.AttachmentUrl).HasMaxLength(500);
            
            entity.HasOne(m => m.User)
                  .WithMany(u => u.Messages)
                  .HasForeignKey(m => m.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(m => m.Room)
                  .WithMany(r => r.Messages)
                  .HasForeignKey(m => m.RoomId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(m => m.CreatedAt);
            entity.HasIndex(m => new { m.RoomId, m.CreatedAt });
        });

        // RoomMember configuration
        modelBuilder.Entity<RoomMember>(entity =>
        {
            entity.HasKey(rm => rm.Id);
            
            entity.HasOne(rm => rm.User)
                  .WithMany(u => u.RoomMemberships)
                  .HasForeignKey(rm => rm.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(rm => rm.Room)
                  .WithMany(r => r.Members)
                  .HasForeignKey(rm => rm.RoomId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(rm => new { rm.UserId, rm.RoomId }).IsUnique();
        });

        // MessageReaction configuration
        modelBuilder.Entity<MessageReaction>(entity =>
        {
            entity.HasKey(mr => mr.Id);
            entity.Property(mr => mr.Emoji).HasMaxLength(10);
            
            entity.HasOne(mr => mr.Message)
                  .WithMany(m => m.Reactions)
                  .HasForeignKey(mr => mr.MessageId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(mr => mr.User)
                  .WithMany()
                  .HasForeignKey(mr => mr.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(mr => new { mr.MessageId, mr.UserId, mr.Emoji }).IsUnique();
        });
    }
    
    private static async Task SeedDataAsync(DbContext dbContext, CancellationToken cancellationToken)
    {
        var context = dbContext as ChatDbContext 
            ?? throw new InvalidOperationException("Invalid DbContext type for seeding");

        if (await context.Users.AnyAsync(cancellationToken)) return;

        // Users
        User[] users =
        [
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
        ];
        await context.Users.AddRangeAsync(users, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        
        // Store user IDs before clearing change tracking
        var userIds = users.Select(u => u.Id).ToArray();
        context.ChangeTracker.Clear();

        // Rooms
        Room[] rooms =
        [
            new Room
            {
                Name = "General",
                Description = "General discussion room",
                IsPrivate = false,
                CreatedBy = userIds[0],
                CreatedAt = DateTime.UtcNow
            },
            new Room
            {
                Name = "Bachata",
                Description = "Bachata chat and fun",
                IsPrivate = false,
                CreatedBy = userIds[0],
                CreatedAt = DateTime.UtcNow
            },
            new Room
            {
                Name = "Gym bros",
                Description = "Chat about gym and fitness",
                IsPrivate = true,
                CreatedBy = userIds[0],
                CreatedAt = DateTime.UtcNow
            }
        ];
        await context.Rooms.AddRangeAsync(rooms, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        
        // Store room IDs before clearing change tracking
        var roomIds = rooms.Select(r => r.Id).ToArray();
        context.ChangeTracker.Clear();

        // Room members
        RoomMember[] roomMembers = [
            // Alice in all rooms (creator)
            new RoomMember { UserId = userIds[0], RoomId = roomIds[0], Role = RoomRole.Admin, JoinedAt = DateTime.UtcNow },
            new RoomMember { UserId = userIds[0], RoomId = roomIds[1], Role = RoomRole.Admin, JoinedAt = DateTime.UtcNow },
            new RoomMember { UserId = userIds[0], RoomId = roomIds[2], Role = RoomRole.Admin, JoinedAt = DateTime.UtcNow },
            
            // Bob in General and Random
            new RoomMember { UserId = userIds[1], RoomId = roomIds[0], Role = RoomRole.Member, JoinedAt = DateTime.UtcNow },
            new RoomMember { UserId = userIds[1], RoomId = roomIds[1], Role = RoomRole.Member, JoinedAt = DateTime.UtcNow },
            
            // Charlie in all rooms
            new RoomMember { UserId = userIds[2], RoomId = roomIds[0], Role = RoomRole.Member, JoinedAt = DateTime.UtcNow },
            new RoomMember { UserId = userIds[2], RoomId = roomIds[1], Role = RoomRole.Moderator, JoinedAt = DateTime.UtcNow },
            new RoomMember { UserId = userIds[2], RoomId = roomIds[2], Role = RoomRole.Member, JoinedAt = DateTime.UtcNow }
        ];
        await context.RoomMembers.AddRangeAsync(roomMembers, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        context.ChangeTracker.Clear();

        // Messages
        Message[] messages = [
            new Message
            {
                Content = "Welcome to the General room! 👋",
                UserId = userIds[0],
                RoomId = roomIds[0],
                Type = MessageType.Text,
                CreatedAt = DateTime.UtcNow.AddMinutes(-30)
            },
            new Message
            {
                Content = "Hey everyone! 🤗",
                UserId = userIds[1],
                RoomId = roomIds[0],
                Type = MessageType.Text,
                CreatedAt = DateTime.UtcNow.AddMinutes(-25)
            },
            new Message
            {
                Content = "Holaaa",
                UserId = userIds[2],
                RoomId = roomIds[0],
                Type = MessageType.Text,
                CreatedAt = DateTime.UtcNow.AddMinutes(-20)
            },
            new Message
            {
                Content = "Welcome to the Bachata room! 😎",
                UserId = userIds[1],
                RoomId = roomIds[1],
                Type = MessageType.Text,
                CreatedAt = DateTime.UtcNow.AddMinutes(-15)
            },
            new Message
            {
                Content = "Lightweighttttt",
                UserId = userIds[0],
                RoomId = roomIds[2],
                Type = MessageType.Text,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10)
            }
        ];
        await context.Messages.AddRangeAsync(messages, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}