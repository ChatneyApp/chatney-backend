using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace ChatneyBackend.Models;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // general stuff
        modelBuilder.Entity<User>().ToCollection("users");

        // relationships
        // modelBuilder.Entity<User>().HasMany<Workspace>();

        // default records
        // modelBuilder.Entity<User>().HasData(
        //     new {Id = "id1", Name = "name 1", Email = "mail1@ddd.com"},
        //     new {Id = "id2", Name = "name 2", Email = "mail2@ddd.com"},
        //     new {Id = "id3", Name = "name 3", Email = "mail3@ddd.com"}
        // );
    }
}
