# Chatney business workflows

The model is split by business functionality. Each state module owns its domain variables, transitions, and safety properties. Each workflow has an independent TLC entry point and configuration.

| Domain | State module | Workflow / TLC configuration | Coverage |
| --- | --- | --- | --- |
| Users | `rules/UsersState.tla` | `UsersWorkflow` | Registration, login/logout, account status, roles, permissions, socket binding, privileged settings |
| Messages | `rules/MessagesState.tla` | `MessagesWorkflow` | Messages, DMs, drafts, attachments, reactions, ownership, publication, deletion |
| Workspaces | `rules/WorkspacesState.tla` | `WorkspacesWorkflow` | Workspace/channel creation, hierarchy, ownership, publication, deletion |
| Setup | `rules/SetupState.tla` | `SetupWorkflow` | Migration, admin-role sentinel, seed completion, partial failure, retry, uninstall |
| Recovery | `rules/RecoveryState.tla` | `RecoveryWorkflow` | Reset requests, replacement, consumption, invalidation, account status changes |

Configurations are in `TLA/test/formal/config/`. `workflows/ChatneyArchitecture.tla` shows composition through named instances with explicit state bindings and exposes `Init`, `Next`, `Spec`, and `Safety`. It is parsed by SANY; the runner checks the five domain workflows separately, not their full product.

## Running checks

From the backend repository root:

```sh
TLA/test/formal/install-tlc.sh
TLA/test/formal/sanity.sh
# Check one workflow after parsing all modules:
TLA/test/formal/sanity.sh Messages
TLA/test/formal/sanity.sh Users
TLA/test/formal/sanity.sh Workspaces
```

`Setup` and `Recovery` are also accepted. Or set `TLA2TOOLS_JAR` to a verified TLA+ Tools 1.8.0 jar. Java 11 or newer is required. The installer pins the jar checksum. Deadlock checking is enabled in every configuration.

## Bounds and environment assumptions

- Users and recovery use two users, two roles, and two permissions. Users start with a seeded active admin and no sessions; other accounts can register. Recovery adds two reset tokens and composes only registration, session, and account-status transitions with token state.
- Messages and workspaces each use two actors and two record IDs. Their state is separate: a message ID never denotes a workspace in the same domain. Symmetry permutes IDs and reset tokens only.
- Messages assume an installed system, existing external scopes, and authenticated actors authorized to create records. Workspaces assume an installed system with fixed administrative creation capability. Changes to accounts and permissions are checked in the users workflow, without multiplying the message/workspace graphs. These assumptions do not establish cross-domain authorization correctness.
- Setup abstracts seed completion as `adminReady`; it does not explore every seeded account, ACL, channel, or configuration value. Uninstall resets lifecycle state and allows recovery from partial installations.
- Payload edits and profile changes are abstract stuttering steps: content, timestamps, avatars, and bio are outside the state. Message-related kinds share a high-level lifecycle. Workspace kinds distinguish root workspaces from their channels.

## Properties and limits

The checks cover types, active sessions, socket/session binding, admin preservation, record ownership and valid parents, workspace hierarchy, draft uniqueness per owner/scope, setup readiness after completed seeding, token ownership, and disjoint live/used tokens. The two-ID bound explores a parent and child; deeper message trees are not verified.

This is a bounded architecture contract, not proof of C# implementation conformance or unbounded correctness. Active-only sessions, socket ownership, admin preservation, record ownership/publication, cascade deletion, and draft uniqueness include proposed requirements or abstractions. Per-object ACL inheritance, membership, groups, message counts, reaction deduplication, storage, email delivery, expiry, and transaction isolation remain excluded. No runtime instrumentation or trace replay is included.

## Implementation mapping

Reviewed against the PR base `main` at `3082f79`. The PR changes only `TLA/`.

- `Domains/Users/UserMutations.cs` and `UserLookup.cs`: the users workflow models default-role registration and credential-based login at the lifecycle level. Active-only login, session revocation, and admin-account protection are proposed requirements; the backend does not enforce all of them. Role and permission changes remain independently explorable.
- `Domains/Messages/MessageMutations.cs`, direct-message, draft, and attachment mutations: create, publish, and delete model lifecycle boundaries and ownership. Object-specific permission resolution, payload updates, and individual transport/storage operations are abstracted.
- `Domains/Workspaces/WorkspaceMutations.cs` and channel mutations: workspace/channel lifecycles are separate from message state. A channel must have a workspace parent; deleting a workspace abstracts deletion of its channels. This is a contract abstraction, not proof of database cascade behavior. Publication abstracts notification after persistence.
- `Domains/InstallWizard/InstallWizardMutations.cs`: migrations precede the persisted admin-role sentinel. A failure after that sentinel can cause retry to report `installed` without finishing seed. The setup graph preserves `failedPartial` and `incomplete` for this early failure window. On `main`, retry also calls `MarkInstalled`, so `InstallWizardQueries.IsInstalled` can be true for incomplete setup. Model phase `installed` means completed seed, not that API response or flag. Valid installer options are assumed; rejected options have no modeled effect. Repeated successful calls stutter.
- Uninstall has no authorization guard in the current mutation. The model reflects that. Rollback failures, concurrent installers, and failures between remaining seed writes are excluded; no automatic rollback of seed writes is assumed. `SetupStateConsistent` does not claim a reported installation flag proves readiness.
- `Domains/Users/UserMutations.cs`: reset requests replace earlier requests. Completion consumes a code and invalidates the user's remaining requests. These are successful sequential outcomes; concurrent requests and token-consumption/password-write failure windows are excluded.
- `Domains/Users/PasswordResetEmailSender.cs`: persisted SMTP settings and fallback options are outside the state. Delivery failure does not undo the persisted request. Mandatory first-login password rotation is not modeled because the backend does not enforce it.
