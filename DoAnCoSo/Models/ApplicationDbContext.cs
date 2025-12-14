using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DoAnCoSo.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options) : base(options) { 
        
        }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<Follow>Follow { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Follow>()
                .HasKey(f => new { f.FollowerId, f.FollowingId });

            builder.Entity<Follow>()
                .HasOne(f => f.Follower)
                .WithMany(u => u.Followings)
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Follow>()
                .HasOne(f => f.Following)
                .WithMany(u => u.Followers)
                .HasForeignKey(f => f.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Message>()
                .HasOne(m => m.FromUser)
                .WithMany()
                .HasForeignKey(m => m.FromUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(m => m.ToUser)
                .WithMany()
                .HasForeignKey(m => m.ToUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }


    }
}
