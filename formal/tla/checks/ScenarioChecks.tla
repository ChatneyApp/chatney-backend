--------------------------- MODULE ScenarioChecks ----------------------------
EXTENDS Integers, FiniteSets, Sequences, TLC, Install, Users, Roles,
        Organization, Messages, Drafts, Attachments, Configs, Invariants,
        UsersRolesQueries, OrganizationQueries, MessageQueries, SystemQueries

TestRole(id, name, permissions, protected) ==
  Role(id, name, protected, permissions)

TestUser(id, nickname, email, roleId) ==
  [User(id, nickname, email, roleId, "password") EXCEPT !.verified = TRUE]

TestMessage(id, userId, channelId, parentId, replyTo, attachments, previews, text) ==
  Message(id, channelId, userId, text, attachments, previews,
          "client-value-is-overwritten", parentId, 99, replyTo)

TestDraft(id, userId, channelId, parentId, attachments, text) ==
  Draft(id, channelId, userId, text, attachments, parentId)

InstalledState == ApplyInstall(EmptyState)

\* Behavioral scenarios (20)
ScopePrecedenceTest ==
  LET a == ApplyAddUserRole(InstalledState, 1,
             UserRole(2, 0, 1, 0, 1, {}, {}))
      b == ApplyAddUserRole(a, 1,
             UserRole(2, 0, 0, 1, 3, {}, {ChannelCreateMessage}))
      c == ApplyAddMessage(b, 2,
             TestMessage(5, 2, 1, 0, 0, {}, {}, "hello"), {})
  IN c.lastResult = Forbidden /\ ~MessageExists(c, 5)

UserRoleUniquenessTest ==
  LET row == UserRole(2, 1, 0, 0, 3, {}, {})
      a == ApplyAddUserRole(InstalledState, 1, row)
      b == ApplyAddUserRole(a, 1, row)
  IN a.lastResult = OK /\ b.lastResult = NotFound /\
     Cardinality(b.userRoles) = 1 /\ UniqueUserRoleKeys(b) /\
     Cardinality({i \in 1..Len(b.events) :
       b.events[i].kind = "userRole" /\
       b.events[i].destination = "targetUser" /\
       b.events[i].targetUserId = 2 /\
       b.events[i].payloadEntityId = 2 /\
       b.events[i].payloadContextId = 3}) = 1

ProtectedRoleIsMutableTest ==
  LET a == ApplyAddRole(InstalledState, 1, TestRole(5, "protected-custom", {}, TRUE))
      b == ApplyDeleteRole(a, 1, 5)
  IN b.lastResult = OK /\ ~RoleExists(b, 5)

PermissionEvidenceInvariantIsNonTautologicalTest ==
  LET forged == [kind |-> "workspace", operation |-> "create", entityId |-> 8,
        actorId |-> 2, requiredPermission |-> WorkspaceCreate,
        prePermissions |-> {}, succeeded |-> TRUE, eventExpected |-> FALSE]
  IN ~PermissionSafety(
       [InstalledState EXCEPT !.audits = Append(@, forged)])

DuplicateScopedRowsRemainSeparateCandidatesTest ==
  LET a == ApplyAddRole(InstalledState, 1, TestRole(5, "empty", {}, FALSE))
      b == ApplyAddUserRole(a, 1,
             UserRole(2, 1, 0, 1, 5, {ChannelRead}, {}))
      c == ApplyAddUserRole(b, 1,
             UserRole(2, 0, 1, 1, 5, {ChannelCreateMessage}, {}))
      candidates == EffectivePermissionCandidates(c, 2, 1, 1, 1)
  IN Cardinality(candidates) = 2 /\
     ~(\E p \in candidates :
       ChannelRead \in p /\ ChannelCreateMessage \in p)

DuplicateDefaultRoleConfigsAreArbitraryTest ==
  LET s == [InstalledState EXCEPT
        !.configs = @ \union
          {Config(5, "settings.NewUserDefaultRole", "1", "int")}]
      u == TestUser(3, "new_user", "new@test.com", 0)
      a == ApplyRegisterWithConfig(s, u, 1)
      b == ApplyRegisterWithConfig(s, u, 5)
  IN a.lastResult = OK /\ b.lastResult = OK /\
     Only(UsersById(a, 3)).roleId = 3 /\ Only(UsersById(b, 3)).roleId = 1

ReferentialCascadeTest ==
  LET a == ApplyAddMessage(InstalledState, 2,
             TestMessage(5, 2, 1, 0, 0, {}, {}, "hello"), {})
      b == ApplyAddReaction(a, 2, 5, "like")
      c == ApplyDeleteUser(b, 1, 2)
  IN ~UserExists(c, 2) /\ ~MessageExists(c, 5) /\
     c.reactions = {} /\ DeclaredReferentialIntegrity(c)

ThreadCountTest ==
  LET a == ApplyAddMessage(InstalledState, 2,
             TestMessage(5, 2, 1, 0, 0, {}, {}, "hello"), {})
      b == ApplyAddMessage(a, 2,
             TestMessage(6, 2, 1, 5, 0, {}, {}, "reply"), {})
      c == ApplyDeleteMessage(b, 2, 6)
  IN Only(MessagesById(b, 5)).childrenCount = 1 /\ ThreadCountsExact(b) /\
     Only(MessagesById(c, 5)).childrenCount = 0 /\ ThreadCountsExact(c)

FailedReplyLeavesCountDriftTest ==
  LET a == ApplyAddMessage(InstalledState, 2,
             TestMessage(5, 2, 1, 0, 0, {}, {}, "hello"), {})
      b == ApplyAddMessage(a, 2,
             TestMessage(6, 2, 1, 5, 8, {}, {}, "reply"), {})
  IN b.lastResult = DbConstraint /\ ~MessageExists(b, 6) /\ ~ThreadCountsExact(b)

DraftUniquenessTest ==
  LET a == ApplyUpsertDraft(InstalledState, 2,
             TestDraft(5, 2, 1, 0, {}, "hello"))
      b == ApplyUpsertDraft(a, 2, TestDraft(6, 2, 1, 0, {}, "edited"))
  IN Cardinality(b.drafts) = 1 /\ Only(DraftsById(b, 5)).content = "edited" /\
     DraftLogicalUniqueness(b)

ReactionSetCanonicalAndDuplicateEventTest ==
  LET a == ApplyAddMessage(InstalledState, 2,
             TestMessage(5, 2, 1, 0, 0, {}, {}, "hello"), {})
      b == ApplyAddReaction(a, 2, 5, "like")
      c == ApplyAddReaction(b, 2, 5, "like")
  IN Cardinality(c.reactions) = 1 /\ ReactionSetCanonical(c) /\
     Cardinality({i \in 1..Len(c.events) :
       c.events[i].kind = "reaction" /\
       c.events[i].operation = "create" /\ c.events[i].entityId = 5}) = 2

ReactionNeedsNoPermissionTest ==
  LET a == ApplyAddMessage(InstalledState, 1,
             TestMessage(5, 1, 1, 0, 0, {}, {}, "hello"), {})
      b == ApplyAddUserRole(a, 1, UserRole(2, 0, 0, 1, 3, {},
             {ChannelRead, ChannelReadMessage, ChannelCreateMessage}))
      c == ApplyAddReaction(b, 2, 5, "heart")
  IN c.lastResult = OK /\
     [messageId |-> 5, userId |-> 2, code |-> "heart"] \in c.reactions

OwnershipGapTest ==
  LET attachment == Attachment(5, 2, "application/octet-stream", 10)
      a == ApplyUploadAttachment(InstalledState, 2, attachment)
      b == ApplyAddMessage(a, 1,
             TestMessage(6, 1, 1, 0, 0, {5}, {}, "hello"), {})
  IN b.lastResult = OK /\ ~AttachmentOwnership(b)

AbstractUrlPreviewTest ==
  LET p == Preview(5, "https://example.test")
      a == ApplyAddMessage(InstalledState, 2,
             TestMessage(6, 2, 1, 0, 0, {}, {5}, "hello"), {p})
  IN a.lastResult = OK /\ p \in a.urlPreviews /\
     Only(MessagesById(a, 6)).urlPreviewIds = {5}

MissingChannelReferencesTest ==
  LET a == ApplyAddChannel(InstalledState, 1, Channel(5, "orphan-type", 8, 1))
  IN a.lastResult = OK /\ ChannelExists(a, 5) /\
     ~StrongDomainReferences(a) /\ DeclaredReferentialIntegrity(a)

InstallIdempotenceTest ==
  LET a == ApplyInstall(EmptyState)
      b == ApplyInstall(a)
  IN a.lastResult = OK /\ b.lastResult = Installed /\
     Cardinality(b.roles) = 3 /\ Cardinality(b.users) = 2 /\
     InstallDataIdempotent(b)

PermissionSafetyTest ==
  LET a == ApplyAddWorkspace(InstalledState, 2, Workspace(5, "forbidden"))
  IN a.lastResult = Forbidden /\ ~WorkspaceExists(a, 5) /\ PermissionSafety(a)

LoginIgnoresAccountFlagsTest ==
  LET banned == [SeededUser EXCEPT !.banned = TRUE, !.active = FALSE]
      a == ApplyUpdateUser(InstalledState, 1, banned)
      b == ApplyLogin(a, "test_user_2", "hash123")
  IN b.lastResult = OK /\ 2 \in b.issuedTokens

ConfigAndEventCorrespondenceTest ==
  LET a == ApplyUpdateConfig(InstalledState, 1,
             Config(2, "messages.sendCooldown", "900", "int"))
      b == ApplyAddWorkspace(a, 1, Workspace(5, "Events"))
      c == ApplyUpdateWorkspace(b, 1, Workspace(5, "Events updated"))
      d == ApplyDeleteWorkspace(c, 1, 5)
  IN d.lastResult = OK /\ EventCorrespondence(d) /\
     Cardinality({i \in 1..Len(d.events) : d.events[i].kind = "workspace"}) = 3

UninstallClearsDomainStateTest ==
  LET s == ApplyUninstall(InstalledState)
  IN ~s.schemaInstalled /\ s.roles = {} /\ s.users = {} /\ s.channels = {} /\
     s.lastResult = OK /\ CoreSafety(s)

\* Query scenarios (9)
SelfUserReadBypassesPermissionTest ==
  CanReadUser(InstalledState, 2, 2) /\ ~CanReadUser(InstalledState, 2, 1)

ScopedChannelListIgnoresDenylistTest ==
  LET a == ApplyAddRole(InstalledState, 1, TestRole(5, "empty", {}, FALSE))
      b == ApplyCreateUser(a, 1, TestUser(3, "new_user", "restricted@test.com", 5))
      c == ApplyAddUserRole(b, 1,
             UserRole(3, 1, 0, 0, 3, {}, {ChannelReadMessage}))
  IN ~CanReadMessages(c, 3, 1) /\ 1 \in VisibleChannelIds(c, 3, 1) /\
     1 \in VisibleWorkspaceIds(c, 3)

RoleAndChannelTypeQueriesNeedOnlyAuthenticationTest ==
  LET a == ApplyAddRole(InstalledState, 1, TestRole(5, "empty", {}, FALSE))
      b == ApplyCreateUser(a, 1, TestUser(3, "new_user", "restricted@test.com", 5))
  IN CanReadRolesOrChannelTypes(b, 3) /\ ~CanReadConfig(b, 3) /\
     ~CanReadAttachment(b, 3)

DefaultUserVisibilityTest ==
  VisibleWorkspaceIds(InstalledState, 2) = {1,2} /\
  VisibleChannelIds(InstalledState, 2, 1) = {1,2,3,4} /\
  CanReadMessages(InstalledState, 2, 1) /\ CanReadAttachment(InstalledState, 2)

UserFilteringAndNicknameLookupTest ==
  LET banned == [SeededUser EXCEPT !.banned = TRUE]
      s == ApplyUpdateUser(InstalledState, 1, banned)
  IN IdsOf(FilteredUsers(s, 1, -1, 1, "", "")) = {2} /\
     IdsOf(FilteredUsers(s, 1, 0, -1, "TEST1@TEST.COM", "TEST_USER_1")) = {1} /\
     IdsOf(UsersByNicknameLookup(s, 2, "TEST_USER_2")) = {2} /\
     UsersByNicknameLookup(s, 2, "test_user_1") = {}

RoleLookupAndGetMyRolesTest ==
  LET s == ApplyAddUserRole(InstalledState, 1,
        UserRole(2, 1, 0, 0, 2, {ConfigRead}, {ChannelDeleteMessage}))
  IN {r.name : r \in RoleLookupById(s, 2, 3)} = {"user"} /\
     IdsOf(RoleLookupByName(s, 2, "moderator")) = {2} /\
     MyGlobalPermissions(s, 2) = DefaultUserPermissions /\
     ConfigRead \in VisibleWorkspacePermissions(s, 2, 1)

MessageSelectionAndHydrationTest ==
  LET a == ApplyAddMessage(InstalledState, 2,
             TestMessage(5, 2, 1, 0, 0, {}, {}, "hello"), {})
      b == ApplyAddMessage(a, 2,
             TestMessage(6, 2, 1, 5, 0, {}, {}, "hello"), {})
      c == ApplyAddMessage(b, 2,
             TestMessage(7, 2, 1, 0, 5, {}, {}, "hello"), {})
      d == ApplyAddReaction(ApplyAddReaction(c, 1, 5, "like"), 2, 5, "like")
      orphan == TestMessage(8, 4, 1, 0, 0, {}, {}, "hello")
  IN RootMessageIds(d, 2, 1) = {5,7} /\ ThreadMessageIds(d, 2, 5) = {6} /\
     HydratedMessageIds(d, {5,6,7}) = {5,6,7} /\
     HydratedMessageIds([d EXCEPT !.messages = @ \union {orphan}], {8}) = {} /\
     [messageId |-> 5, code |-> "like", count |-> 2] \in
       ReactionAggregates(d, {5}) /\
     MyReactionCodes(d, 2, 5) = {"like"} /\ ReplyReferenceIds(d, {5,7}) = {5}

AttachmentAndConfigLookupPermissionTest ==
  LET a == ApplyUploadAttachment(InstalledState, 2,
             Attachment(5, 2, "image/custom", 5))
      b == ApplyAddRole(a, 1, TestRole(5, "empty", {}, FALSE))
      c == ApplyCreateUser(b, 1, TestUser(3, "new_user", "restricted@test.com", 5))
  IN Cardinality(AttachmentLookup(c, 2, 5)) = 1 /\
     AttachmentLookup(c, 3, 5) = {} /\
     Cardinality(ConfigLookupById(c, 1, 1)) = 1 /\
     Cardinality(ConfigLookupByName(c, 1, "settings.NewUserDefaultRole")) = 1 /\
     ConfigLookupById(c, 3, 1) = {}

AttachmentMimePrefixClassesTest ==
  LET input == [Attachment(5, 2, "image/custom", 5) EXCEPT !.extension = "png"]
      s == ApplyUploadAttachment(InstalledState, 2, input)
      a == Only(AttachmentLookup(s, 2, 5))
  IN a.attachmentType = "image" /\ a.extension = "png" /\
     AttachmentTypeForMime("video/custom") = "video" /\
     AttachmentTypeForMime("audio/custom") = "audio" /\
     AttachmentTypeForMime("application/custom") = "binary"

AllBehavioralScenarios ==
  /\ ScopePrecedenceTest /\ UserRoleUniquenessTest
  /\ ProtectedRoleIsMutableTest
  /\ PermissionEvidenceInvariantIsNonTautologicalTest
  /\ DuplicateScopedRowsRemainSeparateCandidatesTest
  /\ DuplicateDefaultRoleConfigsAreArbitraryTest
  /\ ReferentialCascadeTest /\ ThreadCountTest /\ FailedReplyLeavesCountDriftTest
  /\ DraftUniquenessTest /\ ReactionSetCanonicalAndDuplicateEventTest
  /\ ReactionNeedsNoPermissionTest /\ OwnershipGapTest /\ AbstractUrlPreviewTest
  /\ MissingChannelReferencesTest /\ InstallIdempotenceTest
  /\ PermissionSafetyTest /\ LoginIgnoresAccountFlagsTest
  /\ ConfigAndEventCorrespondenceTest /\ UninstallClearsDomainStateTest

AllQueryScenarios ==
  /\ SelfUserReadBypassesPermissionTest /\ ScopedChannelListIgnoresDenylistTest
  /\ RoleAndChannelTypeQueriesNeedOnlyAuthenticationTest
  /\ DefaultUserVisibilityTest /\ UserFilteringAndNicknameLookupTest
  /\ RoleLookupAndGetMyRolesTest /\ MessageSelectionAndHydrationTest
  /\ AttachmentAndConfigLookupPermissionTest /\ AttachmentMimePrefixClassesTest

AllScenarios == AllBehavioralScenarios /\ AllQueryScenarios

VARIABLE check
ScenarioInit == check = 0
ScenarioNext == check' = check
ScenarioSpec == ScenarioInit /\ [][ScenarioNext]_<<check>>
ScenarioInvariant == AllScenarios

=============================================================================
