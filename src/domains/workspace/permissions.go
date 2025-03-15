package workspace

import (
	graphql_models "chatney-backend/graph/model"
	"chatney-backend/src/domains/channel"
)

type PermissionsGroupsStruct struct {
	channel *channel.ChannelPermissionsGroup
}

func (pg *PermissionsGroupsStruct) GetPermissionsList() (*graphql_models.PermissionsGroup, error) {
	var output = graphql_models.PermissionsGroup{
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
