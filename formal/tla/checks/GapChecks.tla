----------------------------- MODULE GapChecks -------------------------------
EXTENDS Install, Messages, Attachments, Organization, Invariants

VARIABLES gapState, phase
gapVars == <<gapState, phase>>

Parent == Message(5, 1, 2, "hello", {}, {}, "input", 0, 99, 0)
InvalidChild == Message(6, 1, 2, "reply", {}, {}, "input", 5, 99, 8)

ThreadGapInit == gapState = EmptyState /\ phase = 0
ThreadGapNext ==
  \/ phase = 0 /\ gapState' = ApplyInstall(gapState) /\ phase' = 1
  \/ phase = 1 /\
     gapState' = ApplyAddMessage(gapState, 2, Parent, {}) /\ phase' = 2
  \/ phase = 2 /\
     gapState' = ApplyAddMessage(gapState, 2, InvalidChild, {}) /\ phase' = 3

ThreadGapSpec == ThreadGapInit /\ [][ThreadGapNext]_gapVars
ThreadCountInvariant == ThreadCountsExact(gapState)

ForeignAttachment == Attachment(5, 2, "application/octet-stream", 10)
ForeignAttachmentMessage ==
  Message(6, 1, 1, "hello", {5}, {}, "input", 0, 99, 0)

OwnershipGapInit == gapState = EmptyState /\ phase = 0
OwnershipGapNext ==
  \/ phase = 0 /\ gapState' = ApplyInstall(gapState) /\ phase' = 1
  \/ phase = 1 /\
     gapState' = ApplyUploadAttachment(gapState, 2, ForeignAttachment) /\
     phase' = 2
  \/ phase = 2 /\
     gapState' = ApplyAddMessage(gapState, 1, ForeignAttachmentMessage, {}) /\
     phase' = 3

OwnershipGapSpec == OwnershipGapInit /\ [][OwnershipGapNext]_gapVars
AttachmentOwnershipInvariant == AttachmentOwnership(gapState)

MissingReferenceGapInit == gapState = EmptyState /\ phase = 0
MissingReferenceGapNext ==
  \/ phase = 0 /\ gapState' = ApplyInstall(gapState) /\ phase' = 1
  \/ phase = 1 /\
     gapState' = ApplyAddChannel(gapState, 1, Channel(5, "orphan", 8, 1)) /\
     phase' = 2

MissingReferenceGapSpec ==
  MissingReferenceGapInit /\ [][MissingReferenceGapNext]_gapVars

StrongReferencesInvariant == StrongDomainReferences(gapState)

=============================================================================
