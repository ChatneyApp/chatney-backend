package permissions

import (
	graphql_models "chatney-backend/graph/model"
	"chatney-backend/src/application/utils"
	"chatney-backend/src/domains/channel"
	"chatney-backend/src/domains/role"
	"chatney-backend/src/domains/user"
	"chatney-backend/src/domains/workspace"
)

type PermissionsGroupsStruct struct {
}

func (pg *PermissionsGroupsStruct) GetPermissionsList() ([]*graphql_models.PermissionsGroup, error) {
	return []*graphql_models.PermissionsGroup{
		utils.GetPermissionsGroup("Workpsace permissions", workspace.WorkspacePermissions),
		utils.GetPermissionsGroup("Channel permissions", channel.ChannelPermissions),
		utils.GetPermissionsGroup("User permissions", user.UserPermissions),
		utils.GetPermissionsGroup("Role permissions", role.RolePermissions),
	}, nil
}

var PermissionsGroups = PermissionsGroupsStruct{}
