---------------- MODULE RecoveryWorkflow ----------------
EXTENDS UsersState, TLC
CONSTANTS ResetTokens, NoUser
VARIABLES resetTokens, resetOwner, usedResetTokens
Recovery == INSTANCE RecoveryState WITH
    userStatus <- userStatus, resetTokens <- resetTokens,
    resetOwner <- resetOwner, usedResetTokens <- usedResetTokens
recoveryVars == <<resetTokens, resetOwner, usedResetTokens>>
InitRecovery == Init /\ Recovery!Init
NextRecovery ==
    (CommonAccountFlow /\ UNCHANGED recoveryVars)
    \/ ((\E actor \in Users, u \in Users, status \in {"active", "disabled", "banned"} :
        ChangeAccountStatus(actor, u, status)) /\ UNCHANGED recoveryVars)
    \/ (Recovery!Next /\ UNCHANGED vars)
SafetyRecovery == Safety /\ Recovery!Safety
allVars == <<vars, recoveryVars>>
Spec == InitRecovery /\ [][NextRecovery]_allVars
Symmetry == Permutations(ResetTokens)
============================================================
