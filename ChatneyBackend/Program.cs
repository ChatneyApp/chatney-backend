using HotChocolate.AspNetCore;
using ChatneyBackend.Setup;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("MongoDB");
var dbName = builder.Configuration.GetConnectionString("dbName");

if (connectionString == null || dbName == null)
{
    throw new ArgumentException("dbName is empty");
}

var url = new MongoUrl(connectionString);
var settings = MongoClientSettings.FromUrl(url);
var mongoClient = new MongoClient(settings);
builder.Services.AddSingleton<IMongoDatabase>((sp) => mongoClient.GetDatabase(dbName));

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
