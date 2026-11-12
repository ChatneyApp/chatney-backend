---------------- MODULE UsersWorkflow ----------------
EXTENDS UsersState, TLC

Spec == Init /\ [][Next]_vars
Symmetry == {}


============================================================
