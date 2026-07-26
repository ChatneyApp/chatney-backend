-------------------------------- MODULE Roles --------------------------------
EXTENDS State

ApplyAddRole(s, actorId, role) ==
  IF ~IsAuthenticated(s, actorId) \/ ~CanGlobal(s, actorId, RoleCreate)
  THEN FailedState(s, Forbidden)
  ELSE IF RoleExists(s, role.id) THEN FailedState(s, DbConstraint)
  ELSE RecordGlobal(s, [s EXCEPT !.roles = @ \union {role}],
         "role", "create", role.id, actorId, RoleCreate, TRUE, 0)

ApplyUpdateRole(s, actorId, role) ==
  IF ~IsAuthenticated(s, actorId) \/ ~CanGlobal(s, actorId, RoleEdit)
  THEN FailedState(s, Forbidden)
  ELSE IF ~RoleExists(s, role.id) THEN FailedState(s, NotFound)
  ELSE RecordGlobal(s, ReplaceRole(s, role), "role", "update",
         role.id, actorId, RoleEdit, TRUE, 0)

ApplyDeleteRole(s, actorId, roleId) ==
  IF ~IsAuthenticated(s, actorId) \/ ~CanGlobal(s, actorId, RoleDelete)
  THEN FailedState(s, Forbidden)
  ELSE IF \E u \in s.users : u.roleId = roleId
       THEN FailedState(s, DbConstraint)
  ELSE LET after == [s EXCEPT
          !.roles = {r \in @ : r.id # roleId},
          !.userRoles = {r \in @ : r.roleId # roleId}]
       IN IF RoleExists(s, roleId)
          THEN RecordGlobal(s, after, "role", "delete", roleId, actorId,
                 RoleDelete, TRUE, 0)
          ELSE FailedState(after, FalseResult)

UserRoleReferencesExist(s, row) ==
  /\ UserExists(s, row.userId) /\ RoleExists(s, row.roleId)
  /\ (row.channelId = 0 \/ ChannelExists(s, row.channelId))
  /\ (row.channelTypeId = 0 \/ ChannelTypeExists(s, row.channelTypeId))
  /\ (row.workspaceId = 0 \/ WorkspaceExists(s, row.workspaceId))

ApplyAddUserRole(s, actorId, row) ==
  IF ~IsAuthenticated(s, actorId) \/ ~CanGlobal(s, actorId, UserEdit)
  THEN FailedState(s, Forbidden)
  ELSE IF UserRoleExists(s, row) THEN FailedState(s, NotFound)
  ELSE IF ~UserRoleReferencesExist(s, row) THEN FailedState(s, DbConstraint)
  ELSE RecordGlobal(s, [s EXCEPT !.userRoles = @ \union {row}],
         "userRole", "create", row.userId, actorId, UserEdit, TRUE, row.roleId)

ApplyUpdateUserRole(s, actorId, row) ==
  IF ~IsAuthenticated(s, actorId) \/ ~CanGlobal(s, actorId, UserEdit)
  THEN FailedState(s, Forbidden)
  ELSE IF ~UserRoleExists(s, row) THEN FailedState(s, NotFound)
  ELSE IF ~UserRoleReferencesExist(s, row) THEN FailedState(s, DbConstraint)
  ELSE RecordGlobal(s, ReplaceUserRole(s, row), "userRole", "update",
         row.userId, actorId, UserEdit, TRUE, row.roleId)

ApplyDeleteUserRole(s, actorId, key) ==
  IF ~IsAuthenticated(s, actorId) \/ ~CanGlobal(s, actorId, UserEdit)
  THEN FailedState(s, Forbidden)
  ELSE LET after == [s EXCEPT
          !.userRoles = {r \in @ : ~UserRoleSameKey(r, key)}]
       IN IF UserRoleExists(s, key)
          THEN RecordGlobal(s, after, "userRole", "delete", key.userId,
                 actorId, UserEdit, TRUE, key.roleId)
          ELSE FailedState(after, FalseResult)

=============================================================================
