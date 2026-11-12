---------------- MODULE SetupWorkflow ----------------
EXTENDS SetupState, TLC

Spec == Init /\ [][Next]_vars
Symmetry == {}


============================================================
