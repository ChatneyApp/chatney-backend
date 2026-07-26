---------------------------- MODULE Organization -----------------------------
EXTENDS State

ApplyAddWorkspace(s, actor, row) ==
  IF ~IsAuthenticated(s, actor) \/ ~CanGlobal(s, actor, WorkspaceCreate)
  THEN FailedState(s, Forbidden)
  ELSE IF WorkspaceExists(s, row.id) THEN FailedState(s, DbConstraint)
  ELSE RecordGlobal(s, [s EXCEPT !.workspaces = @ \union {row}],
         "workspace", "create", row.id, actor, WorkspaceCreate, TRUE, 0)

ApplyUpdateWorkspace(s, actor, row) ==
  IF ~IsAuthenticated(s, actor) \/ ~CanWorkspace(s, actor, row.id, WorkspaceUpdate)
  THEN FailedState(s, Forbidden)
  ELSE IF ~WorkspaceExists(s, row.id) THEN FailedState(s, NullResult)
  ELSE RecordScoped(s, ReplaceWorkspace(s, row), "workspace", "update",
         row.id, actor, WorkspaceUpdate, TRUE, row.id, 0, 0, 0)

ApplyDeleteWorkspace(s, actor, id) ==
  IF ~WorkspaceExists(s, id) THEN FailedState(s, FalseResult)
  ELSE IF ~IsAuthenticated(s, actor) \/ ~CanWorkspace(s, actor, id, WorkspaceDelete)
       THEN FailedState(s, Forbidden)
  ELSE LET after == [s EXCEPT
          !.workspaces = {w \in @ : w.id # id},
          !.userRoles = {r \in @ : r.workspaceId # id}]
       IN RecordScoped(s, after, "workspace", "delete", id, actor,
            WorkspaceDelete, TRUE, id, 0, 0, 0)

ApplyAddChannelType(s, actor, row) ==
  IF ~IsAuthenticated(s, actor) \/ ~CanGlobal(s, actor, ChannelCreate)
  THEN FailedState(s, Forbidden)
  ELSE IF ChannelTypeExists(s, row.id) THEN FailedState(s, DbConstraint)
  ELSE RecordGlobal(s, [s EXCEPT !.channelTypes = @ \union {row}],
         "channelType", "create", row.id, actor, ChannelCreate, TRUE, 0)

ApplyUpdateChannelType(s, actor, row) ==
  IF ~IsAuthenticated(s, actor) \/ ~CanChannelType(s, actor, row.id, ChannelEdit)
  THEN FailedState(s, Forbidden)
  ELSE IF ~ChannelTypeExists(s, row.id) THEN FailedState(s, NullResult)
  ELSE RecordScoped(s, ReplaceChannelType(s, row), "channelType", "update",
         row.id, actor, ChannelEdit, TRUE, 0, row.id, 0, 0)

ApplyDeleteChannelType(s, actor, id) ==
  IF ~ChannelTypeExists(s, id) THEN FailedState(s, FalseResult)
  ELSE IF ~IsAuthenticated(s, actor) \/
          ~CanChannelType(s, actor, id, ChannelTypeDelete)
       THEN FailedState(s, Forbidden)
  ELSE LET after == [s EXCEPT
          !.channelTypes = {t \in @ : t.id # id},
          !.userRoles = {r \in @ : r.channelTypeId # id}]
       IN RecordScoped(s, after, "channelType", "delete", id, actor,
            ChannelTypeDelete, TRUE, 0, id, 0, 0)

ApplyAddChannel(s, actor, row) ==
  IF ~WorkspaceExists(s, row.workspaceId) THEN FailedState(s, NotFound)
  ELSE IF ~IsAuthenticated(s, actor) \/
          ~CanWorkspace(s, actor, row.workspaceId, ChannelCreate)
       THEN FailedState(s, Forbidden)
  ELSE IF ChannelExists(s, row.id) THEN FailedState(s, DbConstraint)
  ELSE RecordScoped(s, [s EXCEPT !.channels = @ \union {row}],
         "channel", "create", row.id, actor, ChannelCreate, TRUE,
         row.workspaceId, 0, 0, row.workspaceId)

ApplyUpdateChannel(s, actor, row) ==
  IF ~IsAuthenticated(s, actor) \/ ~CanChannel(s, actor, row, ChannelEdit)
  THEN FailedState(s, Forbidden)
  ELSE IF ~ChannelExists(s, row.id) THEN FailedState(s, NullResult)
  ELSE RecordScoped(s, ReplaceChannel(s, row), "channel", "update",
         row.id, actor, ChannelEdit, TRUE, row.workspaceId,
         row.channelTypeId, row.id, row.workspaceId)

ApplyDeleteChannel(s, actor, id) ==
  IF ~ChannelExists(s, id) THEN FailedState(s, FalseResult)
  ELSE LET channel == Only(ChannelsById(s, id))
       IN IF ~IsAuthenticated(s, actor) \/
             ~CanChannel(s, actor, channel, ChannelDelete)
          THEN FailedState(s, Forbidden)
          ELSE LET mids ==
                     {m.id : m \in {x \in s.messages : x.channelId = id}}
                   dids ==
                     {d.id : d \in {x \in s.drafts : x.channelId = id}}
                   cascaded == RemoveDrafts(RemoveMessages(s, mids), dids)
                   after == [cascaded EXCEPT
                     !.channels = {c \in @ : c.id # id},
                     !.userRoles = {r \in @ : r.channelId # id}]
               IN RecordScoped(s, after, "channel", "delete", id, actor,
                    ChannelDelete, TRUE, channel.workspaceId,
                    channel.channelTypeId, id, channel.workspaceId)

ApplyAddChannelGroup(s, actor, row) ==
  IF ~WorkspaceExists(s, row.workspaceId) THEN FailedState(s, NotFound)
  ELSE IF ~IsAuthenticated(s, actor) \/
          ~CanWorkspace(s, actor, row.workspaceId, ChannelGroupAdd)
       THEN FailedState(s, Forbidden)
  ELSE IF ExistsId(s.channelGroups, row.id) THEN FailedState(s, DbConstraint)
  ELSE RecordScoped(s, [s EXCEPT !.channelGroups = @ \union {row}],
         "channelGroup", "create", row.id, actor, ChannelGroupAdd, FALSE,
         row.workspaceId, 0, 0, row.workspaceId)

ApplyUpdateChannelGroup(s, actor, row) ==
  IF ~WorkspaceExists(s, row.workspaceId) THEN FailedState(s, NullResult)
  ELSE IF ~IsAuthenticated(s, actor) \/
          ~CanWorkspace(s, actor, row.workspaceId, ChannelGroupEdit)
       THEN FailedState(s, Forbidden)
  ELSE IF ~ExistsId(s.channelGroups, row.id) THEN FailedState(s, NullResult)
  ELSE RecordScoped(s, ReplaceChannelGroup(s, row), "channelGroup", "update",
         row.id, actor, ChannelGroupEdit, FALSE, row.workspaceId, 0, 0,
         row.workspaceId)

ApplyDeleteChannelGroup(s, actor, id) ==
  IF ~ExistsId(s.channelGroups, id) THEN FailedState(s, FalseResult)
  ELSE LET row == Only(RowsById(s.channelGroups, id))
       IN IF ~WorkspaceExists(s, row.workspaceId) THEN FailedState(s, FalseResult)
          ELSE IF ~IsAuthenticated(s, actor) \/
                  ~CanWorkspace(s, actor, row.workspaceId, ChannelGroupDelete)
               THEN FailedState(s, Forbidden)
          ELSE RecordScoped(s,
                 [s EXCEPT !.channelGroups = {g \in @ : g.id # id}],
                 "channelGroup", "delete", id, actor, ChannelGroupDelete,
                 FALSE, row.workspaceId, 0, 0, row.workspaceId)

=============================================================================
