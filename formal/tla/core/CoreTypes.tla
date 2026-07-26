------------------------------ MODULE CoreTypes ------------------------------
EXTENDS Integers, FiniteSets, Sequences, TLC

\* The executable universe matches the Quint abstraction.  Zero is SQL NULL.
Ids == 1..8
UserIds == 1..4
Contents == {"", "hello", "edited", "reply"}
ReactionCodes == {"like", "heart"}
ValidNicknames ==
  {"test_user_1", "TEST_USER_1", "test_user_2", "TEST_USER_2",
   "new_user", "NEW_USER", "admin2", "ADMIN2"}

IdentityKey(v) ==
  CASE v = "TEST_USER_1" -> "test_user_1"
    [] v = "TEST_USER_2" -> "test_user_2"
    [] v = "NEW_USER" -> "new_user"
    [] v = "ADMIN2" -> "admin2"
    [] v = "TEST1@TEST.COM" -> "test1@test.com"
    [] v = "TEST2@TEST.COM" -> "test2@test.com"
    [] OTHER -> v

ValidNickname(v) == v \in ValidNicknames

Role(id, name, protected, permissions) ==
  [id |-> id, name |-> name, isProtected |-> protected,
   permissions |-> permissions]

User(id, nickname, email, roleId, passwordHash) ==
  [id |-> id, nickname |-> nickname, fullName |-> "", active |-> TRUE,
   verified |-> FALSE, banned |-> FALSE, muted |-> FALSE, email |-> email,
   avatarUrl |-> "", roleId |-> roleId, passwordHash |-> passwordHash]

UserRole(userId, workspaceId, channelTypeId, channelId, roleId, allow, deny) ==
  [userId |-> userId, workspaceId |-> workspaceId,
   channelTypeId |-> channelTypeId, channelId |-> channelId,
   roleId |-> roleId, allowlist |-> allow, denylist |-> deny]

Workspace(id, name) == [id |-> id, name |-> name]
ChannelType(id, name, key, baseRoleId) ==
  [id |-> id, name |-> name, key |-> key, baseRoleId |-> baseRoleId]

Channel(id, name, channelTypeId, workspaceId) ==
  [id |-> id, name |-> name, channelTypeId |-> channelTypeId,
   workspaceId |-> workspaceId]

ChannelGroup(id, name, workspaceId, channelIds, ordering) ==
  [id |-> id, name |-> name, workspaceId |-> workspaceId,
   channelIds |-> channelIds, ordering |-> ordering]

Message(id, channelId, userId, content, attachmentIds, previewIds,
        status, parentId, childrenCount, replyTo) ==
  [id |-> id, channelId |-> channelId, userId |-> userId, content |-> content,
   attachmentIds |-> attachmentIds, urlPreviewIds |-> previewIds,
   status |-> status, parentId |-> parentId, childrenCount |-> childrenCount,
   replyTo |-> replyTo]

Draft(id, channelId, userId, content, attachmentIds, parentId) ==
  [id |-> id, channelId |-> channelId, userId |-> userId, content |-> content,
   attachmentIds |-> attachmentIds, parentId |-> parentId]

Attachment(id, userId, mimeType, size) ==
  [id |-> id, userId |-> userId, urlPath |-> "attachments/generated",
   originalFileName |-> "generated.bin", extension |-> "bin",
   mimeType |-> mimeType, size |-> size, width |-> 0, height |-> 0,
   duration |-> 0, attachmentType |-> "input", asFile |-> TRUE]

Preview(id, url) == [id |-> id, url |-> url]
Config(id, name, value, configType) ==
  [id |-> id, name |-> name, configValue |-> value, configType |-> configType]

SetField(r, field, value) == [r EXCEPT ![field] = value]
IdsOf(rows) == {r.id : r \in rows}
RowsById(rows, id) == {r \in rows : r.id = id}
ExistsId(rows, id) == \E r \in rows : r.id = id
ReplaceById(rows, row) == {r \in rows : r.id # row.id} \union {row}

=============================================================================
