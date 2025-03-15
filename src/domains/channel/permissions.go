package channel

import (
	"chatney-backend/src/domains/role/models"
)

const (
	deleteMessage models.PermissionKey = "channel.deleteMessage"
	editMessage   models.PermissionKey = "channel.editMessage"
	createMessage models.PermissionKey = "channel.createMessage"
	readMessage   models.PermissionKey = "channel.readMessage"

	deleteChannel models.PermissionKey = "channel.deleteChannel"
	editChannel   models.PermissionKey = "channel.editChannel"
	createChannel models.PermissionKey = "channel.createChannel"
	readChannel   models.PermissionKey = "channel.readChannel"
)

var ChannelPermissions = []models.PermissionKey{
	deleteMessage, editMessage, createMessage, readMessage,
	deleteChannel, editChannel, createChannel, readChannel,
}
