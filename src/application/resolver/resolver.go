package resolver

import (
	"chatney-backend/graph"
	"chatney-backend/src/domains/permissions"
	"chatney-backend/src/domains/role"

	"go.mongodb.org/mongo-driver/v2/mongo"
)

// This file will not be regenerated automatically.
//
// It serves as dependency injection for your app, add any dependencies you require here.

type Resolver struct {
	DB *mongo.Database
}

type QueryResolver struct {
	*Resolver
	permissions.PermissionsResolver
	role.RoleQueryResolvers
}
type MutationResolver struct {
	*Resolver
	role.RoleMutationsResolvers
}

func (r *Resolver) Query() graph.QueryResolver {
	return &QueryResolver{r,
		permissions.PermissionsResolver{},
		role.GetQueryResolvers(r.DB),
	}
}

// Mutation returns graph.MutationResolver implementation.
func (r *Resolver) Mutation() graph.MutationResolver {
	return &MutationResolver{
		r,
		role.GetMutationResolvers(r.DB),
	}
}
