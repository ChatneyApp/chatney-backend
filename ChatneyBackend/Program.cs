using HotChocolate.AspNetCore;
using Microsoft.EntityFrameworkCore;
using ChatneyBackend.Models;
using ChatneyBackend.Mutations;
using ChatneyBackend.Queries;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("MongoDB");
var dbName = "chatney";
builder.Services.AddDbContextFactory<ApplicationDbContext>(options => options.UseMongoDB(connectionString!, dbName));

builder.Services.AddGraphQLServer()
    .RegisterDbContextFactory<ApplicationDbContext>()
    .AddQueryType<UserQueries>()
    .AddMutationType<UserMutations>()
    .AddMutationConventions();

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
