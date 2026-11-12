---------------- MODULE SetupState ----------------
EXTENDS Naturals
VARIABLES setupPhase, adminReady
vars == <<setupPhase, adminReady>>
SetupPhases == {"uninstalled", "migrating", "seeding", "roleCreated",
    "failed", "failedPartial", "incomplete", "installed"}

Init == setupPhase = "uninstalled" /\ adminReady = FALSE

StartSetup ==
    /\ setupPhase = "uninstalled"
    /\ setupPhase' = "migrating"
    /\ UNCHANGED <<adminReady>>

MigrationsComplete ==
    /\ setupPhase = "migrating"
    /\ setupPhase' = "seeding"
    /\ UNCHANGED <<adminReady>>

\* The role is persisted before users/configuration; it is the retry sentinel.
CreateAdminRole ==
    /\ setupPhase = "seeding"
    /\ setupPhase' = "roleCreated"
    /\ UNCHANGED <<adminReady>>

CommitSetup ==
    /\ setupPhase = "roleCreated"
    /\ setupPhase' = "installed"
    /\ adminReady' = TRUE

FailSetup ==
    /\ setupPhase \in {"migrating", "seeding", "roleCreated"}
    /\ setupPhase' = IF setupPhase = "roleCreated" THEN "failedPartial" ELSE "failed"
    /\ UNCHANGED <<adminReady>>

RetrySetup ==
    /\ setupPhase \in {"failed", "failedPartial"}
    \* Retry sees the persisted role and returns "installed" without finishing seed.
    /\ setupPhase' = IF setupPhase = "failedPartial" THEN "incomplete" ELSE "migrating"
    /\ UNCHANGED <<adminReady>>

UninstallSystem ==
    \* The current mutation does not authorize the caller.
    /\ setupPhase # "uninstalled"
    /\ setupPhase' = "uninstalled"
    /\ adminReady' = FALSE

Next ==
    StartSetup \/ MigrationsComplete \/ CreateAdminRole \/ CommitSetup
    \/ FailSetup \/ RetrySetup \/ UninstallSystem

TypeOK == setupPhase \in SetupPhases /\ adminReady \in BOOLEAN
SetupStateConsistent == setupPhase = "installed" => adminReady
Safety == TypeOK /\ SetupStateConsistent

============================================================
