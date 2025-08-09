using HotChocolate.AspNetCore;
using Microsoft.EntityFrameworkCore;
using ChatneyBackend.Setup;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("MongoDB");
var dbName = builder.Configuration.GetConnectionString("dbName");

if (dbName == null)
{
    throw new ArgumentException("dbName is empty");
}

builder.Services.AddDbContextFactory<ApplicationDbContext>(options => options.UseMongoDB(connectionString!, dbName));

// ---- CORS policies ----
// Dev: open for local tooling; Prod: strict allow-list with credentials.
const string DevOpenCors = "DevOpenCors";
const string ProdCors = "ProdCors";

string[] allowedProdOrigins =
{
    "http://localhost:3001",
    "https://chatney.com"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy(DevOpenCors, policy =>
    {
        // Do NOT call AllowCredentials with AllowAnyOrigin.
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });

    options.AddPolicy(ProdCors, policy =>
    {
        policy
            .WithOrigins(allowedProdOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // enable cookies/auth tokens if you need them
    });
});

builder.Services.AddGraphQLServer()
    .RegisterDbContextFactory<ApplicationDbContext>()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseCors(DevOpenCors);
}
else
{
    app.UseCors(ProdCors);
}
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
