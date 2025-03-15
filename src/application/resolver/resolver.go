package resolver

import (
	"chatney-backend/graph"
	"chatney-backend/src/domains/permissions"
	"chatney-backend/src/domains/role"
	"chatney-backend/src/domains/role/models"

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
	*permissions.PermissionsResolver
}
type MutationResolver struct {
	*Resolver
	*role.RoleMutationsResolvers
}

func (r *Resolver) Query() graph.QueryResolver {
	return &QueryResolver{r, &permissions.PermissionsResolver{}}
}

// Mutation returns graph.MutationResolver implementation.
func (r *Resolver) Mutation() graph.MutationResolver {
	return &MutationResolver{
		r,
		&role.RoleMutationsResolvers{
			RootAggregate: &role.RoleRootAggregateStruct{
				RoleRepo: &models.RoleRepo{Collection: r.DB.Collection("roles")}},
		},
	}
}
