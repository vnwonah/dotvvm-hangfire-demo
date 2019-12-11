using Microsoft.EntityFrameworkCore;
using DotvvmHangfireDemo.DAL.Entities;
namespace DotvvmHangfireDemo.DAL
{
    public class StudentDbContext : DbContext
    {
        public StudentDbContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }
        public DbSet<Student> Students { get; set; }
    }
}
