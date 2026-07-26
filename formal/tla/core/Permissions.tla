----------------------------- MODULE Permissions -----------------------------
EXTENDS CoreTypes

AllMighty == "AllMighty"
RoleCreate == "role.createRole"
RoleEdit == "role.editRole"
RoleDelete == "role.deleteRole"
UserCreate == "user.createUser"
UserEdit == "user.editUser"
UserDelete == "user.deleteUser"
UserRead == "user.readUser"
WorkspaceCreate == "workspace.createWorkspace"
WorkspaceUpdate == "workspace.updateWorkspace"
WorkspaceDelete == "workspace.deleteWorkspace"
WorkspaceRead == "workspace.readWorkspace"
ChannelCreateMessage == "channel.createMessage"
ChannelReadMessage == "channel.readMessage"
ChannelEditMessage == "channel.editMessage"
ChannelDeleteMessage == "channel.deleteMessage"
ChannelEditOwnMessage == "channel.editOwnMessage"
ChannelDeleteOwnMessage == "channel.deleteOwnMessage"
ChannelCreate == "channel.createChannel"
ChannelRead == "channel.readChannel"
ChannelEdit == "channel.editChannel"
ChannelDelete == "channel.deleteChannel"
ChannelTypeDelete == "channel.deleteChannelType"
ChannelGroupAdd == "channel.addChannelGroup"
ChannelGroupEdit == "channel.editChannelGroup"
ChannelGroupDelete == "channel.deleteChannelGroup"
AttachmentUpload == "attachment.upload"
AttachmentRead == "attachment.read"
AttachmentDelete == "attachment.delete"
ConfigRead == "config.readValue"
ConfigUpdate == "config.updateValue"

AllPermissions ==
  {AllMighty, RoleCreate, RoleEdit, RoleDelete,
   UserCreate, UserEdit, UserDelete, UserRead,
   WorkspaceCreate, WorkspaceUpdate, WorkspaceDelete, WorkspaceRead,
   ChannelCreateMessage, ChannelReadMessage, ChannelEditMessage,
   ChannelDeleteMessage, ChannelEditOwnMessage, ChannelDeleteOwnMessage,
   ChannelCreate, ChannelRead, ChannelEdit, ChannelDelete, ChannelTypeDelete,
   ChannelGroupAdd, ChannelGroupEdit, ChannelGroupDelete,
   AttachmentUpload, AttachmentRead, AttachmentDelete, ConfigRead, ConfigUpdate}

DefaultUserPermissions ==
  {ChannelCreateMessage, ChannelRead, ChannelReadMessage,
   ChannelEditOwnMessage, ChannelDeleteOwnMessage, WorkspaceRead,
   AttachmentUpload, AttachmentRead}

DefaultModeratorPermissions ==
  DefaultUserPermissions \union {UserRead, ChannelEditMessage, ChannelDeleteMessage}

DefaultAdminPermissions == AllPermissions

RolePermissions(s, roleId) ==
  UNION {r.permissions : r \in {x \in s.roles : x.id = roleId}}

PermissionsForUserRole(s, row) ==
  (RolePermissions(s, row.roleId) \union row.allowlist) \ row.denylist

\* Authorization chooses one row at the highest populated matching scope.
\* Returning a set of candidates preserves unspecified SQL/List.Find ordering.
EffectivePermissionCandidates(s, userId, workspaceId, channelTypeId, channelId) ==
  LET ur == {r \in s.userRoles : r.userId = userId}
      cr == IF channelId = 0 THEN {} ELSE {r \in ur : r.channelId = channelId}
      tr == IF channelTypeId = 0 THEN {}
            ELSE {r \in ur : r.channelTypeId = channelTypeId}
      wr == IF workspaceId = 0 THEN {}
            ELSE {r \in ur : r.workspaceId = workspaceId}
      global == {RolePermissions(s, u.roleId) :
                 u \in {x \in s.users : x.id = userId}}
  IN CASE cr # {} -> {PermissionsForUserRole(s, r) : r \in cr}
       [] tr # {} -> {PermissionsForUserRole(s, r) : r \in tr}
       [] wr # {} -> {PermissionsForUserRole(s, r) : r \in wr}
       [] OTHER -> global

EffectivePermissions(s, userId, workspaceId, channelTypeId, channelId) ==
  UNION EffectivePermissionCandidates(s, userId, workspaceId, channelTypeId, channelId)

Can(s, userId, workspaceId, channelTypeId, channelId, permission) ==
  \E p \in EffectivePermissionCandidates(
              s, userId, workspaceId, channelTypeId, channelId) :
    AllMighty \in p \/ permission \in p

CanGlobal(s, u, p) == Can(s, u, 0, 0, 0, p)
CanWorkspace(s, u, w, p) == Can(s, u, w, 0, 0, p)
CanChannelType(s, u, t, p) == Can(s, u, 0, t, 0, p)
CanChannel(s, u, c, p) == Can(s, u, c.workspaceId, c.channelTypeId, c.id, p)

VisibleWorkspacePermissions(s, u, w) ==
  EffectivePermissions(s, u, 0, 0, 0) \union
  UNION {PermissionsForUserRole(s, r) :
         r \in {x \in s.userRoles : x.userId = u /\ x.workspaceId = w}}

VisibleChannelTypePermissions(s, u, t) ==
  EffectivePermissions(s, u, 0, 0, 0) \union
  UNION {PermissionsForUserRole(s, r) :
         r \in {x \in s.userRoles : x.userId = u /\ x.channelTypeId = t}}

VisibleChannelPermissions(s, u, c) ==
  LET rows == {r \in s.userRoles : r.userId = u /\ r.channelId = c.id}
      base == EffectivePermissions(s, u, 0, 0, 0)
              \union VisibleWorkspacePermissions(s, u, c.workspaceId)
              \union VisibleChannelTypePermissions(s, u, c.channelTypeId)
      allow == UNION {RolePermissions(s, r.roleId) \union r.allowlist : r \in rows}
      deny == UNION {r.denylist : r \in rows}
  IN (base \union allow) \ deny

=============================================================================
