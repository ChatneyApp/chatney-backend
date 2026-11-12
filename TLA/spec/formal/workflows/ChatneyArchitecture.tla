---------------- MODULE ChatneyArchitecture ----------------
CONSTANTS Users, Roles, Permissions, AdminUser, AdminRole, DefaultRole,
    AdminManage, MessageCreate, ValidLoginUsers, RecordIds, ResetTokens, NoUser, NoRecord
VARIABLES userStatus, sessions, userRoles, rolePermissions, userPermissions,
    socketBindings, configuredKeys, setupPhase, adminReady,
    messages, messageOwner, messageKind, messageParent, publishedMessages,
    workspaces, workspaceOwner, workspaceKind, workspaceParent, publishedWorkspaces,
    resetTokens, resetOwner, usedResetTokens
UsersDomain == INSTANCE UsersWorkflow WITH
    userStatus <- userStatus,
    sessions <- sessions,
    userRoles <- userRoles,
    rolePermissions <- rolePermissions,
    userPermissions <- userPermissions,
    socketBindings <- socketBindings,
    configuredKeys <- configuredKeys
MessagesDomain == INSTANCE MessagesWorkflow WITH
    messages <- messages,
    messageOwner <- messageOwner,
    messageKind <- messageKind,
    messageParent <- messageParent,
    publishedMessages <- publishedMessages
WorkspacesDomain == INSTANCE WorkspacesWorkflow WITH
    workspaces <- workspaces,
    workspaceOwner <- workspaceOwner,
    workspaceKind <- workspaceKind,
    workspaceParent <- workspaceParent,
    publishedWorkspaces <- publishedWorkspaces
SetupDomain == INSTANCE SetupWorkflow WITH
    setupPhase <- setupPhase,
    adminReady <- adminReady
RecoveryDomain == INSTANCE RecoveryState WITH
    userStatus <- userStatus,
    resetTokens <- resetTokens,
    resetOwner <- resetOwner,
    usedResetTokens <- usedResetTokens
vars == <<UsersDomain!vars, MessagesDomain!vars, WorkspacesDomain!vars,
    SetupDomain!vars, RecoveryDomain!vars>>
Init == UsersDomain!Init /\ MessagesDomain!Init /\ WorkspacesDomain!Init
    /\ SetupDomain!Init /\ RecoveryDomain!Init
Next ==
    (SetupDomain!Next /\
        IF setupPhase' = "uninstalled" THEN
            (UsersDomain!Init)' /\ (MessagesDomain!Init)'
            /\ (WorkspacesDomain!Init)' /\ (RecoveryDomain!Init)'
        ELSE UNCHANGED <<UsersDomain!vars, MessagesDomain!vars,
            WorkspacesDomain!vars, RecoveryDomain!vars>>)
    \/ (setupPhase = "installed" /\
        ((UsersDomain!Next /\ UNCHANGED <<SetupDomain!vars, MessagesDomain!vars,
            WorkspacesDomain!vars, RecoveryDomain!vars>>)
        \/ (MessagesDomain!Next /\ UNCHANGED <<SetupDomain!vars, UsersDomain!vars,
            WorkspacesDomain!vars, RecoveryDomain!vars>>)
        \/ (WorkspacesDomain!Next /\ UNCHANGED <<SetupDomain!vars, UsersDomain!vars,
            MessagesDomain!vars, RecoveryDomain!vars>>)
        \/ (RecoveryDomain!Next /\ UNCHANGED <<SetupDomain!vars, UsersDomain!vars,
            MessagesDomain!vars, WorkspacesDomain!vars>>)))
Safety == UsersDomain!Safety /\ MessagesDomain!Safety /\ WorkspacesDomain!Safety
    /\ SetupDomain!Safety /\ RecoveryDomain!Safety
Spec == Init /\ [][Next]_vars
============================================================
