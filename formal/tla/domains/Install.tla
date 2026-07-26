------------------------------- MODULE Install -------------------------------
EXTENDS State

AdminRole == Role(1, "admin", TRUE, DefaultAdminPermissions)
ModeratorRole == Role(2, "moderator", TRUE, DefaultModeratorPermissions)
DefaultUserRole == Role(3, "user", TRUE, DefaultUserPermissions)
SeededAdmin ==
  [User(1, "test_user_1", "test1@test.com", 1, "hash123") EXCEPT
   !.fullName = "Test User 1", !.active = FALSE]

SeededUser ==
  [User(2, "test_user_2", "test2@test.com", 3, "hash123") EXCEPT
   !.fullName = "Test User 2", !.active = FALSE]

SeedWorkspaces == {Workspace(1, "Main"), Workspace(2, "Secondary")}
SeedChannelTypes ==
  {ChannelType(1, "public", "public", 3),
   ChannelType(2, "private", "private", 3)}

SeedChannels ==
  {Channel(1, "public 1", 1, 1), Channel(2, "public 2", 1, 1),
   Channel(3, "private 1", 2, 1), Channel(4, "private 2", 2, 1)}

SeedConfigs ==
  {Config(1, "settings.NewUserDefaultRole", "3", "int"),
   Config(2, "messages.sendCooldown", "600", "int"),
   Config(3, "events.typesEnabled",
          "message.sent,message.edited,message.deleted", "string[]")}

ApplyInstall(s) ==
  IF \E r \in s.roles : r.name = "admin"
  THEN [s EXCEPT !.schemaInstalled = TRUE, !.lastResult = Installed]
  ELSE IF /\ (\E r \in s.roles : r.id \in {1,2,3})
          \/ (\E u \in s.users : u.id \in {1,2})
          \/ (\E w \in s.workspaces : w.id \in {1,2})
          \/ (\E t \in s.channelTypes : t.id \in {1,2})
          \/ (\E c \in s.channels : c.id \in {1,2,3,4})
          \/ (\E c \in s.configs : c.id \in {1,2,3})
       THEN FailedState([s EXCEPT !.schemaInstalled = TRUE], Failed)
       ELSE [s EXCEPT
              !.schemaInstalled = TRUE,
              !.roles = @ \union {AdminRole, ModeratorRole, DefaultUserRole},
              !.users = @ \union {SeededAdmin, SeededUser},
              !.workspaces = @ \union SeedWorkspaces,
              !.channelTypes = @ \union SeedChannelTypes,
              !.channels = @ \union SeedChannels,
              !.configs = @ \union SeedConfigs,
              !.lastResult = OK]

ApplyUninstall(s) == [EmptyState EXCEPT !.lastResult = OK]

=============================================================================
