------------------------------- MODULE Chatney -------------------------------
EXTENDS Integers, FiniteSets, Sequences, TLC,
        CoreTypes, Permissions, State, Install, Users, Roles, Organization,
        Messages, Drafts, Attachments, Configs, Invariants

VARIABLE state
vars == <<state>>

GeneratedUser(id, nickname) ==
  User(id, nickname, "generated@test.com", 3, "generatedHash")

GeneratedRole(id) == Role(id, "generated", FALSE, DefaultUserPermissions)
GeneratedUserRole(id) == UserRole(id, 1, 0, 0, 3, {}, {})
GeneratedWorkspace(id) == Workspace(id, "generated")
GeneratedChannelType(id) == ChannelType(id, "generated", "generated", 3)
GeneratedChannel(id) == Channel(id, "generated", 1, 1)
GeneratedChannelGroup(id) == ChannelGroup(id, "generated", 1, {1}, id)
GeneratedMessage(id, actor, content) ==
  Message(id, 1, actor, content, {}, {}, "input", 0, 0, 0)

GeneratedDraft(id, actor, content) == Draft(id, 1, actor, content, {}, 0)
GeneratedAttachment(id, actor, mime) == Attachment(id, actor, mime, 1)
GeneratedConfig(id, content) == Config(id, "generated", content, "string")

Init == state = EmptyState
InstallAction == state' = ApplyInstall(state)
UninstallAction == state' = ApplyUninstall(state)
RegisterAction ==
  \E id \in UserIds, n \in ValidNicknames :
    LET user == GeneratedUser(id, n)
        choices == DefaultRoleConfigIds(state)
    IN (choices = {} /\ state' = FailedState(state, Failed)) \/
       (\E configId \in choices :
          state' = ApplyRegisterWithConfig(state, user, configId))

LoginAction == state' = ApplyLogin(state, "test_user_1", "hash123")

CreateUserAction ==
  \E a \in UserIds, id \in UserIds, n \in ValidNicknames :
    state' = ApplyCreateUser(state, a, GeneratedUser(id, n))

UpdateUserAction ==
  \E a \in UserIds, id \in UserIds, n \in ValidNicknames :
    state' = ApplyUpdateUser(state, a, GeneratedUser(id, n))

UpdateProfileAction ==
  \E a \in UserIds, n \in ValidNicknames :
    state' = ApplyUpdateProfile(state, a, n, "", FALSE, "", "", FALSE,
                                "hash123", "newHash")

DeleteUserAction ==
  \E a \in UserIds, id \in UserIds : state' = ApplyDeleteUser(state, a, id)

AddRoleAction ==
  \E a \in UserIds, id \in Ids : state' = ApplyAddRole(state, a, GeneratedRole(id))

UpdateRoleAction ==
  \E a \in UserIds, id \in Ids :
    state' = ApplyUpdateRole(state, a, GeneratedRole(id))

DeleteRoleAction ==
  \E a \in UserIds, id \in Ids : state' = ApplyDeleteRole(state, a, id)

AddUserRoleAction ==
  \E a \in UserIds, id \in UserIds :
    state' = ApplyAddUserRole(state, a, GeneratedUserRole(id))

UpdateUserRoleAction ==
  \E a \in UserIds, id \in UserIds :
    state' = ApplyUpdateUserRole(state, a, GeneratedUserRole(id))

DeleteUserRoleAction ==
  \E a \in UserIds, id \in UserIds :
    state' = ApplyDeleteUserRole(state, a, GeneratedUserRole(id))

AddWorkspaceAction ==
  \E a \in UserIds, id \in Ids :
    state' = ApplyAddWorkspace(state, a, GeneratedWorkspace(id))

UpdateWorkspaceAction ==
  \E a \in UserIds, id \in Ids :
    state' = ApplyUpdateWorkspace(state, a, GeneratedWorkspace(id))

DeleteWorkspaceAction ==
  \E a \in UserIds, id \in Ids : state' = ApplyDeleteWorkspace(state, a, id)

AddChannelTypeAction ==
  \E a \in UserIds, id \in Ids :
    state' = ApplyAddChannelType(state, a, GeneratedChannelType(id))

UpdateChannelTypeAction ==
  \E a \in UserIds, id \in Ids :
    state' = ApplyUpdateChannelType(state, a, GeneratedChannelType(id))

DeleteChannelTypeAction ==
  \E a \in UserIds, id \in Ids :
    state' = ApplyDeleteChannelType(state, a, id)

AddChannelAction ==
  \E a \in UserIds, id \in Ids :
    state' = ApplyAddChannel(state, a, GeneratedChannel(id))

UpdateChannelAction ==
  \E a \in UserIds, id \in Ids :
    state' = ApplyUpdateChannel(state, a, GeneratedChannel(id))

DeleteChannelAction ==
  \E a \in UserIds, id \in Ids : state' = ApplyDeleteChannel(state, a, id)

AddChannelGroupAction ==
  \E a \in UserIds, id \in Ids :
    state' = ApplyAddChannelGroup(state, a, GeneratedChannelGroup(id))

UpdateChannelGroupAction ==
  \E a \in UserIds, id \in Ids :
    state' = ApplyUpdateChannelGroup(state, a, GeneratedChannelGroup(id))

DeleteChannelGroupAction ==
  \E a \in UserIds, id \in Ids :
    state' = ApplyDeleteChannelGroup(state, a, id)

AddMessageAction ==
  \E a \in UserIds, id \in Ids, content \in Contents :
    state' = ApplyAddMessage(state, a, GeneratedMessage(id, a, content), {})

UpdateMessageAction ==
  \E a \in UserIds, id \in Ids, content \in Contents :
    state' = ApplyUpdateMessage(state, a, id, content, {}, {}, {})

DeleteMessageAction ==
  \E a \in UserIds, id \in Ids : state' = ApplyDeleteMessage(state, a, id)

AddReactionAction ==
  \E a \in UserIds, id \in Ids, code \in ReactionCodes :
    state' = ApplyAddReaction(state, a, id, code)

DeleteReactionAction ==
  \E a \in UserIds, id \in Ids, code \in ReactionCodes :
    state' = ApplyDeleteReaction(state, a, id, code)

UpsertDraftAction ==
  \E a \in UserIds, id \in Ids, content \in Contents :
    state' = ApplyUpsertDraft(state, a, GeneratedDraft(id, a, content))

DeleteDraftAction ==
  \E a \in UserIds, id \in Ids : state' = ApplyDeleteDraft(state, a, id)

UploadAttachmentAction ==
  \E a \in UserIds, id \in Ids, mime \in MimeTypes :
    state' = ApplyUploadAttachment(state, a, GeneratedAttachment(id, a, mime))

UpdateConfigAction ==
  \E a \in UserIds, id \in Ids, content \in Contents :
    state' = ApplyUpdateConfig(state, a, GeneratedConfig(id, content))

Next ==
  \/ InstallAction \/ UninstallAction \/ RegisterAction \/ LoginAction
  \/ CreateUserAction \/ UpdateUserAction \/ UpdateProfileAction
  \/ DeleteUserAction \/ AddRoleAction \/ UpdateRoleAction \/ DeleteRoleAction
  \/ AddUserRoleAction \/ UpdateUserRoleAction \/ DeleteUserRoleAction
  \/ AddWorkspaceAction \/ UpdateWorkspaceAction \/ DeleteWorkspaceAction
  \/ AddChannelTypeAction \/ UpdateChannelTypeAction \/ DeleteChannelTypeAction
  \/ AddChannelAction \/ UpdateChannelAction \/ DeleteChannelAction
  \/ AddChannelGroupAction \/ UpdateChannelGroupAction \/ DeleteChannelGroupAction
  \/ AddMessageAction \/ UpdateMessageAction \/ DeleteMessageAction
  \/ AddReactionAction \/ DeleteReactionAction
  \/ UpsertDraftAction \/ DeleteDraftAction
  \/ UploadAttachmentAction \/ UpdateConfigAction

Spec == Init /\ [][Next]_vars
\* Representative arguments keep exhaustive safety checking tractable.  Every
\* action family remains present here; Next above retains the complete finite
\* generator (Ids 1..8, UserIds 1..4, all contents, reactions and MIME classes).
CoreRegister ==
  LET user == GeneratedUser(3, "new_user")
      choices == DefaultRoleConfigIds(state)
  IN (choices = {} /\ state' = FailedState(state, Failed)) \/
     (\E configId \in choices :
       state' = ApplyRegisterWithConfig(state, user, configId))

CoreNext ==
  /\ Len(state.audits) < 2
  /\ \/ InstallAction
     \/ UninstallAction
     \/ CoreRegister
     \/ LoginAction
     \/ state' = ApplyCreateUser(state, 1, GeneratedUser(3, "new_user"))
     \/ state' = ApplyUpdateUser(state, 1, SeededUser)
     \/ state' = ApplyUpdateProfile(state, 2, "test_user_2", "", FALSE,
                    "", "", FALSE, "hash123", "newHash")
     \/ state' = ApplyDeleteUser(state, 1, 2)
     \/ state' = ApplyAddRole(state, 1, GeneratedRole(5))
     \/ state' = ApplyUpdateRole(state, 1, GeneratedRole(2))
     \/ state' = ApplyDeleteRole(state, 1, 5)
     \/ state' = ApplyAddUserRole(state, 1, GeneratedUserRole(2))
     \/ state' = ApplyUpdateUserRole(state, 1, GeneratedUserRole(2))
     \/ state' = ApplyDeleteUserRole(state, 1, GeneratedUserRole(2))
     \/ state' = ApplyAddWorkspace(state, 1, GeneratedWorkspace(5))
     \/ state' = ApplyUpdateWorkspace(state, 1, GeneratedWorkspace(1))
     \/ state' = ApplyDeleteWorkspace(state, 1, 2)
     \/ state' = ApplyAddChannelType(state, 1, GeneratedChannelType(5))
     \/ state' = ApplyUpdateChannelType(state, 1, GeneratedChannelType(1))
     \/ state' = ApplyDeleteChannelType(state, 1, 2)
     \/ state' = ApplyAddChannel(state, 1, GeneratedChannel(5))
     \/ state' = ApplyUpdateChannel(state, 1, GeneratedChannel(1))
     \/ state' = ApplyDeleteChannel(state, 1, 4)
     \/ state' = ApplyAddChannelGroup(state, 1, GeneratedChannelGroup(5))
     \/ state' = ApplyUpdateChannelGroup(state, 1, GeneratedChannelGroup(5))
     \/ state' = ApplyDeleteChannelGroup(state, 1, 5)
     \/ state' = ApplyAddMessage(
          state, 2, GeneratedMessage(5, 2, "hello"), {})
     \/ state' = ApplyUpdateMessage(state, 2, 5, "edited", {}, {}, {})
     \/ state' = ApplyDeleteMessage(state, 2, 5)
     \/ state' = ApplyAddReaction(state, 2, 5, "like")
     \/ state' = ApplyDeleteReaction(state, 2, 5, "like")
     \/ state' = ApplyUpsertDraft(
          state, 2, GeneratedDraft(5, 2, "hello"))
     \/ state' = ApplyDeleteDraft(state, 2, 5)
     \/ state' = ApplyUploadAttachment(
          state, 2, GeneratedAttachment(5, 2, "image/custom"))
     \/ state' = ApplyUpdateConfig(
          state, 1, Config(2, "messages.sendCooldown", "900", "int"))

CoreSpec == Init /\ [][CoreNext]_vars

UniquenessInvariant == Uniqueness(state)
ReferentialIntegrityInvariant == DeclaredReferentialIntegrity(state)
ThreadCountExpectedGap == ThreadCountsExact(state)
DraftUniquenessInvariant == DraftLogicalUniqueness(state)
ReactionCanonicalInvariant == ReactionSetCanonical(state)
AttachmentOwnershipExpectedGap == AttachmentOwnership(state)
PermissionSafetyInvariant == PermissionSafety(state)
InstallIdempotenceInvariant == InstallDataIdempotent(state)
EventCorrespondenceInvariant == EventCorrespondence(state)
CoreSafetyInvariant == CoreSafety(state)

=============================================================================
