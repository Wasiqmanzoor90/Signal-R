using Microsoft.EntityFrameworkCore;
using MyApiProject.Model;

namespace MyApiProject.Service
{
    public class SqlDbContext : DbContext
    {
        public SqlDbContext(DbContextOptions<SqlDbContext> options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }

        public DbSet<ChatRoom> ChatRooms { get; set; }




//Minimal api's
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           // Many-to-Many: User ↔ ChatRoom
            modelBuilder.Entity<User>()
                .HasMany(u => u.ChatRooms)
                .WithMany(c => c.Users)
                .UsingEntity(j => j.ToTable("UserChatRooms"));

            // One-to-Many: ChatRoom ↔ Messages
            modelBuilder.Entity<Message>()
                .HasOne(m => m.ChatRoom)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ChatRoomId);

            // One-to-Many: User ↔ Messages
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.Messages)
                .HasForeignKey(m => m.SenderId);

        }

    }


}