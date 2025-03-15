package permissions

import (
	graphql_models "chatney-backend/graph/model"
	"chatney-backend/src/domains/channel"
)

type PermissionsGroupsStruct struct {
	channel *channel.ChannelPermissionsGroup
}

func (pg *PermissionsGroupsStruct) GetPermissionsList() ([]*graphql_models.PermissionsGroup, error) {
	var channel = graphql_models.PermissionsGroup{
		Label: "Channel permissions",
		List: []string{
			"channel.deleteMessage",
			"channel.editMessage",
			"channel.createMessage",
			"channel.readMessage",
		},
	}
	var workspace = graphql_models.PermissionsGroup{
		Label: "Workspace permissions",
		List: []string{
			"workspace.deleteWorkspace",
			"workspace.editWorkspace",
			"workspace.createWorkspace",
			"workspace.readWorkspace",
		},
	}
	return []*graphql_models.PermissionsGroup{
		&workspace,
		&channel,
	}, nil
}

var PermissionsGroups = PermissionsGroupsStruct{channel: &channel.ChannelPermissions}
