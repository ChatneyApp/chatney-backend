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
var userPasswordSalt = builder.Configuration.GetSection("UserPasswordSalt").Value;
var jwtSecret = builder.Configuration.GetSection("JwtSecret").Value;

if (connectionString == null || dbName == null || userPasswordSalt == null || jwtSecret == null)
{
    throw new ArgumentException("App settings are invalid");
}

var url = new MongoUrl(connectionString);
var settings = MongoClientSettings.FromUrl(url);
var mongoClient = new MongoClient(settings);
var db = mongoClient.GetDatabase(dbName);

var wsConfig = new WebSocketConnector();

DbInit.Init(mongoClient, dbName);
builder.Services.AddSingleton(_ => db);
builder.Services.AddSingleton(_ => new AppConfig { UserPasswordSalt = userPasswordSalt, JwtSecret = jwtSecret });
builder.Services.AddSingleton(_ => new RoleManager(db));
builder.Services.AddSingleton(_ => new Repo<User>(db, UserDomainSettings.UserCollectionName));
builder.Services.AddSingleton(_ => new Repo<Channel>(db, ChannelDomainSettings.ChannelCollectionName));
builder.Services.AddSingleton(_ => new Repo<ChannelType>(db, ChannelDomainSettings.ChannelTypeCollectionName));
builder.Services.AddSingleton(_ => new Repo<ChannelGroup>(db, ChannelDomainSettings.ChannelGroupCollectionName));
builder.Services.AddSingleton(_ => new Repo<Config>(db, ConfigsDomainSettings.ConfigCollectionName));
builder.Services.AddSingleton(_ => new Repo<Message>(db, MessagesDomainSettings.MessageCollectionName));
builder.Services.AddSingleton(_ => new Repo<MessageAttachment>(db, MessagesDomainSettings.MessageAttachmentCollectionName));
builder.Services.AddSingleton(_ => new Repo<Role>(db, RolesDomainSettings.RoleCollectionName));
builder.Services.AddSingleton(_ => new Repo<Workspace>(db, WorkspacesDomainSettings.WorkspaceCollectionName));
builder.Services.AddSingleton(_ => wsConfig);


// ---- CORS policies ----
// Dev: open for local tooling; Prod: strict allow-list with credentials.
const string devOpenCors = "DevOpenCors";
const string prodCors = "ProdCors";

string[] allowedProdOrigins =
[
    "http://localhost:3001",
    "https://chatney.com"
];

builder.Services.AddCors(options =>
{
    options.AddPolicy(devOpenCors, policy =>
    {
        // Do NOT call AllowCredentials with AllowAnyOrigin.
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });

    options.AddPolicy(prodCors, policy =>
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
    .AddMutationType<Mutation>()
    .AddTypeExtension<HasUserIdTypeExtension<Message>>()
    .AddDataLoader<UserByIdDataLoader>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddWebSockets(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(120); // Optional
});

var app = builder.Build();
wsConfig.Configure(app);

app.UseMiddleware<AuthMiddleware>();

app.UseCors(app.Environment.IsDevelopment() ? devOpenCors : prodCors);
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
