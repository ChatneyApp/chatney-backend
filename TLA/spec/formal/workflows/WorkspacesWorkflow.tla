---------------- MODULE WorkspacesWorkflow ----------------
EXTENDS WorkspacesState, TLC

Spec == Init /\ [][Next]_vars
Symmetry == Permutations(RecordIds)


============================================================
