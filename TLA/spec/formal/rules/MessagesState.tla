---------------- MODULE MessagesState ----------------
EXTENDS Naturals, FiniteSets
CONSTANTS Users, AdminUser, RecordIds, NoUser, NoRecord
VARIABLES messages, messageOwner, messageKind, messageParent, publishedMessages
vars == <<messages, messageOwner, messageKind, messageParent, publishedMessages>>
MessageKinds == {"directMessage", "message", "draft", "attachment", "reaction"}
\* Installed scopes and authenticated, authorized actors are fixed environment fixtures.
IsAdmin(u) == u = AdminUser
CanCreate(u, kind) == u \in Users

Init ==
    /\ messages = {}
    /\ messageOwner = [r \in RecordIds |-> NoUser]
    /\ messageKind = [r \in RecordIds |-> "none"]
    /\ messageParent = [r \in RecordIds |-> NoRecord]
    /\ publishedMessages = {}

CreateMessage(actor, id, kind, parent) ==
    /\ id \in RecordIds \ messages
    /\ kind \in MessageKinds /\ CanCreate(actor, kind)
    /\ (parent = NoRecord \/ parent \in messages)
    /\ (kind # "draft" \/
        ~\E draft \in messages : messageKind[draft] = "draft"
            /\ messageOwner[draft] = actor /\ messageParent[draft] = parent)
    /\ messages' = messages \cup {id}
    /\ messageOwner' = [messageOwner EXCEPT ![id] = actor]
    /\ messageKind' = [messageKind EXCEPT ![id] = kind]
    /\ messageParent' = [messageParent EXCEPT ![id] = parent]
    /\ UNCHANGED <<publishedMessages>>

PublishMessage(actor, id) ==
    /\ id \in messages /\ messageOwner[id] = actor
    /\ actor \in Users
    /\ publishedMessages' = publishedMessages \cup {id}
    /\ UNCHANGED <<messages, messageOwner, messageKind, messageParent>>

DeleteMessage(actor, id) ==
    /\ id \in messages /\ actor \in Users
    /\ (messageOwner[id] = actor \/ IsAdmin(actor))
    /\ messages' = messages \ ({id} \cup {child \in messages : messageParent[child] = id})
    /\ publishedMessages' = publishedMessages \ ({id} \cup {child \in messages : messageParent[child] = id})
    /\ UNCHANGED <<messageOwner, messageKind, messageParent>>

SaveDraft(actor, id, scope) == CreateMessage(actor, id, "draft", scope)

Next ==
    (\E actor \in Users, id \in RecordIds, kind \in MessageKinds,
        parent \in (RecordIds \cup {NoRecord}) : CreateMessage(actor, id, kind, parent))
    \/ (\E actor \in Users, id \in RecordIds : PublishMessage(actor, id) \/ DeleteMessage(actor, id))
    \/ (\E actor \in Users, id \in RecordIds, scope \in (RecordIds \cup {NoRecord}) :
        SaveDraft(actor, id, scope))

TypeOK ==
    /\ messages \subseteq RecordIds
    /\ messageOwner \in [RecordIds -> (Users \cup {NoUser})]
    /\ messageKind \in [RecordIds -> (MessageKinds \cup {"none"})]
    /\ messageParent \in [RecordIds -> (RecordIds \cup {NoRecord})]
    /\ publishedMessages \subseteq messages
MessagesHaveOwnersAndValidParents ==
    \A id \in messages : messageOwner[id] \in Users
        /\ messageKind[id] \in MessageKinds
        /\ (messageParent[id] = NoRecord \/ messageParent[id] \in messages)

DraftsAreUniquePerOwnerAndScope ==
    \A u \in Users, scope \in (RecordIds \cup {NoRecord}) :
        Cardinality({id \in messages : messageKind[id] = "draft"
            /\ messageOwner[id] = u /\ messageParent[id] = scope}) <= 1
Safety == TypeOK /\ MessagesHaveOwnersAndValidParents /\ DraftsAreUniquePerOwnerAndScope

============================================================
