using BaseWebApp.Dal.Entities;
using Microsoft.EntityFrameworkCore;

namespace BaseWebApp.Dal;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // This DbSet<User> will become a 'Users' table in the database
    public DbSet<User> Users { get; set; }
}