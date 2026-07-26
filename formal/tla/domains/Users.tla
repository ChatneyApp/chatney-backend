-------------------------------- MODULE Users --------------------------------
EXTENDS State

ConfiguredRoleId(c) ==
  CASE c.configValue = "1" -> 1 [] c.configValue = "2" -> 2
    [] c.configValue = "3" -> 3 [] c.configValue = "4" -> 4
    [] c.configValue = "5" -> 5 [] c.configValue = "6" -> 6
    [] c.configValue = "7" -> 7 [] c.configValue = "8" -> 8
    [] OTHER -> 0

DefaultRoleConfigIds(s) ==
  {c.id : c \in {x \in s.configs :
                   x.name = "settings.NewUserDefaultRole"}}

ApplyRegisterWithConfig(s, user, selectedConfigId) ==
  LET selected == {c \in s.configs :
                    c.id = selectedConfigId /\
                    c.name = "settings.NewUserDefaultRole"}
      roleId == IF selected = {} THEN 0 ELSE ConfiguredRoleId(Only(selected))
      registered == [user EXCEPT !.roleId = roleId, !.active = FALSE,
                       !.verified = FALSE, !.banned = FALSE, !.muted = FALSE]
  IN IF ~ValidNickname(user.nickname) THEN FailedState(s, InvalidNickname)
     ELSE IF IsNicknameTaken(s, user.nickname, 0) THEN FailedState(s, NicknameTaken)
     ELSE IF UserExists(s, user.id) \/ IsEmailTaken(s, user.email, 0)
          THEN FailedState(s, DbConstraint)
     ELSE IF selected = {} \/ roleId = 0 \/ ~RoleExists(s, roleId)
          THEN FailedState(s, Failed)
     ELSE RecordUnpermissioned(
            s, [s EXCEPT !.users = @ \union {registered}],
            "user", "register", user.id, user.id, FALSE, 0)

ApplyLogin(s, login, passwordHash) ==
  LET matches == {u \in s.users :
        u.passwordHash = passwordHash /\
        (IdentityKey(u.email) = IdentityKey(login) \/
         IdentityKey(u.nickname) = IdentityKey(login))}
  IN IF matches = {} THEN FailedState(s, NullResult)
     ELSE [s EXCEPT
            !.issuedTokens = @ \union {u.id : u \in matches},
            !.lastResult = OK]

ApplyCreateUser(s, actorId, user) ==
  IF ~IsAuthenticated(s, actorId) \/ ~CanGlobal(s, actorId, UserCreate)
  THEN FailedState(s, Forbidden)
  ELSE IF ~ValidNickname(user.nickname) THEN FailedState(s, InvalidNickname)
  ELSE IF IsNicknameTaken(s, user.nickname, 0) THEN FailedState(s, NicknameTaken)
  ELSE IF UserExists(s, user.id) \/ IsEmailTaken(s, user.email, 0)
          \/ ~RoleExists(s, user.roleId)
       THEN FailedState(s, DbConstraint)
       ELSE RecordGlobal(s, [s EXCEPT !.users = @ \union {user}],
              "user", "create", user.id, actorId, UserCreate, FALSE, 0)

ApplyUpdateUser(s, actorId, user) ==
  IF ~IsAuthenticated(s, actorId) \/ ~CanGlobal(s, actorId, UserEdit)
  THEN FailedState(s, Forbidden)
  ELSE IF ~UserExists(s, user.id) THEN FailedState(s, NotFound)
  ELSE IF ~ValidNickname(user.nickname) THEN FailedState(s, InvalidNickname)
  ELSE IF IsNicknameTaken(s, user.nickname, user.id)
       THEN FailedState(s, NicknameTaken)
  ELSE IF IsEmailTaken(s, user.email, user.id) \/ ~RoleExists(s, user.roleId)
       THEN FailedState(s, DbConstraint)
       ELSE RecordGlobal(s, ReplaceUser(s, user), "user", "update",
              user.id, actorId, UserEdit, FALSE, 0)

ApplyUpdateProfile(s, actorId, nickname, fullName, setFullName, email,
                   avatarUrl, setAvatarUrl, currentPassword, newPassword) ==
  IF ~IsAuthenticated(s, actorId) THEN FailedState(s, Forbidden)
  ELSE LET old == Only(UsersById(s, actorId))
           nn == IF nickname = "" THEN old.nickname ELSE nickname
           nf == IF setFullName THEN fullName ELSE old.fullName
           ne == IF email = "" THEN old.email ELSE email
           na == IF setAvatarUrl THEN avatarUrl ELSE old.avatarUrl
           np == IF newPassword = "" THEN old.passwordHash ELSE newPassword
           updated == [old EXCEPT !.nickname = nn, !.fullName = nf,
                        !.email = ne, !.avatarUrl = na, !.passwordHash = np]
       IN IF ~ValidNickname(nn) THEN FailedState(s, InvalidNickname)
          ELSE IF IsNicknameTaken(s, nn, actorId) THEN FailedState(s, NicknameTaken)
          ELSE IF newPassword # "" /\ currentPassword # old.passwordHash
               THEN FailedState(s, Forbidden)
          ELSE IF IsEmailTaken(s, ne, actorId) THEN FailedState(s, DbConstraint)
          ELSE RecordUnpermissioned(s, ReplaceUser(s, updated), "user",
                 "profile", actorId, actorId, FALSE, 0)

ApplyDeleteUser(s, actorId, userId) ==
  IF ~IsAuthenticated(s, actorId) \/ ~CanGlobal(s, actorId, UserDelete)
  THEN FailedState(s, Forbidden)
  ELSE LET mids == {m.id : m \in {x \in s.messages : x.userId = userId}}
           dids == {d.id : d \in {x \in s.drafts : x.userId = userId}}
           mless == RemoveMessages(s, mids)
           dless == RemoveDrafts(mless, dids)
           after == [dless EXCEPT
             !.users = {u \in @ : u.id # userId},
             !.userRoles = {r \in @ : r.userId # userId},
             !.reactions = {r \in @ : r.userId # userId},
             !.attachments = {a \in @ : a.userId # userId},
             !.issuedTokens = @ \ {userId}]
       IN IF UserExists(s, userId)
          THEN RecordGlobal(s, after, "user", "delete", userId, actorId,
                 UserDelete, FALSE, 0)
          ELSE FailedState(after, FalseResult)

=============================================================================
