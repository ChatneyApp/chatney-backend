------------------------------- MODULE Configs -------------------------------
EXTENDS State

ApplyUpdateConfig(s, actor, config) ==
  IF ~IsAuthenticated(s, actor) \/ ~CanGlobal(s, actor, ConfigUpdate)
  THEN FailedState(s, Forbidden)
  ELSE IF ~ConfigExists(s, config.id) THEN FailedState(s, NullResult)
  ELSE RecordGlobal(s, ReplaceConfig(s, config), "config", "update",
         config.id, actor, ConfigUpdate, FALSE, 0)

=============================================================================
