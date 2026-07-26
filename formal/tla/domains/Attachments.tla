----------------------------- MODULE Attachments -----------------------------
EXTENDS State

ImageMimes == {"image/gif", "image/png", "image/jpeg", "image/custom"}
VideoMimes == {"video/mp4", "video/webm", "video/custom"}
AudioMimes == {"audio/mpeg", "audio/ogg", "audio/custom"}
MimeTypes == ImageMimes \union VideoMimes \union AudioMimes
             \union {"application/octet-stream", "application/custom"}

AttachmentTypeForMime(mime) ==
  CASE mime = "image/gif" -> "gif"
    [] mime \in ImageMimes -> "image"
    [] mime \in VideoMimes -> "video"
    [] mime \in AudioMimes -> "audio"
    [] OTHER -> "binary"

AttachmentExtension(kind, original) ==
  CASE kind = "audio" -> "mp3"
    [] kind = "video" -> "mp4"
    [] kind = "gif" -> "gif"
    [] OTHER -> original

ApplyUploadAttachment(s, actor, attachment) ==
  IF ~IsAuthenticated(s, actor) \/ ~CanGlobal(s, actor, AttachmentUpload)
  THEN FailedState(s, Forbidden)
  ELSE IF attachment.size = 0 THEN FailedState(s, FileEmpty)
  ELSE IF ExistsId(s.attachments, attachment.id) THEN FailedState(s, DbConstraint)
  ELSE LET kind == AttachmentTypeForMime(attachment.mimeType)
           owned == [attachment EXCEPT !.userId = actor,
             !.attachmentType = kind,
             !.extension = AttachmentExtension(kind, attachment.extension)]
       IN RecordGlobal(s, [s EXCEPT !.attachments = @ \union {owned}],
            "attachment", "upload", attachment.id, actor,
            AttachmentUpload, FALSE, 0)

=============================================================================
