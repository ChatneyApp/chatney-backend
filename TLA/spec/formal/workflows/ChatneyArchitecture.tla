---------------- MODULE ChatneyArchitecture ----------------
CONSTANTS Users, Roles, RecordIds, ResetTokens, Permissions,
    AdminUser, AdminRole, DefaultRole, AdminManage, MessageCreate, WorkflowScope,
    ValidLoginUsers, NoUser, NoRecord

VARIABLES setupPhase, userStatus, sessions, userRoles, rolePermissions,
    userPermissions, records, recordOwner, recordKind, recordParent,
    publishedRecords, resetTokens, resetOwner, usedResetTokens,
    socketBindings, configuredKeys

State == INSTANCE ChatneyState WITH
    Users <- Users,
    Roles <- Roles,
    RecordIds <- RecordIds,
    ResetTokens <- ResetTokens,
    Permissions <- Permissions,
    AdminUser <- AdminUser,
    AdminRole <- AdminRole,
    DefaultRole <- DefaultRole,
    AdminManage <- AdminManage,
    MessageCreate <- MessageCreate,
    WorkflowScope <- WorkflowScope,
    ValidLoginUsers <- ValidLoginUsers,
    NoUser <- NoUser,
    NoRecord <- NoRecord,
    setupPhase <- setupPhase,
    userStatus <- userStatus,
    sessions <- sessions,
    userRoles <- userRoles,
    rolePermissions <- rolePermissions,
    userPermissions <- userPermissions,
    records <- records,
    recordOwner <- recordOwner,
    recordKind <- recordKind,
    recordParent <- recordParent,
    publishedRecords <- publishedRecords,
    resetTokens <- resetTokens,
    resetOwner <- resetOwner,
    usedResetTokens <- usedResetTokens,
    socketBindings <- socketBindings,
    configuredKeys <- configuredKeys

Init == State!Init
Next == State!Next
vars == <<setupPhase, userStatus, sessions, userRoles, rolePermissions,
    userPermissions, records, recordOwner, recordKind, recordParent,
    publishedRecords, resetTokens, resetOwner, usedResetTokens,
    socketBindings, configuredKeys>>

Safety ==
    /\ State!TypeOK
    /\ State!RecordsHaveOwnersAndValidParents
    /\ State!DraftsAreUniquePerOwnerAndScope
    /\ State!ResetTokensHaveOwners
    /\ State!SetupStateConsistent
    /\ State!ActiveSessionsOnly
    /\ State!SocketSessionsOnly
    /\ State!UsedResetTokensAreDead

Spec == Init /\ [][Next]_vars

===============================================================
