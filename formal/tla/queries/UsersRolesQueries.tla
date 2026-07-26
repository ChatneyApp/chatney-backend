-------------------------- MODULE UsersRolesQueries --------------------------
EXTENDS State, SystemQueries

CanReadUser(s, actor, target) ==
  IsAuthenticated(s, actor) /\ (actor = target \/ CanGlobal(s, actor, UserRead))

FilteredUsers(s, actor, activeFilter, bannedFilter, emailFilter, nicknameFilter) ==
  IF ~IsAuthenticated(s, actor) \/ ~CanGlobal(s, actor, UserRead) THEN {}
  ELSE {u \in s.users :
    (activeFilter = -1 \/ (activeFilter = 1 /\ u.active) \/
     (activeFilter = 0 /\ ~u.active)) /\
    (bannedFilter = -1 \/ (bannedFilter = 1 /\ u.banned) \/
     (bannedFilter = 0 /\ ~u.banned)) /\
    (emailFilter = "" \/ IdentityKey(u.email) = IdentityKey(emailFilter)) /\
    (nicknameFilter = "" \/
     IdentityKey(u.nickname) = IdentityKey(nicknameFilter))}

UsersByNicknameLookup(s, actor, nickname) ==
  LET matches == {u \in s.users :
        IdentityKey(u.nickname) = IdentityKey(nickname)}
  IN IF (\E u \in matches : u.id = actor) \/ CanGlobal(s, actor, UserRead)
     THEN matches ELSE {}

RoleLookupById(s, actor, id) ==
  IF CanReadRolesOrChannelTypes(s, actor) THEN RoleById(s, id) ELSE {}

RoleLookupByName(s, actor, name) ==
  IF CanReadRolesOrChannelTypes(s, actor)
  THEN {r \in s.roles : r.name = name} ELSE {}

MyGlobalPermissions(s, actor) ==
  IF IsAuthenticated(s, actor)
  THEN EffectivePermissions(s, actor, 0, 0, 0) ELSE {}

=============================================================================
