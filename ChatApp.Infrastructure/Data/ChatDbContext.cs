using Microsoft.EntityFrameworkCore;
using ChatApp.Core.Entities;

namespace ChatApp.Infrastructure.Data;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<RoomMember> RoomMembers { get; set; }
    public DbSet<MessageReaction> MessageReactions { get; set; }

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

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed a default "General" room
        modelBuilder.Entity<Room>().HasData(
            new Room
            {
                Id = 1,
                Name = "General",
                Description = "Default chat room for everyone",
                IsPrivate = false,
                CreatedBy = 1, // We'll create a system user
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
