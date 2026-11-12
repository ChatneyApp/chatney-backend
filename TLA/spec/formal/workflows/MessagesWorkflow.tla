---------------- MODULE MessagesWorkflow ----------------
EXTENDS MessagesState, TLC

Spec == Init /\ [][Next]_vars
Symmetry == Permutations(RecordIds)

============================================================
