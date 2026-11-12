---------------- MODULE MessagesState ----------------
EXTENDS Naturals, FiniteSets
CONSTANTS Users, RecordIds, NoUser, NoRecord, MessageOperation
VARIABLES messages, messageOwner, messageKind, messageParent, publishedMessages,
    editedMessages, actorAccess, mutation
messageVars == <<messages, messageOwner, messageKind, messageParent, publishedMessages,
    editedMessages, mutation>>
vars == <<messageVars, actorAccess>>
MessageKinds == {"directMessage", "message", "draft", "attachment", "reaction"}
\* Effective rights for the operation under test; role/ACL origin is abstracted.
AccessProfiles == {"anonymous", "denied", "own", "any", "disabled", "banned"}
Authenticated(actor) == actorAccess[actor] \in {"denied", "own", "any"}
CanCreate(actor, kind) ==
    Authenticated(actor) /\
        (MessageOperation # "create" \/ actorAccess[actor] \in {"own", "any"})
CanMutate(actor, id, operation) ==
    Authenticated(actor) /\
        (MessageOperation # operation \/ actorAccess[actor] = "any"
            \/ (actorAccess[actor] = "own" /\ messageOwner[id] = actor))

ChangeActorAccess(actor, profile) ==
    /\ profile \in AccessProfiles
    /\ actorAccess' = [actorAccess EXCEPT ![actor] = profile]
    /\ UNCHANGED messageVars

Init ==
    /\ messages = {}
    /\ messageOwner = [r \in RecordIds |-> NoUser]
    /\ messageKind = [r \in RecordIds |-> "none"]
    /\ messageParent = [r \in RecordIds |-> NoRecord]
    /\ publishedMessages = {}
    /\ editedMessages = {}
    /\ actorAccess = [actor \in Users |-> "denied"]
    /\ mutation = [actor |-> NoUser, operation |-> "none", owner |-> NoUser]

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
    /\ mutation' = [actor |-> actor, operation |-> "create", owner |-> actor]
    /\ UNCHANGED <<publishedMessages, editedMessages, actorAccess>>

PublishMessage(actor, id) ==
    /\ id \in messages /\ messageOwner[id] = actor
    \* Completing notification of an already persisted record needs no fresh grant.
    /\ actor \in Users
    /\ publishedMessages' = publishedMessages \cup {id}
    /\ UNCHANGED <<messages, messageOwner, messageKind, messageParent,
        editedMessages, actorAccess, mutation>>

DeleteMessage(actor, id) ==
    /\ id \in messages /\ CanMutate(actor, id, "delete")
    /\ messages' = messages \ ({id} \cup {child \in messages : messageParent[child] = id})
    /\ publishedMessages' = publishedMessages \ ({id} \cup {child \in messages : messageParent[child] = id})
    /\ editedMessages' = editedMessages \ ({id} \cup
        {child \in messages : messageParent[child] = id})
    /\ mutation' = [actor |-> actor, operation |-> "delete", owner |-> messageOwner[id]]
    /\ UNCHANGED <<messageOwner, messageKind, messageParent, actorAccess>>

EditMessage(actor, id) ==
    /\ id \in messages /\ CanMutate(actor, id, "edit")
    \* Toggle an abstract content revision so repeated edits remain observable.
    /\ editedMessages' = IF id \in editedMessages
        THEN editedMessages \ {id} ELSE editedMessages \cup {id}
    /\ mutation' = [actor |-> actor, operation |-> "edit", owner |-> messageOwner[id]]
    /\ UNCHANGED <<messages, messageOwner, messageKind, messageParent,
        publishedMessages, actorAccess>>

ReadMessage(actor, id) ==
    /\ id \in messages /\ Authenticated(actor)
    /\ (MessageOperation # "read" \/ actorAccess[actor] \in {"own", "any"})
    /\ mutation' = [actor |-> actor, operation |-> "read", owner |-> messageOwner[id]]
    /\ UNCHANGED <<messages, messageOwner, messageKind, messageParent,
        publishedMessages, editedMessages, actorAccess>>

SaveDraft(actor, id, scope) == CreateMessage(actor, id, "draft", scope)

Next ==
    (\E actor \in Users, id \in RecordIds, kind \in MessageKinds,
        parent \in (RecordIds \cup {NoRecord}) : CreateMessage(actor, id, kind, parent))
    \/ (\E actor \in Users, id \in RecordIds : PublishMessage(actor, id) \/ ReadMessage(actor, id)
        \/ EditMessage(actor, id) \/ DeleteMessage(actor, id))
    \/ (\E actor \in Users, id \in RecordIds, scope \in (RecordIds \cup {NoRecord}) :
        SaveDraft(actor, id, scope))

    \/ (\E actor \in Users, profile \in AccessProfiles : ChangeActorAccess(actor, profile))

TypeOK ==
    /\ messages \subseteq RecordIds
    /\ messageOwner \in [RecordIds -> (Users \cup {NoUser})]
    /\ messageKind \in [RecordIds -> (MessageKinds \cup {"none"})]
    /\ messageParent \in [RecordIds -> (RecordIds \cup {NoRecord})]
    /\ publishedMessages \subseteq messages
    /\ editedMessages \subseteq messages
    /\ actorAccess \in [Users -> AccessProfiles]
    /\ mutation \in [actor : Users \cup {NoUser},
        operation : {"none", "create", "read", "edit", "delete"}, owner : Users \cup {NoUser}]
MessagesHaveOwnersAndValidParents ==
    \A id \in messages : messageOwner[id] \in Users
        /\ messageKind[id] \in MessageKinds
        /\ (messageParent[id] = NoRecord \/ messageParent[id] \in messages)

DraftsAreUniquePerOwnerAndScope ==
    \A u \in Users, scope \in (RecordIds \cup {NoRecord}) :
        Cardinality({id \in messages : messageKind[id] = "draft"
            /\ messageOwner[id] = u /\ messageParent[id] = scope}) <= 1
Safety == TypeOK /\ MessagesHaveOwnersAndValidParents /\ DraftsAreUniquePerOwnerAndScope

\* Check authorization against the state BEFORE the write, independently of action guards.
\* Later revocation does not retroactively invalidate an earlier authorized write.
AuthorizationStep ==
    (UNCHANGED <<messages, messageOwner, messageKind, messageParent, editedMessages, mutation>>)
    \/ /\ mutation'.actor \in Users
       /\ actorAccess[mutation'.actor] \in {"denied", "own", "any"}
       /\ (mutation'.operation # MessageOperation
           \/ actorAccess[mutation'.actor] = "any"
           \/ (actorAccess[mutation'.actor] = "own" /\
               (mutation'.operation \in {"create", "read"} \/ mutation'.actor = mutation'.owner)))
PermissionsRespected == [][AuthorizationStep]_vars

============================================================
