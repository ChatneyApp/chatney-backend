---------------------------- MODULE SystemQueries ----------------------------
EXTENDS State

CanReadAttachment(s, actor) ==
  IsAuthenticated(s, actor) /\ CanGlobal(s, actor, AttachmentRead)

CanReadConfig(s, actor) ==
  IsAuthenticated(s, actor) /\ CanGlobal(s, actor, ConfigRead)

CanReadRolesOrChannelTypes(s, actor) == IsAuthenticated(s, actor)

AttachmentLookup(s, actor, id) ==
  IF CanReadAttachment(s, actor)
  THEN {a \in s.attachments : a.id = id} ELSE {}

ConfigLookupById(s, actor, id) ==
  IF CanReadConfig(s, actor) THEN {c \in s.configs : c.id = id} ELSE {}

ConfigLookupByName(s, actor, name) ==
  IF CanReadConfig(s, actor) THEN {c \in s.configs : c.name = name} ELSE {}

=============================================================================
