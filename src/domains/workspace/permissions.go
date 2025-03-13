package workspace

import (
	"chatney-backend/graph/model"
	"chatney-backend/src/domains/channel"
)

type PermissionsGroupsStruct struct {
	channel *channel.ChannelPermissionsGroup
}

func (pg *PermissionsGroupsStruct) GetPermissionsList() (*model.PermissionsGroup, error) {
	var output = model.PermissionsGroup{
		Label: "Workspace permissions",
		List: []string{
			"workspace.createWorkspace",
			"workspace.editWorkspace",
			"workspace.deleteWorkspace",
			"workspace.readWorkspace",
		},
	}
	return &output, nil
}

var PermissionsGroups = PermissionsGroupsStruct{channel: &channel.ChannelPermissions}
