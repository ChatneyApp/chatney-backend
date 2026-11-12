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

- **Setup:** start, migrate, persist the admin-role sentinel, finish seeding, fail/retry, uninstall.
- **Identity and access:** registration, login/logout, account status, role assignment, role/user permissions.
- **Collaboration records:** workspace/channel/DM/message/draft/attachment/reaction creation as typed records, update ownership, publication, deletion, parent relationships.
- **Account recovery:** issue, replace, consume, and invalidate reset tokens.
- **Realtime and settings:** authenticated socket binding and privileged configuration writes.

The model checks type correctness, valid record ownership and parent links, draft uniqueness per owner and scope, setup/admin consistency, active sessions, socket/session binding, and single-use reset tokens.

## Abstraction notes

- This is a proposed safety contract, not proof that the current C# implementation satisfies it. Uninstall is modeled without authorization, matching the current mutation. Authenticated socket ownership and active-only sessions remain proposed requirements; the backend does not enforce them consistently (including login through `UserLookup`).
- Each TLC run explores two users, two roles, two generic record IDs, and two reset tokens. That is enough to explore competing operations, duplicate IDs, retries, and single-use behavior in a finite state space.
- TLC symmetry reduction permutes the record IDs and reset tokens because those values have no special meaning in the model.
- `ChatneySetup.cfg` checks install, failure, retry, and commit transitions.
- `ChatneyMain.cfg` checks account/access transitions, with setup and common account prerequisites.
- `ChatneyCollaboration.cfg` checks generic collaboration records, publication, and ownership, with setup/account prerequisites.
- `ChatneyRecovery.cfg` checks reset-token recovery, with setup/account prerequisites.
- Every graph uses the same safety invariants and finite bounds. Setup is isolated to prevent its state history multiplying unrelated workflows.
- Deadlock checking is enabled for all four graphs. Uninstall allows setup recovery from complete and partial installations.
- Record kinds collapse several concrete domains into generic create, publish, and delete transitions. Per-object ACL inheritance, channel membership, groups, message counts, reaction deduplication, attachment storage, email delivery, timestamp expiry, transaction isolation, and GraphQL selection behavior are intentionally excluded.
- Setup steps are separate transitions so competing installers and failure/retry states remain visible. The concrete DB writes within a step are abstracted.
- TLC verifies this bounded model only; it does not prove implementation conformance or unbounded correctness. No runtime instrumentation or trace replay is included.

## Implementation mapping and limits

Reviewed against the PR base `main` at `3082f79`, including the configurable installer, installation-status query, SMTP configuration lookup, and profile/avatar changes. The PR remains limited to `TLA/`.

- `ChatneyBackend/Domains/InstallWizard/InstallWizardMutations.cs`: `StartSetup`/`MigrationsComplete` abstract migrations and type reload. `CreateAdminRole` separates the persisted retry sentinel from `CommitSetup`, which abstracts the remaining seed writes. A failure after that sentinel followed by retry reaches `incomplete`: the API returns `installed`, but an admin account/configuration may still be missing. On `main`, that retry also calls `MarkInstalled`, so `InstallWizardQueries.IsInstalled` can return true for this incomplete state. Model phase `installed` means seed completion, not that API status string or configuration marker. Repeated successful installs are abstracted as stuttering. Valid installer input is an environment assumption; rejected options have no modeled effect.
- `UninstallSystem` abstracts migration rollback and clearing process configuration. Rollback failures, concurrent installers, and failures between individual remaining seed writes are excluded; `failedPartial` represents the early failure window before users/configuration are persisted. No transaction or automatic rollback of seed writes is assumed. `SetupStateConsistent` applies only to fully completed seeding; it does not claim that an `installed` API response proves readiness.
- `ChatneyBackend/Domains/Users/UserMutations.cs`: registration assigns the configured default role. `RequestPasswordReset` replaces earlier requests; `CompletePasswordReset` consumes the code and invalidates all other requests belonging to that user. These are successful sequential outcomes: concurrent reset requests, token-consumption/password-write failures, and email delivery are excluded. The token ownership and single-use invariants describe the bounded contract, not proof of atomic backend transactions.
- `ChatneyBackend/Domains/Users/PasswordResetEmailSender.cs`: SMTP now reads persisted settings with fallback options. Configuration selection and delivery have no modeled state; delivery failure does not undo the persisted reset request and is abstracted as stuttering.
- Setup inputs, seeded channel variants, profile fields, avatars, and seed-user counts do not affect this model's guarantees and remain abstract. No model of mandatory first-login password rotation is included because the current backend does not enforce it.
