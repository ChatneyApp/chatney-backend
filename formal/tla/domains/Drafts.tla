------------------------------- MODULE Drafts --------------------------------
EXTENDS State

DraftSameLogicalKey(a, b) ==
  a.userId = b.userId /\ a.channelId = b.channelId /\ a.parentId = b.parentId

ApplyUpsertDraft(s, actor, draft) ==
  IF ~IsAuthenticated(s, actor) THEN FailedState(s, Forbidden)
  ELSE IF ~ChannelExists(s, draft.channelId)
       THEN FailedState(s, InvalidChannelOrUser)
  ELSE LET c == Only(ChannelsById(s, draft.channelId))
           matches == {d \in s.drafts :
             d.userId = actor /\ d.channelId = draft.channelId /\
             d.parentId = draft.parentId}
       IN IF ~CanChannel(s, actor, c, ChannelCreateMessage)
          THEN FailedState(s, Forbidden)
          ELSE IF matches # {}
               THEN LET old == Only(matches)
                        updated == [old EXCEPT !.content = draft.content,
                          !.attachmentIds = draft.attachmentIds]
                    IN RecordScoped(s, ReplaceDraft(s, updated), "draft",
                         "upsert", old.id, actor, ChannelCreateMessage, FALSE,
                         c.workspaceId, c.channelTypeId, c.id, c.id)
          ELSE IF DraftExists(s, draft.id) \/
                  (draft.parentId # 0 /\ ~DraftExists(s, draft.parentId))
               THEN FailedState(s, DbConstraint)
          ELSE LET inserted == [draft EXCEPT !.userId = actor]
               IN RecordScoped(s, [s EXCEPT !.drafts = @ \union {inserted}],
                    "draft", "upsert", draft.id, actor, ChannelCreateMessage,
                    FALSE, c.workspaceId, c.channelTypeId, c.id, c.id)

ApplyDeleteDraft(s, actor, id) ==
  IF ~IsAuthenticated(s, actor) THEN FailedState(s, Forbidden)
  ELSE IF ~DraftExists(s, id) THEN FailedState(s, FalseResult)
  ELSE LET d == Only(DraftsById(s, id))
       IN IF d.userId # actor \/ ~ChannelExists(s, d.channelId)
          THEN FailedState(s, FalseResult)
          ELSE LET c == Only(ChannelsById(s, d.channelId))
               IN IF ~CanChannel(s, actor, c, ChannelRead)
                  THEN FailedState(s, Forbidden)
                  ELSE RecordScoped(s, RemoveDrafts(s, {id}), "draft",
                         "delete", id, actor, ChannelRead, FALSE,
                         c.workspaceId, c.channelTypeId, c.id, c.id)

=============================================================================
