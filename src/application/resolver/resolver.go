package resolver

import (
	"chatney-backend/graph"
	"chatney-backend/src/application"
	"chatney-backend/src/domains/channel"
	"chatney-backend/src/domains/config"
	"chatney-backend/src/domains/message"
	"chatney-backend/src/domains/permissions"
	"chatney-backend/src/domains/role"
	"chatney-backend/src/domains/user"
	"chatney-backend/src/domains/workspace"

	"go.mongodb.org/mongo-driver/v2/mongo"
)

// This file will not be regenerated automatically.
//
// It serves as dependency injection for your app, add any dependencies you require here.

type Resolver struct {
	DB     *mongo.Database
	Bucket *application.Bucket
}

type QueryResolver struct {
	*Resolver
	permissions.PermissionsResolver
	config.ConfigQueryResolvers
	role.RoleQueryResolvers
	user.UserQueryResolvers
	channel.ChannelQueryResolvers
	workspace.WorkspaceQueryResolvers
	message.MessageQueryResolvers
}
type MutationResolver struct {
	*Resolver
	config.ConfigMutationsResolvers
	role.RoleMutationsResolvers
	channel.ChannelMutationsResolvers
	workspace.WorkspaceMutationsResolvers
	user.UserMutationsResolvers
	message.MessageMutationsResolvers
}

func (r *Resolver) Query() graph.QueryResolver {
	return &QueryResolver{r,
		permissions.PermissionsResolver{},
		config.GetQueryResolvers(r.DB),
		role.GetQueryResolvers(r.DB),
		user.GetQueryResolvers(r.DB),
		channel.GetQueryResolvers(r.DB),
		workspace.GetQueryResolvers(r.DB),
		message.GetMessageQueryResolvers(r.DB, r.Bucket),
	}
}

// Mutation returns graph.MutationResolver implementation.
func (r *Resolver) Mutation() graph.MutationResolver {
	return &MutationResolver{
		r,
		config.GetMutationResolvers(r.DB),
		role.GetMutationResolvers(r.DB),
		channel.GetMutationResolvers(r.DB),
		workspace.GetMutationResolvers(r.DB),
		user.GetMutationResolvers(r.DB),
		message.GetMessageMutationResolvers(r.DB),
	}
}
