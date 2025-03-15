package permissions

import (
	graphql_models "chatney-backend/graph/model"
	"context"
)

type PermissionsResolver struct{}

// GetPermissionsList is the resolver for the getPermissionsList field.
func (r *PermissionsResolver) GetPermissionsList(ctx context.Context) (*graphql_models.PermissionsListReturn, error) {
	permissionsGroups, err := PermissionsGroups.GetPermissionsList()

	if err != nil {
		return nil, err
	}

	return &graphql_models.PermissionsListReturn{
		Groups: permissionsGroups,
	}, nil
}
