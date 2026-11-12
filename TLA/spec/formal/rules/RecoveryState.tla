---------------- MODULE RecoveryState ----------------
EXTENDS FiniteSets
CONSTANTS Users, ResetTokens, NoUser
VARIABLES userStatus, resetTokens, resetOwner, usedResetTokens
vars == <<resetTokens, resetOwner, usedResetTokens>>
Init ==
    /\ resetTokens = {}
    /\ resetOwner = [t \in ResetTokens |-> NoUser]
    /\ usedResetTokens = {}

RequestPasswordReset(user, token) ==
    /\ userStatus[user] = "active"
    /\ token \in ResetTokens \ (resetTokens \cup usedResetTokens)
    /\ resetTokens' = (resetTokens \ {old \in resetTokens : resetOwner[old] = user})
        \cup {token}
    /\ resetOwner' = [resetOwner EXCEPT ![token] = user]
    /\ usedResetTokens' = usedResetTokens \cup {old \in resetTokens : resetOwner[old] = user}

CompletePasswordReset(user, token) ==
    /\ token \in resetTokens /\ resetOwner[token] = user
    /\ resetTokens' = {other \in resetTokens : resetOwner[other] # user}
    /\ usedResetTokens' = usedResetTokens \cup
        {other \in resetTokens : resetOwner[other] = user}
    /\ UNCHANGED <<resetOwner>>

Next == \E user \in Users, token \in ResetTokens :
    RequestPasswordReset(user, token) \/ CompletePasswordReset(user, token)
TypeOK ==
    /\ resetTokens \subseteq ResetTokens
    /\ usedResetTokens \subseteq ResetTokens
    /\ resetOwner \in [ResetTokens -> (Users \cup {NoUser})]
ResetTokensHaveOwners == \A token \in resetTokens : resetOwner[token] \in Users
UsedResetTokensAreDead == resetTokens \cap usedResetTokens = {}
Safety == TypeOK /\ ResetTokensHaveOwners /\ UsedResetTokensAreDead

============================================================
