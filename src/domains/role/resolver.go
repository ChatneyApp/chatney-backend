package role

import (
	graphql_models "chatney-backend/graph/model"
	"chatney-backend/src/domains/role/models"
	"context"
)

type RoleMutationsResolvers struct {
	RootAggregate *RoleRootAggregateStruct
}

type RoleQueryResolvers struct {
	RootAggregate *RoleRootAggregateStruct
}

// CreateRole is the resolver for the createRole field.
func (r *RoleMutationsResolvers) CreateRole(ctx context.Context, roleData graphql_models.CreateRoleDto) (*graphql_models.Role, error) {
	permissions := make([]models.PermissionKey, len(roleData.Permissions))
	for i, p := range roleData.Permissions {
		permissions[i] = models.PermissionKey(p) // converting string into PermissionKey type
	}

	role, err := r.RootAggregate.CreateNewRole(models.Role{
		Name: roleData.Name,
		Settings: models.RoleSettings{
			Base: roleData.Settings.Base,
		},
		Permissions: permissions,
	})

	permissionsOut := make([]string, len(role.Permissions))
	for i, p := range role.Permissions {
		permissionsOut[i] = string(p) // converting string into PermissionKey type
	}

	return &graphql_models.Role{
		ID:          role.Id.String(),
		Name:        role.Name,
		Permissions: permissionsOut,
		Settings:    (*graphql_models.RoleSettings)(&role.Settings),
	}, err
}

// EditRole is the resolver for the editRole field.
func (r *RoleMutationsResolvers) EditRole(ctx context.Context, roleData graphql_models.EditRoleDto) (*graphql_models.Role, error) {
	permissions := make([]models.PermissionKey, len(roleData.Permissions))
	for i, p := range roleData.Permissions {
		permissions[i] = models.PermissionKey(p) // converting string into PermissionKey type
	}

	role, err := r.RootAggregate.CreateNewRole(models.Role{
		Name: roleData.Name,
		Settings: models.RoleSettings{
			Base: roleData.Settings.Base,
		},
		Permissions: permissions,
	})

	permissionsOut := make([]string, len(role.Permissions))
	for i, p := range role.Permissions {
		permissionsOut[i] = string(p) // converting string into PermissionKey type
	}

	return &graphql_models.Role{
		ID:          role.Id.String(),
		Name:        role.Name,
		Permissions: permissionsOut,
		Settings:    (*graphql_models.RoleSettings)(&role.Settings),
	}, err
}
