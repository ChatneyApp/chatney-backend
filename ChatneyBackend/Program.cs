using HotChocolate.AspNetCore;
using Microsoft.EntityFrameworkCore;
using ChatneyBackend.Models;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Configs;
using ChatneyBackend.Domains.Workspaces;
using ChatneyBackend.Domains.Permissions;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("MongoDB");
var dbName = builder.Configuration.GetConnectionString("dbName");

if (dbName == null)
{
    throw new ArgumentException("dbName is empty");
}
builder.Services.AddDbContextFactory<ApplicationDbContext>(options => options.UseMongoDB(connectionString!, dbName));

IDomainSetup[] endpoints = {
    new UserDomainSetup(),
    new ChannelDomainSetup(),
    new MessageDomainSetup(),
    new RoleDomainSetup(),
    new ConfigDomainSetup(),
    new WorkspaceDomainSetup(),
    new PermissionDomainSetup()
};

var endpointBuilder = builder.Services.AddGraphQLServer()
    .RegisterDbContextFactory<ApplicationDbContext>()
    .AddQueryType(d => d.Name("Query"))
    .AddMutationType(d => d.Name("Mutation"));

foreach (var endpoint in endpoints)
{
    endpoint.Setup(endpointBuilder);
}
endpointBuilder.AddMutationConventions();


var app = builder.Build();
app.MapGraphQL("/query").WithOptions(new GraphQLServerOptions
{
    EnableMultipartRequests = true,
    Tool = {
        DisableTelemetry = true,
#if DEBUG
        Enable = true
#else
        Enable = false
#endif
    }
});
app.Run();
