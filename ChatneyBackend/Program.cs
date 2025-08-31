using HotChocolate.AspNetCore;
using ChatneyBackend.Setup;
using MongoDB.Driver;
using ChatneyBackend.Utils;
using ChatneyBackend.Infra.Middleware;
using ChatneyBackend.Infra;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("MongoDB");
var dbName = builder.Configuration.GetConnectionString("dbName");
var UserPasswordSalt = builder.Configuration.GetSection("UserPasswordSalt").Value;
var JwtSecret = builder.Configuration.GetSection("JwtSecret").Value;

if (connectionString == null || dbName == null || UserPasswordSalt == null || JwtSecret == null)
{
    throw new ArgumentException("appsettings are invalid");
}

var url = new MongoUrl(connectionString);
var settings = MongoClientSettings.FromUrl(url);
var mongoClient = new MongoClient(settings);
var db = mongoClient.GetDatabase(dbName);
DbInit.Init(mongoClient, dbName);
builder.Services.AddSingleton((sp) => db);
builder.Services.AddSingleton((sp) => new AppConfig { UserPasswordSalt = UserPasswordSalt, JwtSecret = JwtSecret });
builder.Services.AddSingleton((sp) => new RoleManager(db));

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
    .AddAuthorization()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseMiddleware<AuthMiddleware>();

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
