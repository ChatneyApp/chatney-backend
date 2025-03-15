package utils

import (
	graphql_models "chatney-backend/graph/model"
	"chatney-backend/src/domains/role/models"
)

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
