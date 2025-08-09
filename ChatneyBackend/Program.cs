using HotChocolate.AspNetCore;
using Microsoft.EntityFrameworkCore;
using ChatneyBackend.Setup;
using ChatneyBackend.Domains.Users;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("MongoDB");
var dbName = builder.Configuration.GetConnectionString("dbName");

if (dbName == null)
{
    throw new ArgumentException("dbName is empty");
}

builder.Services.AddDbContextFactory<ApplicationDbContext>(options => options.UseMongoDB(connectionString!, dbName));

var endpointBuilder = builder.Services.AddGraphQLServer()
    .RegisterDbContextFactory<ApplicationDbContext>()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>();

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
