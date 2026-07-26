-------------------------------- MODULE State --------------------------------
EXTENDS CoreTypes, Permissions

OK == "success"
Installed == "installed"
Failed == "failed"
Forbidden == "FORBIDDEN_ACTION"
NotFound == "NOT_FOUND"
InvalidNickname == "INVALID_NICKNAME"
NicknameTaken == "NICKNAME_TAKEN"
InvalidChannelOrUser == "Channel or user is invalid"
FileEmpty == "File is empty."
WrongMessageId == "wrong message id"
ChannelNotFound == "channel not found"
ReactionNotFound == "reaction not found"
DbConstraint == "database constraint"
FalseResult == "false"
NullResult == "null"
NoResult == "none"
NoPermission == ""

EmptyState ==
  [schemaInstalled |-> FALSE, roles |-> {}, users |-> {}, userRoles |-> {},
   workspaces |-> {}, channelTypes |-> {}, channels |-> {}, channelGroups |-> {},
   messages |-> {}, reactions |-> {}, drafts |-> {}, attachments |-> {},
   urlPreviews |-> {}, configs |-> {}, issuedTokens |-> {},
   events |-> <<>>, audits |-> <<>>, lastResult |-> NoResult]

FailedState(s, result) == [s EXCEPT !.lastResult = result]
IsAuthenticated(s, userId) == ExistsId(s.users, userId)
RoleExists(s, id) == ExistsId(s.roles, id)
UserExists(s, id) == ExistsId(s.users, id)
WorkspaceExists(s, id) == ExistsId(s.workspaces, id)
ChannelTypeExists(s, id) == ExistsId(s.channelTypes, id)
ChannelExists(s, id) == ExistsId(s.channels, id)
MessageExists(s, id) == ExistsId(s.messages, id)
DraftExists(s, id) == ExistsId(s.drafts, id)
ConfigExists(s, id) == ExistsId(s.configs, id)

Only(rows) == CHOOSE r \in rows : TRUE
RoleById(s, id) == RowsById(s.roles, id)
UsersById(s, id) == RowsById(s.users, id)
ChannelsById(s, id) == RowsById(s.channels, id)
MessagesById(s, id) == RowsById(s.messages, id)
DraftsById(s, id) == RowsById(s.drafts, id)

ReplaceRole(s, row) == [s EXCEPT !.roles = ReplaceById(@, row)]
ReplaceUser(s, row) == [s EXCEPT !.users = ReplaceById(@, row)]
ReplaceWorkspace(s, row) == [s EXCEPT !.workspaces = ReplaceById(@, row)]
ReplaceChannelType(s, row) == [s EXCEPT !.channelTypes = ReplaceById(@, row)]
ReplaceChannel(s, row) == [s EXCEPT !.channels = ReplaceById(@, row)]
ReplaceChannelGroup(s, row) == [s EXCEPT !.channelGroups = ReplaceById(@, row)]
ReplaceMessage(s, row) == [s EXCEPT !.messages = ReplaceById(@, row)]
ReplaceDraft(s, row) == [s EXCEPT !.drafts = ReplaceById(@, row)]
ReplaceConfig(s, row) == [s EXCEPT !.configs = ReplaceById(@, row)]

UserRoleSameKey(a, b) ==
  /\ a.userId = b.userId /\ a.channelId = b.channelId
  /\ a.channelTypeId = b.channelTypeId /\ a.workspaceId = b.workspaceId

UserRoleExists(s, row) == \E old \in s.userRoles : UserRoleSameKey(old, row)
ReplaceUserRole(s, row) ==
  [s EXCEPT !.userRoles =
    {old \in @ : ~UserRoleSameKey(old, row)} \union {row}]

IsNicknameTaken(s, nickname, exceptId) ==
  \E u \in s.users :
    u.id # exceptId /\ IdentityKey(u.nickname) = IdentityKey(nickname)

IsEmailTaken(s, email, exceptId) ==
  \E u \in s.users : u.id # exceptId /\ u.email = email

Children(messages, ids) ==
  {m.id : m \in {x \in messages : x.parentId \in ids}}

Reach8(messages, ids) ==
  LET i1 == ids \union Children(messages, ids)
      i2 == i1 \union Children(messages, i1)
      i3 == i2 \union Children(messages, i2)
      i4 == i3 \union Children(messages, i3)
      i5 == i4 \union Children(messages, i4)
      i6 == i5 \union Children(messages, i5)
      i7 == i6 \union Children(messages, i6)
  IN i7 \union Children(messages, i7)

DraftChildren(drafts, ids) ==
  {d.id : d \in {x \in drafts : x.parentId \in ids}}

DraftReach8(drafts, ids) ==
  LET i1 == ids \union DraftChildren(drafts, ids)
      i2 == i1 \union DraftChildren(drafts, i1)
      i3 == i2 \union DraftChildren(drafts, i2)
      i4 == i3 \union DraftChildren(drafts, i3)
      i5 == i4 \union DraftChildren(drafts, i4)
      i6 == i5 \union DraftChildren(drafts, i5)
      i7 == i6 \union DraftChildren(drafts, i6)
  IN i7 \union DraftChildren(drafts, i7)

RemoveMessages(s, initialIds) ==
  LET removed == Reach8(s.messages, initialIds)
      remaining ==
        {IF m.replyTo \in removed THEN [m EXCEPT !.replyTo = 0] ELSE m :
         m \in {x \in s.messages : x.id \notin removed}}
  IN [s EXCEPT
       !.messages = remaining,
       !.reactions = {r \in @ : r.messageId \notin removed}]

RemoveDrafts(s, initialIds) ==
  LET removed == DraftReach8(s.drafts, initialIds)
  IN [s EXCEPT !.drafts = {d \in @ : d.id \notin removed}]

RecordMutation(before, after, kind, operation, entityId, actorId,
               permission, eventExpected, prePermissions, payloadContextId) ==
  LET audit ==
        [kind |-> kind, operation |-> operation, entityId |-> entityId,
         actorId |-> actorId, requiredPermission |-> permission,
         prePermissions |-> prePermissions, succeeded |-> TRUE,
         eventExpected |-> eventExpected]
      destination == IF kind = "userRole" THEN "targetUser" ELSE "broadcast"
      target == IF kind = "userRole" THEN entityId ELSE 0
      event ==
        [kind |-> kind, operation |-> operation, entityId |-> entityId,
         actorId |-> actorId, destination |-> destination,
         targetUserId |-> target, payloadEntityId |-> entityId,
         payloadContextId |-> payloadContextId]
  IN [after EXCEPT
       !.audits = Append(@, audit),
       !.events = IF eventExpected THEN Append(@, event) ELSE @,
       !.lastResult = OK]

RecordGlobal(before, after, kind, operation, entityId, actorId,
             permission, eventExpected, context) ==
  RecordMutation(before, after, kind, operation, entityId, actorId,
    permission, eventExpected, EffectivePermissions(before, actorId, 0, 0, 0),
    context)

RecordScoped(before, after, kind, operation, entityId, actorId,
             permission, eventExpected, workspaceId, channelTypeId, channelId,
             context) ==
  RecordMutation(before, after, kind, operation, entityId, actorId,
    permission, eventExpected,
    EffectivePermissions(before, actorId, workspaceId, channelTypeId, channelId),
    context)

RecordUnpermissioned(before, after, kind, operation, entityId, actorId,
                     eventExpected, context) ==
  RecordMutation(before, after, kind, operation, entityId, actorId,
    NoPermission, eventExpected, {}, context)

=============================================================================
