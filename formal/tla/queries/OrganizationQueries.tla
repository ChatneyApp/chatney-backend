------------------------- MODULE OrganizationQueries -------------------------
EXTENDS State

CanReadWorkspace(s, actor, id) ==
  WorkspaceExists(s, id) /\ IsAuthenticated(s, actor)
  /\ CanWorkspace(s, actor, id, WorkspaceRead)

RawScopedRoleCanReadMessages(s, row) ==
  ChannelReadMessage \in RolePermissions(s, row.roleId)

\* This query intentionally ignores global roles and allow/deny precedence.
PermittedChannelIds(s, userId) ==
  {c.id : c \in {x \in s.channels :
    \E r \in s.userRoles :
      r.userId = userId /\ RawScopedRoleCanReadMessages(s, r) /\
      (r.channelId = x.id \/ r.channelTypeId = x.channelTypeId \/
       r.workspaceId = x.workspaceId)}}

VisibleWorkspaceIds(s, actor) ==
  IF ~IsAuthenticated(s, actor) THEN {}
  ELSE IF CanGlobal(s, actor, ChannelReadMessage) THEN IdsOf(s.workspaces)
  ELSE {c.workspaceId : c \in {x \in s.channels :
         x.id \in PermittedChannelIds(s, actor) /\
         WorkspaceExists(s, x.workspaceId)}}

VisibleChannelIds(s, actor, workspaceId) ==
  IF ~WorkspaceExists(s, workspaceId) \/ ~IsAuthenticated(s, actor) THEN {}
  ELSE IF CanGlobal(s, actor, ChannelReadMessage)
       THEN {c.id : c \in {x \in s.channels : x.workspaceId = workspaceId}}
       ELSE PermittedChannelIds(s, actor) \intersect
            {c.id : c \in {x \in s.channels : x.workspaceId = workspaceId}}

CanReadChannel(s, actor, id) ==
  ChannelExists(s, id) /\ IsAuthenticated(s, actor) /\
  \A c \in ChannelsById(s, id) : CanChannel(s, actor, c, ChannelRead)

CanReadMessages(s, actor, id) ==
  ChannelExists(s, id) /\ IsAuthenticated(s, actor) /\
  \A c \in ChannelsById(s, id) : CanChannel(s, actor, c, ChannelReadMessage)

VisibleDraftIds(s, actor) ==
  {d.id : d \in {x \in s.drafts : x.userId = actor /\
    ChannelExists(s, x.channelId) /\
    \A c \in ChannelsById(s, x.channelId) :
      CanChannel(s, actor, c, ChannelRead)}}

=============================================================================
