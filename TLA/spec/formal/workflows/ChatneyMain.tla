-------------------- MODULE ChatneyMain --------------------
EXTENDS ChatneyArchitecture, TLC

\* Records and reset tokens are interchangeable model values in this abstraction.
Symmetry == Permutations(RecordIds) \union Permutations(ResetTokens)

============================================================
