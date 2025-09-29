using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Configs;
using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Domains.Workspaces;
using HotChocolate.AspNetCore;
using ChatneyBackend.Setup;
using MongoDB.Driver;
using ChatneyBackend.Utils;
using ChatneyBackend.Infra.Middleware;
using ChannelDomainSettings = ChatneyBackend.Domains.Channels.DomainSettings;
using ConfigsDomainSettings = ChatneyBackend.Domains.Configs.DomainSettings;
using MessagesDomainSettings = ChatneyBackend.Domains.Messages.DomainSettings;
using RolesDomainSettings = ChatneyBackend.Domains.Roles.DomainSettings;
using UserDomainSettings = ChatneyBackend.Domains.Users.DomainSettings;
using WorkspacesDomainSettings = ChatneyBackend.Domains.Workspaces.DomainSettings;
using Microsoft.AspNetCore.WebSockets;

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

var messagesRepo = new Repo<Message>(db, MessagesDomainSettings.MessageCollectionName);
var usersRepo = new Repo<User>(db, UserDomainSettings.UserCollectionName);
var channelsRepo = new Repo<Channel>(db, ChannelDomainSettings.ChannelCollectionName);
var wsConfig = new WebSocketConnector();

DbInit.Init(mongoClient, dbName);
builder.Services.AddSingleton((sp) => db);
builder.Services.AddSingleton((sp) => new AppConfig { UserPasswordSalt = UserPasswordSalt, JwtSecret = JwtSecret });
builder.Services.AddSingleton((sp) => new RoleManager(db));
builder.Services.AddSingleton((sp) => new Repo<User>(db, UserDomainSettings.UserCollectionName));
builder.Services.AddSingleton((sp) => new Repo<Channel>(db, ChannelDomainSettings.ChannelCollectionName));
builder.Services.AddSingleton((sp) => new Repo<ChannelType>(db, ChannelDomainSettings.ChannelTypeCollectionName));
builder.Services.AddSingleton((sp) => new Repo<ChannelGroup>(db, ChannelDomainSettings.ChannelGroupCollectionName));
builder.Services.AddSingleton((sp) => new Repo<Config>(db, ConfigsDomainSettings.ConfigCollectionName));
builder.Services.AddSingleton((sp) => new Repo<Message>(db, MessagesDomainSettings.MessageCollectionName));
builder.Services.AddSingleton((sp) => new Repo<Role>(db, RolesDomainSettings.RoleCollectionName));
builder.Services.AddSingleton((sp) => new Repo<Workspace>(db, WorkspacesDomainSettings.WorkspaceCollectionName));
builder.Services.AddSingleton(sp => wsConfig);


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

builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddWebSockets(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(120); // Optional
});

var app = builder.Build();
wsConfig.Configure(app);

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
