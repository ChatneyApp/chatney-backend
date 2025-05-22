package permissions

import (
	graphql_models "chatney-backend/graph/model"
	"chatney-backend/src/domains/channel"
	"chatney-backend/src/domains/config"
	"chatney-backend/src/domains/role"
	"chatney-backend/src/domains/role/models"
	"chatney-backend/src/domains/user"
	"chatney-backend/src/domains/workspace"
)

type PermissionsGroupsStruct struct {
}

func GetPermissionsGroup(groupLabel string, permissionsList []models.PermissionKey) *graphql_models.PermissionsGroup {
	permissionsStrings := make([]string, len(permissionsList))
	for i, p := range permissionsList {
		permissionsStrings[i] = string(p) // converting PermissionKey type into string
	}

	return &graphql_models.PermissionsGroup{
		Label: groupLabel,
		List:  permissionsStrings,
	}
}

func (pg *PermissionsGroupsStruct) GetPermissionsList() ([]*graphql_models.PermissionsGroup, error) {
	return []*graphql_models.PermissionsGroup{
		GetPermissionsGroup("System config permissions", config.SystemConfigPermissions),
		GetPermissionsGroup("Workspace permissions", workspace.WorkspacePermissions),
		GetPermissionsGroup("Channel permissions", channel.ChannelPermissions),
		GetPermissionsGroup("User permissions", user.UserPermissions),
		GetPermissionsGroup("Role permissions", role.RolePermissions),
	}, nil
}

var PermissionsGroups = PermissionsGroupsStruct{}
