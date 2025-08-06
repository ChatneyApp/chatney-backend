using HotChocolate.AspNetCore;
using Microsoft.EntityFrameworkCore;
using ChatneyBackend.Models;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Domains.Channels;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("MongoDB");
var dbName = "chatney";
builder.Services.AddDbContextFactory<ApplicationDbContext>(options => options.UseMongoDB(connectionString!, dbName));

IDomainSetup[] endpoints = {
    new UserDomainSetup(),
    new ChannelDomainSetup()
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
