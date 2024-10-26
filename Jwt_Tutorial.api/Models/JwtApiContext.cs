using Microsoft.EntityFrameworkCore;

namespace Jwt_Tutorial.api.Models
{
    public class JwtApiContext: DbContext
    {
        public JwtApiContext(DbContextOptions<JwtApiContext> options) : base(options) { }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);
        //}

        public DbSet<User> Users { get; set; }

    }
}
