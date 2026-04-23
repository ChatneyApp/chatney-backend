using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Configs;
using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Domains.Attachments;
using ChatneyBackend.Domains.DraftMessages;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Domains.Workspaces;
using HotChocolate.AspNetCore;
using ChatneyBackend.Setup;
using ChatneyBackend.Utils;
using ChatneyBackend.Infra.Middleware;
using ChannelDomainSettings = ChatneyBackend.Domains.Channels.DomainSettings;
using ConfigsDomainSettings = ChatneyBackend.Domains.Configs.DomainSettings;
using UsersDomainSettings = ChatneyBackend.Domains.Users.DomainSettings;
using MessagesDomainSettings = ChatneyBackend.Domains.Messages.DomainSettings;
using AttachmentsDomainSettings = ChatneyBackend.Domains.Attachments.DomainSettings;
using DraftMessagesDomainSettings = ChatneyBackend.Domains.DraftMessages.DomainSettings;
using WorkspacesDomainSettings = ChatneyBackend.Domains.Workspaces.DomainSettings;
using Microsoft.AspNetCore.WebSockets;
using Amazon.Runtime;
using Amazon.S3;
using ChatneyBackend.Infra.Migrations;
using ChatneyBackend.Infra;
using RepoDb;
using FluentMigrator.Runner;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var postgresConnectionString = builder.Configuration.GetConnectionString("Postgres");
var dbName = builder.Configuration.GetConnectionString("dbName");
var userPasswordSalt = builder.Configuration.GetSection("UserPasswordSalt").Value;
var jwtSecret = builder.Configuration.GetSection("JwtSecret").Value;

if (postgresConnectionString == null || dbName == null || userPasswordSalt == null || jwtSecret == null)
{
    throw new ArgumentException("App settings are invalid");
}

GlobalConfiguration.Setup().UsePostgreSql();
var dataSourceBuilder = new NpgsqlDataSourceBuilder(postgresConnectionString);
dataSourceBuilder.EnableParameterLogging();
dataSourceBuilder.EnableDynamicJson();
var pgDataSource = dataSourceBuilder.Build();

await using (var pgConnection = await pgDataSource.OpenConnectionAsync())
{
    var ping = await pgConnection.ExecuteScalarAsync<int>("SELECT 1");
    if (ping == 1)
    {
        Console.WriteLine("pg connection is OK");
    }
}

var wsConfig = new WebSocketConnector();

// var bucket = builder.Configuration.GetSection("AWS").GetValue<string>("Bucket");
// Console.WriteLine(bucket);

var appRepos = new AppRepos(
    users: new PgRepo<User, Guid>(pgDataSource, UsersDomainSettings.UserTableName),
    userRoles: new PgRepo<UserRole, UserRoleKey>(pgDataSource, UsersDomainSettings.UserRoleTableName),
    roles: new PgRepo<Role, int>(pgDataSource, "roles"),
    messages: new PgRepo<Message, int>(pgDataSource, MessagesDomainSettings.MessageTableName),
    draftMessages: new PgRepo<DraftMessage, int>(pgDataSource, DraftMessagesDomainSettings.MessageTableName),
    reactions: new PgRepo<MessageReaction, MessageReactionKey>(pgDataSource, MessagesDomainSettings.ReactionTableName),
    attachments: new PgRepo<Attachment, int>(pgDataSource, AttachmentsDomainSettings.AttachmentTableName),
    urlPreviews: new PgRepo<UrlPreview, int>(pgDataSource, MessagesDomainSettings.UrlPreviewTableName),
    channels: new PgRepo<Channel, int>(pgDataSource, ChannelDomainSettings.ChannelTableName),
    channelTypes: new PgRepo<ChannelType, int>(pgDataSource, ChannelDomainSettings.ChannelTypeTableName),
    channelGroups: new PgRepo<ChannelGroup, int>(pgDataSource, ChannelDomainSettings.ChannelGroupTableName),
    configs: new PgRepo<Config, int>(pgDataSource, ConfigsDomainSettings.ConfigTableName),
    workspaces: new PgRepo<Workspace, int>(pgDataSource, WorkspacesDomainSettings.WorkspaceTableName)
);

// Database
builder.Services.AddSingleton(pgDataSource);
builder.Services.AddSingleton(_ => new AppConfig { UserPasswordSalt = userPasswordSalt, JwtSecret = jwtSecret });
builder.Services.AddSingleton(_ => appRepos);
builder.Services.AddSingleton(_ => new RoleManager(appRepos.Roles));
builder.Services
    .AddFluentMigratorCore()
    .ConfigureRunner(runner => runner
        .AddPostgres()
        .WithGlobalConnectionString(postgresConnectionString)
        .ScanIn(typeof(Program).Assembly).For.Migrations())
        .ConfigureRunner(rb => rb.WithVersionTable(new CustomVersionTable()));
// WebSocket
builder.Services.AddSingleton(_ => wsConfig);
// AWS S3 setup
var awsOptions = builder.Configuration.GetAWSOptions();
builder.Services.AddDefaultAWSOptions(awsOptions);
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();

    var serviceUrl = cfg["AWS:ServiceUrl"]!;
    var forcePathStyle = bool.Parse(cfg["AWS:ForcePathStyle"] ?? "false");

    var accessKey = cfg["AWS:AccessKey"] ?? "";
    var secretKey = cfg["AWS:SecretKey"] ?? "";

    if (string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey))
        throw new InvalidOperationException("S3 credentials are not configured.");

    var s3Config = new AmazonS3Config
    {
        ServiceURL = serviceUrl,
        ForcePathStyle = forcePathStyle
    };

    return new AmazonS3Client(new BasicAWSCredentials(accessKey, secretKey), s3Config);
});
builder.Services.AddControllers();

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
    .AddType<UploadType>();


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
