----------------------------- MODULE Invariants ------------------------------
EXTENDS State, Install, Drafts

UniqueIds(rows) == Cardinality(IdsOf(rows)) = Cardinality(rows)
UniqueUserRoleKeys(s) ==
  \A a, b \in s.userRoles : a = b \/ ~UserRoleSameKey(a, b)

UniqueNicknames(s) ==
  \A a, b \in s.users :
    a.id = b.id \/ IdentityKey(a.nickname) # IdentityKey(b.nickname)

UniqueEmails(s) ==
  \A a, b \in s.users : a.id = b.id \/ a.email # b.email

Uniqueness(s) ==
  /\ UniqueIds(s.roles) /\ UniqueIds(s.users) /\ UniqueIds(s.workspaces)
  /\ UniqueIds(s.channelTypes) /\ UniqueIds(s.channels)
  /\ UniqueIds(s.channelGroups) /\ UniqueIds(s.messages)
  /\ UniqueIds(s.drafts) /\ UniqueIds(s.attachments)
  /\ UniqueIds(s.urlPreviews) /\ UniqueIds(s.configs)
  /\ UniqueUserRoleKeys(s) /\ UniqueNicknames(s) /\ UniqueEmails(s)

DeclaredReferentialIntegrity(s) ==
  /\ \A u \in s.users : RoleExists(s, u.roleId)
  /\ \A r \in s.userRoles :
       UserExists(s, r.userId) /\ RoleExists(s, r.roleId) /\
       (r.channelId = 0 \/ ChannelExists(s, r.channelId)) /\
       (r.channelTypeId = 0 \/ ChannelTypeExists(s, r.channelTypeId)) /\
       (r.workspaceId = 0 \/ WorkspaceExists(s, r.workspaceId))
  /\ \A m \in s.messages :
       ChannelExists(s, m.channelId) /\ UserExists(s, m.userId) /\
       (m.parentId = 0 \/ MessageExists(s, m.parentId)) /\
       (m.replyTo = 0 \/ MessageExists(s, m.replyTo))
  /\ \A r \in s.reactions :
       MessageExists(s, r.messageId) /\ UserExists(s, r.userId)
  /\ \A d \in s.drafts :
       ChannelExists(s, d.channelId) /\ UserExists(s, d.userId) /\
       (d.parentId = 0 \/ DraftExists(s, d.parentId))
  /\ \A a \in s.attachments : UserExists(s, a.userId)

StrongDomainReferences(s) ==
  /\ \A t \in s.channelTypes : RoleExists(s, t.baseRoleId)
  /\ \A c \in s.channels :
       WorkspaceExists(s, c.workspaceId) /\ ChannelTypeExists(s, c.channelTypeId)
  /\ \A g \in s.channelGroups :
       WorkspaceExists(s, g.workspaceId) /\
       \A id \in g.channelIds :
         ChannelExists(s, id) /\
         \A c \in ChannelsById(s, id) : c.workspaceId = g.workspaceId
  /\ \A m \in s.messages :
       (\A id \in m.attachmentIds : ExistsId(s.attachments, id)) /\
       (\A id \in m.urlPreviewIds : ExistsId(s.urlPreviews, id))
  /\ \A d \in s.drafts :
       \A id \in d.attachmentIds : ExistsId(s.attachments, id)

ThreadCountsExact(s) ==
  \A p \in s.messages :
    p.childrenCount = Cardinality({m \in s.messages : m.parentId = p.id})

DraftLogicalUniqueness(s) ==
  \A a, b \in s.drafts : a.id = b.id \/ ~DraftSameLogicalKey(a, b)

ReactionSetCanonical(s) ==
  Cardinality(s.reactions) =
  Cardinality({<<r.messageId, r.userId, r.code>> : r \in s.reactions})

AttachmentOwnership(s) ==
  /\ \A m \in s.messages :
       \A id \in m.attachmentIds :
         \E a \in s.attachments : a.id = id /\ a.userId = m.userId
  /\ \A d \in s.drafts :
       \A id \in d.attachmentIds :
         \E a \in s.attachments : a.id = id /\ a.userId = d.userId

MatchingEvent(a, e) ==
  a.kind = e.kind /\ a.operation = e.operation /\
  a.entityId = e.entityId /\ a.actorId = e.actorId

EventPayloadAndDestinationWellFormed(e) ==
  /\ e.payloadEntityId = e.entityId
  /\ IF e.kind = "userRole"
     THEN e.destination = "targetUser" /\ e.targetUserId = e.entityId
     ELSE e.destination = "broadcast" /\ e.targetUserId = 0

EventCorrespondence(s) ==
  /\ \A i \in 1..Len(s.audits) :
       ~s.audits[i].eventExpected \/
       \E j \in 1..Len(s.events) : MatchingEvent(s.audits[i], s.events[j])
  /\ \A j \in 1..Len(s.events) :
       EventPayloadAndDestinationWellFormed(s.events[j]) /\
       \E i \in 1..Len(s.audits) :
         s.audits[i].eventExpected /\ MatchingEvent(s.audits[i], s.events[j])

PermissionSafety(s) ==
  \A i \in 1..Len(s.audits) :
    ~s.audits[i].succeeded \/ s.audits[i].requiredPermission = NoPermission \/
    AllMighty \in s.audits[i].prePermissions \/
    s.audits[i].requiredPermission \in s.audits[i].prePermissions

SameInstalledData(a, b) ==
  /\ a.schemaInstalled = b.schemaInstalled /\ a.roles = b.roles
  /\ a.users = b.users /\ a.userRoles = b.userRoles
  /\ a.workspaces = b.workspaces /\ a.channelTypes = b.channelTypes
  /\ a.channels = b.channels /\ a.channelGroups = b.channelGroups
  /\ a.messages = b.messages /\ a.reactions = b.reactions
  /\ a.drafts = b.drafts /\ a.attachments = b.attachments
  /\ a.urlPreviews = b.urlPreviews /\ a.configs = b.configs

InstallDataIdempotent(s) ==
  SameInstalledData(ApplyInstall(s), ApplyInstall(ApplyInstall(s)))

CoreSafety(s) ==
  /\ Uniqueness(s) /\ DeclaredReferentialIntegrity(s)
  /\ DraftLogicalUniqueness(s) /\ ReactionSetCanonical(s)
  /\ PermissionSafety(s) /\ EventCorrespondence(s)

=============================================================================
