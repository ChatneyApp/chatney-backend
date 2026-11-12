-------------------- MODULE ChatneyState --------------------
EXTENDS Naturals, FiniteSets

CONSTANTS
    Users, Roles, RecordIds, ResetTokens, Permissions,
    AdminUser, AdminRole, DefaultRole, AdminManage, MessageCreate, WorkflowScope,
    ValidLoginUsers, NoUser, NoRecord

VARIABLES
    setupPhase, userStatus, sessions, userRoles, rolePermissions,
    userPermissions, records, recordOwner, recordKind, recordParent,
    publishedRecords, resetTokens, resetOwner, usedResetTokens,
    socketBindings, configuredKeys

vars == <<setupPhase, userStatus, sessions, userRoles, rolePermissions,
    userPermissions, records, recordOwner, recordKind, recordParent,
    publishedRecords, resetTokens, resetOwner, usedResetTokens,
    socketBindings, configuredKeys>>

Statuses == {"absent", "active", "disabled", "banned"}
SetupPhases == {"uninstalled", "migrating", "seeding", "roleCreated",
    "failed", "failedPartial", "incomplete", "installed"}
RecordKinds == {"workspace", "channel", "directMessage", "message", "draft", "attachment", "reaction"}

Init ==
    /\ setupPhase = "uninstalled"
    /\ userStatus = [u \in Users |-> "absent"]
    /\ sessions = {}
    /\ userRoles = [u \in Users |-> {}]
    /\ rolePermissions = [r \in Roles |-> {}]
    /\ userPermissions = [u \in Users |-> {}]
    /\ records = {}
    /\ recordOwner = [r \in RecordIds |-> NoUser]
    /\ recordKind = [r \in RecordIds |-> "none"]
    /\ recordParent = [r \in RecordIds |-> NoRecord]
    /\ publishedRecords = {}
    /\ resetTokens = {}
    /\ resetOwner = [t \in ResetTokens |-> NoUser]
    /\ usedResetTokens = {}
    /\ socketBindings = {}
    /\ configuredKeys = {}

IsAdmin(u) ==
    userStatus[u] = "active" /\ u \in sessions /\
        (AdminManage \in userPermissions[u]
            \/ \E role \in userRoles[u] : AdminManage \in rolePermissions[role])

HasPermission(u, permission) ==
    permission \in userPermissions[u]
    \/ \E role \in userRoles[u] : permission \in rolePermissions[role]

CanCreate(u, kind) ==
    userStatus[u] = "active" /\ u \in sessions /\
        (IF kind = "message" THEN HasPermission(u, MessageCreate)
            ELSE IF kind \in {"draft", "attachment", "reaction", "directMessage"}
                THEN TRUE ELSE IsAdmin(u))

StartSetup ==
    /\ setupPhase = "uninstalled"
    /\ setupPhase' = "migrating"
    /\ UNCHANGED <<userStatus, sessions, userRoles, rolePermissions,
        userPermissions, records, recordOwner, recordKind, recordParent,
        publishedRecords, resetTokens, resetOwner, usedResetTokens,
        socketBindings, configuredKeys>>

MigrationsComplete ==
    /\ setupPhase = "migrating"
    /\ setupPhase' = "seeding"
    /\ UNCHANGED <<userStatus, sessions, userRoles, rolePermissions,
        userPermissions, records, recordOwner, recordKind, recordParent,
        publishedRecords, resetTokens, resetOwner, usedResetTokens,
        socketBindings, configuredKeys>>

\* The role is persisted before users/configuration; it is the retry sentinel.
CreateAdminRole ==
    /\ setupPhase = "seeding"
    /\ setupPhase' = "roleCreated"
    /\ UNCHANGED <<userStatus, sessions, userRoles, rolePermissions,
        userPermissions, records, recordOwner, recordKind, recordParent,
        publishedRecords, resetTokens, resetOwner, usedResetTokens,
        socketBindings, configuredKeys>>

CommitSetup ==
    /\ setupPhase = "roleCreated"
    /\ setupPhase' = "installed"
    /\ userStatus' = [userStatus EXCEPT ![AdminUser] = "active"]
    /\ userRoles' = [userRoles EXCEPT ![AdminUser] = {AdminRole}]
    /\ userPermissions' = [userPermissions EXCEPT ![AdminUser] = {AdminManage}]
    /\ rolePermissions' = [rolePermissions EXCEPT
        ![AdminRole] = {AdminManage, MessageCreate},
        ![DefaultRole] = {MessageCreate}]
    /\ UNCHANGED <<sessions, records, recordOwner, recordKind, recordParent,
        publishedRecords, resetTokens, resetOwner, usedResetTokens,
        socketBindings, configuredKeys>>

FailSetup ==
    /\ setupPhase \in {"migrating", "seeding", "roleCreated"}
    /\ setupPhase' = IF setupPhase = "roleCreated" THEN "failedPartial" ELSE "failed"
    /\ UNCHANGED <<userStatus, sessions, userRoles, rolePermissions,
        userPermissions, records, recordOwner, recordKind, recordParent,
        publishedRecords, resetTokens, resetOwner, usedResetTokens,
        socketBindings, configuredKeys>>

RetrySetup ==
    /\ setupPhase \in {"failed", "failedPartial"}
    \* Retry sees the persisted role and returns "installed" without finishing seed.
    /\ setupPhase' = IF setupPhase = "failedPartial" THEN "incomplete" ELSE "migrating"
    /\ UNCHANGED <<userStatus, sessions, userRoles, rolePermissions,
        userPermissions, records, recordOwner, recordKind, recordParent,
        publishedRecords, resetTokens, resetOwner, usedResetTokens,
        socketBindings, configuredKeys>>

UninstallSystem ==
    \* The current mutation does not authorize the caller.
    /\ setupPhase # "uninstalled"
    /\ setupPhase' = "uninstalled"
    /\ userStatus' = [u \in Users |-> "absent"]
    /\ sessions' = {}
    /\ userRoles' = [u \in Users |-> {}]
    /\ rolePermissions' = [r \in Roles |-> {}]
    /\ userPermissions' = [u \in Users |-> {}]
    /\ records' = {}
    /\ publishedRecords' = {}
    /\ resetTokens' = {}
    /\ usedResetTokens' = {}
    /\ socketBindings' = {}
    /\ configuredKeys' = {}
    /\ recordOwner' = [r \in RecordIds |-> NoUser]
    /\ recordKind' = [r \in RecordIds |-> "none"]
    /\ recordParent' = [r \in RecordIds |-> NoRecord]
    /\ resetOwner' = [t \in ResetTokens |-> NoUser]

RegisterUser(u) ==
    /\ setupPhase = "installed" /\ userStatus[u] = "absent"
    /\ userStatus' = [userStatus EXCEPT ![u] = "active"]
    /\ userRoles' = [userRoles EXCEPT ![u] = {DefaultRole}]
    /\ UNCHANGED <<setupPhase, sessions, rolePermissions, userPermissions,
        records, recordOwner, recordKind, recordParent, publishedRecords,
        resetTokens, resetOwner, usedResetTokens, socketBindings, configuredKeys>>

Login(u) ==
    /\ setupPhase = "installed" /\ u \in ValidLoginUsers
    /\ userStatus[u] = "active"
    /\ sessions' = sessions \cup {u}
    /\ UNCHANGED <<setupPhase, userStatus, userRoles, rolePermissions,
        userPermissions, records, recordOwner, recordKind, recordParent,
        publishedRecords, resetTokens, resetOwner, usedResetTokens,
        socketBindings, configuredKeys>>

Logout(u) ==
    /\ u \in sessions
    /\ sessions' = sessions \ {u}
    /\ socketBindings' = socketBindings \ {u}
    /\ UNCHANGED <<setupPhase, userStatus, userRoles, rolePermissions,
        userPermissions, records, recordOwner, recordKind, recordParent,
        publishedRecords, resetTokens, resetOwner, usedResetTokens, configuredKeys>>

ChangeAccountStatus(actor, u, status) ==
    /\ IsAdmin(actor) /\ status \in {"active", "disabled", "banned"}
    /\ (u # AdminUser \/ status = "active")
    /\ userStatus' = [userStatus EXCEPT ![u] = status]
    /\ sessions' = IF status = "active" THEN sessions ELSE sessions \ {u}
    /\ socketBindings' = IF status = "active" THEN socketBindings
        ELSE socketBindings \ {u}
    /\ UNCHANGED <<setupPhase, userRoles, rolePermissions, userPermissions,
        records, recordOwner, recordKind, recordParent, publishedRecords,
        resetTokens, resetOwner, usedResetTokens, configuredKeys>>

AssignRole(actor, u, role) ==
    /\ IsAdmin(actor) /\ role \in Roles
    /\ userRoles' = [userRoles EXCEPT ![u] = @ \cup {role}]
    /\ UNCHANGED <<setupPhase, userStatus, sessions, rolePermissions,
        userPermissions, records, recordOwner, recordKind, recordParent,
        publishedRecords, resetTokens, resetOwner, usedResetTokens,
        socketBindings, configuredKeys>>

SetRolePermission(actor, role, permission, grant) ==
    /\ IsAdmin(actor) /\ role \in Roles /\ permission \in Permissions /\ grant \in BOOLEAN
    /\ rolePermissions' = [rolePermissions EXCEPT ![role] =
        IF grant THEN @ \cup {permission} ELSE @ \ {permission}]
    /\ UNCHANGED <<setupPhase, userStatus, sessions, userRoles,
        userPermissions, records, recordOwner, recordKind, recordParent,
        publishedRecords, resetTokens, resetOwner, usedResetTokens,
        socketBindings, configuredKeys>>

SetUserPermission(actor, user, permission, grant) ==
    /\ IsAdmin(actor) /\ user \in Users /\ permission \in Permissions /\ grant \in BOOLEAN
    /\ userPermissions' = [userPermissions EXCEPT ![user] =
        IF grant THEN @ \cup {permission} ELSE @ \ {permission}]
    /\ UNCHANGED <<setupPhase, userStatus, sessions, userRoles,
        rolePermissions, records, recordOwner, recordKind, recordParent,
        publishedRecords, resetTokens, resetOwner, usedResetTokens,
        socketBindings, configuredKeys>>

CreateRecord(actor, id, kind, parent) ==
    /\ setupPhase = "installed" /\ id \in RecordIds \ records
    /\ kind \in RecordKinds /\ CanCreate(actor, kind)
    /\ (parent = NoRecord \/ parent \in records)
    /\ (kind # "draft" \/
        ~\E draft \in records : recordKind[draft] = "draft"
            /\ recordOwner[draft] = actor /\ recordParent[draft] = parent)
    /\ records' = records \cup {id}
    /\ recordOwner' = [recordOwner EXCEPT ![id] = actor]
    /\ recordKind' = [recordKind EXCEPT ![id] = kind]
    /\ recordParent' = [recordParent EXCEPT ![id] = parent]
    /\ UNCHANGED <<setupPhase, userStatus, sessions, userRoles,
        rolePermissions, userPermissions, publishedRecords, resetTokens,
        resetOwner, usedResetTokens, socketBindings, configuredKeys>>

PublishRecord(actor, id) ==
    /\ id \in records /\ recordOwner[id] = actor
    /\ actor \in sessions /\ userStatus[actor] = "active"
    /\ publishedRecords' = publishedRecords \cup {id}
    /\ UNCHANGED <<setupPhase, userStatus, sessions, userRoles,
        rolePermissions, userPermissions, records, recordOwner, recordKind,
        recordParent, resetTokens, resetOwner, usedResetTokens,
        socketBindings, configuredKeys>>

DeleteRecord(actor, id) ==
    /\ id \in records /\ actor \in sessions /\ userStatus[actor] = "active"
    /\ (recordOwner[id] = actor \/ IsAdmin(actor))
    /\ records' = records \ ({id} \cup {child \in records : recordParent[child] = id})
    /\ publishedRecords' = publishedRecords \ ({id} \cup {child \in records : recordParent[child] = id})
    /\ UNCHANGED <<setupPhase, userStatus, sessions, userRoles,
        rolePermissions, userPermissions, recordOwner, recordKind,
        recordParent, resetTokens, resetOwner, usedResetTokens,
        socketBindings, configuredKeys>>

SaveDraft(actor, id, scope) == CreateRecord(actor, id, "draft", scope)

RequestPasswordReset(user, token) ==
    /\ setupPhase = "installed" /\ userStatus[user] = "active"
    /\ token \in ResetTokens \ (resetTokens \cup usedResetTokens)
    /\ resetTokens' = (resetTokens \ {old \in resetTokens : resetOwner[old] = user})
        \cup {token}
    /\ resetOwner' = [resetOwner EXCEPT ![token] = user]
    /\ usedResetTokens' = usedResetTokens \cup {old \in resetTokens : resetOwner[old] = user}
    /\ UNCHANGED <<setupPhase, userStatus, sessions, userRoles,
        rolePermissions, userPermissions, records, recordOwner, recordKind,
        recordParent, publishedRecords, socketBindings, configuredKeys>>

CompletePasswordReset(user, token) ==
    /\ token \in resetTokens /\ resetOwner[token] = user
    /\ resetTokens' = {other \in resetTokens : resetOwner[other] # user}
    /\ usedResetTokens' = usedResetTokens \cup
        {other \in resetTokens : resetOwner[other] = user}
    /\ UNCHANGED <<setupPhase, userStatus, sessions, userRoles,
        rolePermissions, userPermissions, records, recordOwner, recordKind,
        recordParent, publishedRecords, resetOwner, socketBindings, configuredKeys>>

ConnectSocket(user) ==
    /\ user \in sessions /\ userStatus[user] = "active"
    /\ socketBindings' = socketBindings \cup {user}
    /\ UNCHANGED <<setupPhase, userStatus, sessions, userRoles,
        rolePermissions, userPermissions, records, recordOwner, recordKind,
        recordParent, publishedRecords, resetTokens, resetOwner,
        usedResetTokens, configuredKeys>>

DisconnectSocket(user) ==
    /\ user \in socketBindings
    /\ socketBindings' = socketBindings \ {user}
    /\ UNCHANGED <<setupPhase, userStatus, sessions, userRoles,
        rolePermissions, userPermissions, records, recordOwner, recordKind,
        recordParent, publishedRecords, resetTokens, resetOwner,
        usedResetTokens, configuredKeys>>

WriteConfig(actor, key) ==
    /\ IsAdmin(actor) /\ key \in {"system", "security", "email"}
    /\ configuredKeys' = configuredKeys \cup {key}
    /\ UNCHANGED <<setupPhase, userStatus, sessions, userRoles,
        rolePermissions, userPermissions, records, recordOwner, recordKind,
        recordParent, publishedRecords, resetTokens, resetOwner,
        usedResetTokens, socketBindings>>

SetupFlow ==
    StartSetup \/ MigrationsComplete \/ CreateAdminRole \/ CommitSetup
    \/ FailSetup \/ RetrySetup \/ UninstallSystem

CommonAccountFlow ==
    (\E u \in Users : RegisterUser(u) \/ Login(u) \/ Logout(u))

AccessFlow ==
    (\E actor \in Users, u \in Users, status \in {"active", "disabled", "banned"} :
        ChangeAccountStatus(actor, u, status))
    \/ (\E actor \in Users, u \in Users, role \in Roles : AssignRole(actor, u, role))
    \/ (\E actor \in Users, role \in Roles, permission \in Permissions, grant \in BOOLEAN :
        SetRolePermission(actor, role, permission, grant))
    \/ (\E actor \in Users, user \in Users, permission \in Permissions, grant \in BOOLEAN :
        SetUserPermission(actor, user, permission, grant))
    \/ (\E u \in Users : ConnectSocket(u) \/ DisconnectSocket(u))
    \/ (\E actor \in Users, key \in {"system", "security", "email"} : WriteConfig(actor, key))

CollaborationFlow ==
    (\E actor \in Users, id \in RecordIds, kind \in RecordKinds,
        parent \in (RecordIds \cup {NoRecord}) : CreateRecord(actor, id, kind, parent))
    \/ (\E actor \in Users, id \in RecordIds : PublishRecord(actor, id) \/ DeleteRecord(actor, id))
    \/ (\E actor \in Users, id \in RecordIds, scope \in (RecordIds \cup {NoRecord}) :
        SaveDraft(actor, id, scope))

RecoveryFlow ==
    (\E actor \in Users, u \in Users, status \in {"active", "disabled", "banned"} :
        ChangeAccountStatus(actor, u, status))
    \/ (\E user \in Users, token \in ResetTokens : RequestPasswordReset(user, token))
    \/ (\E user \in Users, token \in ResetTokens : CompletePasswordReset(user, token))

Next ==
    IF WorkflowScope = "setup" THEN SetupFlow
    ELSE SetupFlow \/ CommonAccountFlow
        \/ IF WorkflowScope = "access" THEN AccessFlow
           ELSE IF WorkflowScope = "collaboration" THEN CollaborationFlow
           ELSE IF WorkflowScope = "recovery" THEN RecoveryFlow
           ELSE FALSE

TypeOK ==
    /\ setupPhase \in SetupPhases
    /\ userStatus \in [Users -> Statuses]
    /\ sessions \subseteq Users
    /\ userRoles \in [Users -> SUBSET Roles]
    /\ rolePermissions \in [Roles -> SUBSET Permissions]
    /\ userPermissions \in [Users -> SUBSET Permissions]
    /\ records \subseteq RecordIds
    /\ recordOwner \in [RecordIds -> (Users \cup {NoUser})]
    /\ recordKind \in [RecordIds -> (RecordKinds \cup {"none"})]
    /\ recordParent \in [RecordIds -> (RecordIds \cup {NoRecord})]
    /\ publishedRecords \subseteq records
    /\ resetTokens \subseteq ResetTokens /\ usedResetTokens \subseteq ResetTokens
    /\ socketBindings \subseteq Users
    /\ configuredKeys \subseteq {"system", "security", "email"}

RecordsHaveOwnersAndValidParents ==
    \A id \in records : recordOwner[id] \in Users
        /\ recordKind[id] \in RecordKinds
        /\ (recordParent[id] = NoRecord \/ recordParent[id] \in records)

DraftsAreUniquePerOwnerAndScope ==
    \A u \in Users, scope \in (RecordIds \cup {NoRecord}) :
        Cardinality({id \in records : recordKind[id] = "draft"
            /\ recordOwner[id] = u /\ recordParent[id] = scope}) <= 1

ResetTokensHaveOwners == \A token \in resetTokens : resetOwner[token] \in Users

SetupStateConsistent ==
    setupPhase = "installed" =>
        userStatus[AdminUser] = "active" /\ AdminRole \in userRoles[AdminUser]

ActiveSessionsOnly == \A user \in sessions : userStatus[user] = "active"

SocketSessionsOnly == socketBindings \subseteq sessions

UsedResetTokensAreDead == resetTokens \cap usedResetTokens = {}

==============================================================
