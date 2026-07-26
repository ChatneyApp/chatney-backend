------------------------------ MODULE Messages -------------------------------
EXTENDS State

IncrementParent(s, parentId, actor) ==
  IF parentId = 0 \/ ~MessageExists(s, parentId) THEN s
  ELSE LET p == Only(MessagesById(s, parentId))
           next == ReplaceMessage(s, [p EXCEPT !.childrenCount = @ + 1])
       IN RecordUnpermissioned(s, next, "messageChildrenCount", "update",
            parentId, actor, TRUE, p.childrenCount + 1)

DecrementParent(s, parentId, actor) ==
  IF parentId = 0 \/ ~MessageExists(s, parentId) THEN s
  ELSE LET p == Only(MessagesById(s, parentId))
           n == IF p.childrenCount > 0 THEN p.childrenCount - 1 ELSE 0
           next == ReplaceMessage(s, [p EXCEPT !.childrenCount = n])
       IN RecordUnpermissioned(s, next, "messageChildrenCount", "update",
            parentId, actor, TRUE, n)

PreviewInsertValid(s, previews) ==
  /\ Cardinality({p.id : p \in previews}) = Cardinality(previews)
  /\ \A p \in previews : ~ExistsId(s.urlPreviews, p.id)

PreviewIdsExist(s, previews, ids) ==
  ids \subseteq (IdsOf(s.urlPreviews) \union IdsOf(previews))

ApplyAddMessage(s, actor, message, previews) ==
  IF ~IsAuthenticated(s, actor) THEN FailedState(s, Forbidden)
  ELSE IF ~ChannelExists(s, message.channelId)
       THEN FailedState(s, InvalidChannelOrUser)
  ELSE LET channel == Only(ChannelsById(s, message.channelId))
       IN IF ~CanChannel(s, actor, channel, ChannelCreateMessage)
          THEN FailedState(s, Forbidden)
          ELSE IF message.id = 0 \/ MessageExists(s, message.id)
               THEN FailedState(s, DbConstraint)
          ELSE LET incremented == IncrementParent(s, message.parentId, actor)
               IN IF ~PreviewInsertValid(s, previews) \/
                     ~PreviewIdsExist(s, previews, message.urlPreviewIds)
                  THEN FailedState(incremented, DbConstraint)
                  ELSE LET withPreviews ==
                         [incremented EXCEPT !.urlPreviews = @ \union previews]
                       IN IF (message.parentId # 0 /\
                              ~MessageExists(s, message.parentId)) \/
                             (message.replyTo # 0 /\
                              ~MessageExists(s, message.replyTo))
                          THEN FailedState(withPreviews, DbConstraint)
                          ELSE LET inserted ==
                            [message EXCEPT !.userId = actor, !.status = "sent",
                             !.childrenCount = 0]
                            IN RecordScoped(s,
                              [withPreviews EXCEPT
                               !.messages = @ \union {inserted}],
                              "message", "create", message.id, actor,
                              ChannelCreateMessage, TRUE, channel.workspaceId,
                              channel.channelTypeId, channel.id, channel.id)

ApplyUpdateMessage(s, actor, id, content, attachmentIds, previewIds, previews) ==
  IF ~IsAuthenticated(s, actor) THEN FailedState(s, Forbidden)
  ELSE IF ~MessageExists(s, id) THEN FailedState(s, NotFound)
  ELSE LET old == Only(MessagesById(s, id))
       IN IF ~ChannelExists(s, old.channelId) THEN FailedState(s, NotFound)
          ELSE LET channel == Only(ChannelsById(s, old.channelId))
                   own == old.userId = actor
                   mayEdit == CanChannel(s, actor, channel, ChannelEditMessage)
                              \/ (own /\ CanChannel(
                                   s, actor, channel, ChannelEditOwnMessage))
                   changed == old.content # content
                   nextIds == IF changed THEN previewIds ELSE old.urlPreviewIds
                   nextPreviews == IF changed THEN previews ELSE {}
               IN IF ~mayEdit THEN FailedState(s, Forbidden)
                  ELSE IF ~PreviewInsertValid(s, nextPreviews) \/
                          ~PreviewIdsExist(s, nextPreviews, nextIds)
                       THEN FailedState(s, DbConstraint)
                  ELSE LET updated == [old EXCEPT !.content = content,
                             !.attachmentIds = attachmentIds,
                             !.urlPreviewIds = nextIds]
                           after == [ReplaceMessage(s, updated) EXCEPT
                             !.urlPreviews = @ \union nextPreviews]
                       IN RecordScoped(s, after, "message", "update", id, actor,
                            IF own THEN ChannelEditOwnMessage
                            ELSE ChannelEditMessage,
                            TRUE, channel.workspaceId, channel.channelTypeId,
                            channel.id, channel.id)

ApplyDeleteMessage(s, actor, id) ==
  IF ~IsAuthenticated(s, actor) THEN FailedState(s, Forbidden)
  ELSE IF ~MessageExists(s, id) THEN FailedState(s, NotFound)
  ELSE LET m == Only(MessagesById(s, id))
       IN IF ~ChannelExists(s, m.channelId) THEN FailedState(s, NotFound)
          ELSE LET c == Only(ChannelsById(s, m.channelId))
                   own == m.userId = actor
                   mayDelete == CanChannel(s, actor, c, ChannelDeleteMessage)
                     \/ (own /\ CanChannel(s, actor, c, ChannelDeleteOwnMessage))
               IN IF ~mayDelete THEN FailedState(s, Forbidden)
                  ELSE LET counted == IF m.parentId = 0 THEN s
                                      ELSE DecrementParent(s, m.parentId, actor)
                       IN RecordScoped(s, RemoveMessages(counted, {id}),
                            "message", "delete", id, actor,
                            IF own THEN ChannelDeleteOwnMessage
                            ELSE ChannelDeleteMessage,
                            TRUE, c.workspaceId, c.channelTypeId, c.id, c.id)

ApplyAddReaction(s, actor, id, code) ==
  IF ~MessageExists(s, id) THEN FailedState(s, WrongMessageId)
  ELSE LET m == Only(MessagesById(s, id))
       IN IF ~ChannelExists(s, m.channelId) THEN FailedState(s, ChannelNotFound)
          ELSE IF ~UserExists(s, actor) THEN FailedState(s, DbConstraint)
          ELSE LET r == [messageId |-> id, userId |-> actor, code |-> code]
               IN RecordUnpermissioned(s,
                    [s EXCEPT !.reactions = @ \union {r}],
                    "reaction", "create", id, actor, TRUE, m.channelId)

ApplyDeleteReaction(s, actor, id, code) ==
  IF ~MessageExists(s, id) THEN FailedState(s, WrongMessageId)
  ELSE LET m == Only(MessagesById(s, id))
           r == [messageId |-> id, userId |-> actor, code |-> code]
       IN IF ~ChannelExists(s, m.channelId) THEN FailedState(s, ChannelNotFound)
          ELSE IF r \notin s.reactions THEN FailedState(s, ReactionNotFound)
          ELSE RecordUnpermissioned(s,
                 [s EXCEPT !.reactions = @ \ {r}],
                 "reaction", "delete", id, actor, TRUE, m.channelId)

=============================================================================
