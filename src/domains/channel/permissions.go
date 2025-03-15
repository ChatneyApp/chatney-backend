package channel

import (
	"chatney-backend/src/domains/role/models"
)

const (
	DeleteMessage models.PermissionKey = "channel.deleteMessage"
	EditMessage   models.PermissionKey = "channel.editMessage"
	CreateMessage models.PermissionKey = "channel.createMessage"
	ReadMessage   models.PermissionKey = "channel.readMessage"

	DeleteChannel models.PermissionKey = "channel.deleteChannel"
	EditChannel   models.PermissionKey = "channel.editChannel"
	CreateChannel models.PermissionKey = "channel.createChannel"
	ReadChannel   models.PermissionKey = "channel.readChannel"
)

var ChannelPermissions = []models.PermissionKey{
	DeleteMessage, EditMessage, CreateMessage, ReadMessage,
	DeleteChannel, EditChannel, CreateChannel, ReadChannel,
}
