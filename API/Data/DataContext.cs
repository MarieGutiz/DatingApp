using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext : DbContext
    {
        

        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<AppUser> Users { get; set; }

        public DbSet<UserLike> Likes { get; set; }//table name

        protected override void OnModelCreating(ModelBuilder builder){//Configuring
            
            base.OnModelCreating(builder);

            //primary key 4 the table
            builder.Entity<UserLike>()
            .HasKey(k=>new {k.SourceUserId, k.LikedUserId});

            //Specify the relationships
            builder.Entity<UserLike>()
            .HasOne(s=>s.SourceUser)
            .WithMany(l=>l.LikedUsers)
            .HasForeignKey(s=>s.SourceUserId)
            .OnDelete(DeleteBehavior.Cascade);//<-- 4 sql server set DeleteBehavior.NoAction

            builder.Entity<UserLike>()//<--The other side of relationships
            .HasOne(s=>s.LikedUser)
            .WithMany(l=>l.LikedByUsers)
            .HasForeignKey(s=>s.LikedUserId)
            .OnDelete(DeleteBehavior.Cascade);
        }
     
    }
}