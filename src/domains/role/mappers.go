package role

import (
	graphql_models "chatney-backend/graph/model"
	"chatney-backend/src/domains/role/models"
)

func RoleToDTO(role *models.Role) *graphql_models.Role {
	permStringList := make([]string, len(role.Permissions))
	for i, s := range role.Permissions {
		permStringList[i] = string(s)
	}

	return &graphql_models.Role{
		ID:          role.Id,
		Name:        role.Name,
		Settings:    (*graphql_models.RoleSettings)(role.Settings),
		Permissions: permStringList,
	}
}
