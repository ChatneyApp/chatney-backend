--------------------------- MODULE MessageQueries ----------------------------
EXTENDS State, OrganizationQueries

RootMessageIds(s, actor, channelId) ==
  IF CanReadMessages(s, actor, channelId)
  THEN {m.id : m \in {x \in s.messages :
          x.channelId = channelId /\ x.parentId = 0}}
  ELSE {}

ThreadMessageIds(s, actor, threadId) ==
  IF MessageExists(s, threadId) /\
     (\A t \in MessagesById(s, threadId) :
       CanReadMessages(s, actor, t.channelId))
  THEN {m.id : m \in {x \in s.messages : x.parentId = threadId}} ELSE {}

HydratedMessageIds(s, ids) ==
  {m.id : m \in {x \in s.messages :
    x.id \in ids /\ UserExists(s, x.userId)}}

ReactionAggregates(s, ids) ==
  LET selected == {r \in s.reactions : r.messageId \in ids}
  IN {[messageId |-> r.messageId, code |-> r.code,
       count |-> Cardinality({x \in selected :
         x.messageId = r.messageId /\ x.code = r.code})] : r \in selected}

MyReactionCodes(s, actor, id) ==
  {r.code : r \in {x \in s.reactions :
    x.messageId = id /\ x.userId = actor}}

ReplyReferenceIds(s, ids) ==
  {m.replyTo : m \in {x \in s.messages :
    x.id \in ids /\ x.replyTo # 0 /\ MessageExists(s, x.replyTo)}}

=============================================================================
