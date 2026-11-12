---------------- MODULE WorkspacesState ----------------
EXTENDS Naturals, FiniteSets
CONSTANTS Users, AdminUser, RecordIds, NoUser, NoRecord
VARIABLES workspaces, workspaceOwner, workspaceKind, workspaceParent, publishedWorkspaces
vars == <<workspaces, workspaceOwner, workspaceKind, workspaceParent, publishedWorkspaces>>
WorkspaceKinds == {"workspace", "channel"}
\* Installed scopes and authenticated, authorized actors are fixed environment fixtures.
IsAdmin(u) == u = AdminUser
CanCreate(u, kind) == u \in Users /\ IsAdmin(u)

Init ==
    /\ workspaces = {}
    /\ workspaceOwner = [r \in RecordIds |-> NoUser]
    /\ workspaceKind = [r \in RecordIds |-> "none"]
    /\ workspaceParent = [r \in RecordIds |-> NoRecord]
    /\ publishedWorkspaces = {}

CreateWorkspace(actor, id, kind, parent) ==
    /\ id \in RecordIds \ workspaces
    /\ kind \in WorkspaceKinds /\ CanCreate(actor, kind)
    /\ IF kind = "workspace" THEN parent = NoRecord
        ELSE parent \in workspaces /\ workspaceKind[parent] = "workspace"
    /\ workspaces' = workspaces \cup {id}
    /\ workspaceOwner' = [workspaceOwner EXCEPT ![id] = actor]
    /\ workspaceKind' = [workspaceKind EXCEPT ![id] = kind]
    /\ workspaceParent' = [workspaceParent EXCEPT ![id] = parent]
    /\ UNCHANGED <<publishedWorkspaces>>

PublishWorkspace(actor, id) ==
    /\ id \in workspaces /\ workspaceOwner[id] = actor
    /\ actor \in Users
    /\ publishedWorkspaces' = publishedWorkspaces \cup {id}
    /\ UNCHANGED <<workspaces, workspaceOwner, workspaceKind, workspaceParent>>

DeleteWorkspace(actor, id) ==
    /\ id \in workspaces /\ actor \in Users
    /\ (workspaceOwner[id] = actor \/ IsAdmin(actor))
    /\ workspaces' = workspaces \ ({id} \cup {child \in workspaces : workspaceParent[child] = id})
    /\ publishedWorkspaces' = publishedWorkspaces \ ({id} \cup {child \in workspaces : workspaceParent[child] = id})
    /\ UNCHANGED <<workspaceOwner, workspaceKind, workspaceParent>>

Next ==
    (\E actor \in Users, id \in RecordIds, kind \in WorkspaceKinds,
        parent \in (RecordIds \cup {NoRecord}) : CreateWorkspace(actor, id, kind, parent))
    \/ (\E actor \in Users, id \in RecordIds : PublishWorkspace(actor, id) \/ DeleteWorkspace(actor, id))

TypeOK ==
    /\ workspaces \subseteq RecordIds
    /\ workspaceOwner \in [RecordIds -> (Users \cup {NoUser})]
    /\ workspaceKind \in [RecordIds -> (WorkspaceKinds \cup {"none"})]
    /\ workspaceParent \in [RecordIds -> (RecordIds \cup {NoRecord})]
    /\ publishedWorkspaces \subseteq workspaces
WorkspacesHaveOwnersAndValidParents ==
    \A id \in workspaces : workspaceOwner[id] \in Users
        /\ workspaceKind[id] \in WorkspaceKinds
        /\ (workspaceParent[id] = NoRecord \/ workspaceParent[id] \in workspaces)

WorkspaceHierarchy ==
    \A id \in workspaces :
        IF workspaceKind[id] = "workspace" THEN workspaceParent[id] = NoRecord
        ELSE workspaceParent[id] \in workspaces
            /\ workspaceKind[workspaceParent[id]] = "workspace"
Safety == TypeOK /\ WorkspacesHaveOwnersAndValidParents /\ WorkspaceHierarchy

============================================================
