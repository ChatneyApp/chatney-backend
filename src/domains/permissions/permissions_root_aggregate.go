package permissions

import (
	"chatney-backend/graph/model"
	"chatney-backend/src/domains/channel"
)

type PermissionsGroupsStruct struct {
	channel *channel.ChannelPermissionsGroup
}

func (pg *PermissionsGroupsStruct) GetPermissionsList() (*model.PermissionsGroup, error) {
	var output = model.PermissionsGroup{
		Label: "Channel permissions",
		List: []string{
			"channel.deleteMessage",
			"channel.editMessage",
			"channel.createMessage",
			"channel.readMessage",
		},
	}
	return &output, nil
}

var PermissionsGroups = PermissionsGroupsStruct{channel: &channel.ChannelPermissions}
