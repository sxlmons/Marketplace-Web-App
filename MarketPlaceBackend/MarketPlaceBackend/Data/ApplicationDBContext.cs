using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MarketPlaceBackend.Models;

namespace MarketPlaceBackend.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Comments> Comments => Set<Comments>();
    public DbSet<Posts> Posts => Set<Posts>();
    public DbSet<Logs> Logs => Set<Logs>();
}
