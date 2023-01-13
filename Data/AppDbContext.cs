using Authentication.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Data
{
    public class AppDbContext: IdentityDbContext
    {
        public DbSet<Hotel> Hotels { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        {
        }
    }
}
