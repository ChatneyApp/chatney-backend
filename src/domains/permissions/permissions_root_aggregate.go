package permissions

import (
	"chatney-backend/graph/model"
	"chatney-backend/src/domains/channel"
)

type PermissionsGroupsStruct struct {
	channel *channel.ChannelPermissionsGroup
}

func (pg *PermissionsGroupsStruct) GetPermissionsList() ([]*model.PermissionsGroup, error) {
	var channel = model.PermissionsGroup{
		Label: "Channel permissions",
		List: []string{
			"channel.deleteMessage",
			"channel.editMessage",
			"channel.createMessage",
			"channel.readMessage",
		},
	}
	var workspace = model.PermissionsGroup{
		Label: "Workspace permissions",
		List: []string{
			"workspace.deleteWorkspace",
			"workspace.editWorkspace",
			"workspace.createWorkspace",
			"workspace.readWorkspace",
		},
	}
	return []*model.PermissionsGroup{
		&workspace,
		&channel,
	}, nil
}

var PermissionsGroups = PermissionsGroupsStruct{channel: &channel.ChannelPermissions}
