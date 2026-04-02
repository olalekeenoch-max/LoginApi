using LoginApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ProjecteE.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<RegisterRequest> RegisterRequests { get; set; }
        public DbSet<LoginApi.Models.User> Users { get; set; }
    }
}
