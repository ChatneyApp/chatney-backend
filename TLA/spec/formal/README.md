# Chatney business workflow model

This is a high-level bounded architecture model for the backend's main business workflows. It deliberately abstracts database rows, GraphQL fields, object payloads, and infrastructure APIs into a small set of lifecycle, identity, permission, record, and publication state.

## Reading order

1. `rules/ChatneyState.tla` defines the state and domain operations.
2. `workflows/ChatneyArchitecture.tla` composes the rules and defines shared safety properties.
3. `workflows/ChatneyMain.tla` is the TLC entry module.
4. `../../test/formal/config/ChatneyMain.cfg` defines the finite exploration bound.

Run SANY and TLC from the backend repository root. The runner parses every module, then checks four separate state graphs:

```sh
TLA/test/formal/install-tlc.sh
TLA/test/formal/sanity.sh
```

Or set `TLA2TOOLS_JAR` to a verified TLA+ Tools 1.8.0 jar. Java 11 or newer is required.

## Flow coverage

- **Setup:** start, migrate, seed/commit, fail/retry, uninstall.
- **Identity and access:** registration, login/logout, account status, role assignment, role/user permissions.
- **Collaboration records:** workspace/channel/DM/message/draft/attachment/reaction creation as typed records, update ownership, publication, deletion, parent relationships.
- **Account recovery:** issue, replace, consume, and invalidate reset tokens.
- **Realtime and settings:** authenticated socket binding and privileged configuration writes.

The model checks type correctness, valid record ownership and parent links, draft uniqueness per owner and scope, setup/admin consistency, active sessions, socket/session binding, and single-use reset tokens.

## Abstraction notes

- This is a proposed safety contract, not proof that the current C# implementation satisfies it. The model requires admin authorization for uninstall and authenticated socket ownership; the current backend code does not enforce both.
- Each TLC run explores two users, two roles, two generic record IDs, and two reset tokens. That is enough to explore competing operations, duplicate IDs, retries, and single-use behavior in a finite state space.
- TLC symmetry reduction permutes the record IDs and reset tokens because those values have no special meaning in the model.
- `ChatneySetup.cfg` checks install, failure, retry, and commit transitions.
- `ChatneyMain.cfg` checks account/access transitions, with setup and common account prerequisites.
- `ChatneyCollaboration.cfg` checks generic collaboration records, publication, and ownership, with setup/account prerequisites.
- `ChatneyRecovery.cfg` checks reset-token recovery, with setup/account prerequisites.
- Every graph uses the same safety invariants and finite bounds. Setup is isolated to prevent its state history multiplying unrelated workflows.
- Deadlock checking is enabled for access, collaboration, and recovery. It is disabled only for the setup graph, whose successful `installed` state is an intentional terminal state.
- Record kinds collapse several concrete domains into generic create, publish, and delete transitions. Per-object ACL inheritance, channel membership, groups, message counts, reaction deduplication, attachment storage, email delivery, timestamp expiry, transaction isolation, and GraphQL selection behavior are intentionally excluded.
- Setup steps are separate transitions so competing installers and failure/retry states remain visible. The concrete DB writes within a step are abstracted.
- TLC verifies this bounded model only; it does not prove implementation conformance or unbounded correctness. No runtime instrumentation or trace replay is included.
