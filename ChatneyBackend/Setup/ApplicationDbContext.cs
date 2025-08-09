using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Configs;
using ChatneyBackend.Domains.Workspaces;
using ChatneyBackend.Domains.Permissions;

namespace ChatneyBackend.Setup;


public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Channel> Channels => Set<Channel>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Config> Configs => Set<Config>();
    public DbSet<Workspace> Workspaces => Set<Workspace>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // general stuff
        modelBuilder.Entity<User>().ToCollection("users");
        modelBuilder.Entity<Channel>().ToCollection("channels");
        modelBuilder.Entity<ChannelType>().ToCollection("channel_types");
        modelBuilder.Entity<Message>().ToCollection("messages");
        modelBuilder.Entity<Role>().ToCollection("roles");
        modelBuilder.Entity<Config>().ToCollection("system_configs");
        modelBuilder.Entity<Workspace>().ToCollection("workspaces");

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
