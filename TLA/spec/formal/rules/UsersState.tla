---------------- MODULE UsersState ----------------
EXTENDS FiniteSets
CONSTANTS Users, Roles, Permissions, AdminUser, AdminRole, DefaultRole,
    AdminManage, MessageCreate, ValidLoginUsers
VARIABLES userStatus, sessions, userRoles, rolePermissions, userPermissions, socketBindings, configuredKeys
vars == <<userStatus, sessions, userRoles, rolePermissions, userPermissions, socketBindings, configuredKeys>>

Init ==
    /\ userStatus = [u \in Users |-> IF u = AdminUser THEN "active" ELSE "absent"]
    /\ sessions = {}
    /\ userRoles = [u \in Users |-> IF u = AdminUser THEN {AdminRole} ELSE {}]
    /\ rolePermissions = [r \in Roles |-> IF r = AdminRole THEN Permissions ELSE {MessageCreate}]
    /\ userPermissions = [u \in Users |-> IF u = AdminUser THEN {AdminManage} ELSE {}]
    /\ socketBindings = {}
    /\ configuredKeys = {}

IsAdmin(u) ==
    userStatus[u] = "active" /\ u \in sessions /\
        (AdminManage \in userPermissions[u]
            \/ \E role \in userRoles[u] : AdminManage \in rolePermissions[role])

HasPermission(u, permission) ==
    permission \in userPermissions[u]
    \/ \E role \in userRoles[u] : permission \in rolePermissions[role]

RegisterUser(u) ==
    /\ userStatus[u] = "absent"
    /\ userStatus' = [userStatus EXCEPT ![u] = "active"]
    /\ userRoles' = [userRoles EXCEPT ![u] = {DefaultRole}]
    /\ UNCHANGED <<sessions, rolePermissions, userPermissions, socketBindings, configuredKeys>>

Login(u) ==
    /\ u \in ValidLoginUsers
    /\ userStatus[u] = "active"
    /\ sessions' = sessions \cup {u}
    /\ UNCHANGED <<userStatus, userRoles, rolePermissions, userPermissions, socketBindings, configuredKeys>>

Logout(u) ==
    /\ u \in sessions
    /\ sessions' = sessions \ {u}
    /\ socketBindings' = socketBindings \ {u}
    /\ UNCHANGED <<userStatus, userRoles, rolePermissions, userPermissions, configuredKeys>>

ChangeAccountStatus(actor, u, status) ==
    /\ IsAdmin(actor) /\ status \in {"active", "disabled", "banned"}
    /\ (u # AdminUser \/ status = "active")
    /\ userStatus' = [userStatus EXCEPT ![u] = status]
    /\ sessions' = IF status = "active" THEN sessions ELSE sessions \ {u}
    /\ socketBindings' = IF status = "active" THEN socketBindings
        ELSE socketBindings \ {u}
    /\ UNCHANGED <<userRoles, rolePermissions, userPermissions, configuredKeys>>

AssignRole(actor, u, role) ==
    /\ IsAdmin(actor) /\ role \in Roles
    /\ userRoles' = [userRoles EXCEPT ![u] = @ \cup {role}]
    /\ UNCHANGED <<userStatus, sessions, rolePermissions, userPermissions, socketBindings, configuredKeys>>

SetRolePermission(actor, role, permission, grant) ==
    /\ IsAdmin(actor) /\ role \in Roles /\ permission \in Permissions /\ grant \in BOOLEAN
    /\ rolePermissions' = [rolePermissions EXCEPT ![role] =
        IF grant THEN @ \cup {permission} ELSE @ \ {permission}]
    /\ UNCHANGED <<userStatus, sessions, userRoles, userPermissions, socketBindings, configuredKeys>>

SetUserPermission(actor, user, permission, grant) ==
    /\ IsAdmin(actor) /\ user \in Users /\ permission \in Permissions /\ grant \in BOOLEAN
    /\ userPermissions' = [userPermissions EXCEPT ![user] =
        IF grant THEN @ \cup {permission} ELSE @ \ {permission}]
    /\ UNCHANGED <<userStatus, sessions, userRoles, rolePermissions, socketBindings, configuredKeys>>

ConnectSocket(user) ==
    /\ user \in sessions /\ userStatus[user] = "active"
    /\ socketBindings' = socketBindings \cup {user}
    /\ UNCHANGED <<userStatus, sessions, userRoles, rolePermissions, userPermissions, configuredKeys>>

DisconnectSocket(user) ==
    /\ user \in socketBindings
    /\ socketBindings' = socketBindings \ {user}
    /\ UNCHANGED <<userStatus, sessions, userRoles, rolePermissions, userPermissions, configuredKeys>>

WriteConfig(actor, key) ==
    /\ IsAdmin(actor) /\ key \in {"system", "security", "email"}
    /\ configuredKeys' = configuredKeys \cup {key}
    /\ UNCHANGED <<userStatus, sessions, userRoles, rolePermissions, userPermissions, socketBindings>>

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

Next == CommonAccountFlow \/ AccessFlow

TypeOK ==
    /\ userStatus \in [Users -> {"absent", "active", "disabled", "banned"}]
    /\ sessions \subseteq Users
    /\ userRoles \in [Users -> SUBSET Roles]
    /\ rolePermissions \in [Roles -> SUBSET Permissions]
    /\ userPermissions \in [Users -> SUBSET Permissions]
    /\ socketBindings \subseteq Users
    /\ configuredKeys \subseteq {"system", "security", "email"}
ActiveSessionsOnly == \A u \in sessions : userStatus[u] = "active"
SocketSessionsOnly == socketBindings \subseteq sessions
AdminAccountPreserved == userStatus[AdminUser] = "active" /\ AdminRole \in userRoles[AdminUser]
Safety == TypeOK /\ ActiveSessionsOnly /\ SocketSessionsOnly /\ AdminAccountPreserved

============================================================
