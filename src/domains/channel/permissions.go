package channel

import (
	"chatney-backend/src/domains/role/models"
)

type ChannelPermissionsGroup struct {
	DeleteMessage models.PermissionKey
	EditMessage   models.PermissionKey
	CreateMessage models.PermissionKey
	ReadMessage   models.PermissionKey
}

var ChannelPermissions = ChannelPermissionsGroup{
	DeleteMessage: "channel.deleteMessage",
	EditMessage:   "channel.editMessage",
	CreateMessage: "channel.createMessage",
	ReadMessage:   "channel.readMessage",
}
