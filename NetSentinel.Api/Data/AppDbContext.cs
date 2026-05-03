using Microsoft.EntityFrameworkCore;
using NetSentinel.Api.Models;

namespace NetSentinel.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<Role> Roles { get; set; } 
    public DbSet<User> Users { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<InstalledApplication> InstalledApplications { get; set; }

    public DbSet<SoftwareVulnerability> SoftwareVulnerabilities { get; set; }

    public DbSet<Cve> Cves { get; set; }
    
}