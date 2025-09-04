using System;
using AppProject.Core.Infrastructure.Database.Entities.Auth;
using AppProject.Core.Infrastructure.Database.Entities.General;
using Microsoft.EntityFrameworkCore;

namespace AppProject.Core.Infrastructure.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets for your entities
    public DbSet<TbUser> Users { get; set; }

    public DbSet<TbCountry> Countries { get; set; }

    public DbSet<TbState> States { get; set; }

    public DbSet<TbCity> Cities { get; set; }

    public DbSet<TbNeighborhood> Neighborhoods { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
