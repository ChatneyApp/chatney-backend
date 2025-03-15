package channel

import (
	graphql_models "chatney-backend/graph/model"
	"chatney-backend/src/application/repository"
	"chatney-backend/src/domains/channel/models"
	"context"

	"github.com/google/uuid"
	"go.mongodb.org/mongo-driver/v2/mongo"
)

type ChannelMutationsResolvers struct {
	rootAggregate *ChannelRootAggregate
}

type ChannelQueryResolvers struct {
	rootAggregate *ChannelRootAggregate
}

func (r *ChannelQueryResolvers) GetChannelGroup(ctx context.Context, id uuid.UUID) (*graphql_models.ChannelGroup, error) {
	res, err := r.rootAggregate.getChannelGroupById(id)
	if err != nil {
		return nil, err
	}

	return channelGroupToDTO(*res), nil
}

func (r *ChannelQueryResolvers) ListChannelGroups(ctx context.Context, workspaceId uuid.UUID) ([]*graphql_models.ChannelGroup, error) {
	groups, err := r.rootAggregate.getChannelGroupListWithinWorkspace(workspaceId)
	if err != nil {
		return nil, err
	}

	out := make([]*graphql_models.ChannelGroup, len(groups))
	for _, group := range groups {
		out = append(out, channelGroupToDTO(group))
	}

	return out, nil
}

func (r *ChannelMutationsResolvers) CreateChannelGroup(ctx context.Context, input graphql_models.CreateChannelGroupInput) (*graphql_models.ChannelGroup, error) {
	res, err := r.rootAggregate.createChannelGroup(models.ChannelGroup{
		Name:      input.Name,
		Workspace: input.Workspace,
	})

	if err != nil {
		return nil, err
	}

	return channelGroupToDTO(*res), nil
}

func (r *ChannelMutationsResolvers) UpdateChannelGroup(ctx context.Context, input graphql_models.UpdateChannelGroupInput) (*graphql_models.ChannelGroup, error) {
	res, err := r.rootAggregate.updateChannelGroup(models.ChannelGroup{
		Id:       input.ID,
		Channels: input.Channels,
		Name:     *input.Name,
		Order:    int(*input.Order),
	})

	if err != nil {
		return nil, err
	}

	return channelGroupToDTO(*res), nil
}

func (r *ChannelMutationsResolvers) DeleteChannelGroup(ctx context.Context, UUID uuid.UUID) (bool, error) {
	return r.rootAggregate.deleteChannelGroup(UUID)
}

func getAggregateRoot(DB *mongo.Database) *ChannelRootAggregate {
	return &ChannelRootAggregate{
		channelRepo: &models.ChannelRepo{
			BaseRepo: &repository.BaseRepo[models.Channel]{
				Collection: DB.Collection("channels"),
			}},
		channelGroupRepo: &models.ChannelGroupRepo{
			BaseRepo: &repository.BaseRepo[models.ChannelGroup]{
				Collection: DB.Collection("channel_groups"),
			}},
		channelTypeRepo: &models.ChannelTypeRepo{
			BaseRepo: &repository.BaseRepo[models.ChannelType]{
				Collection: DB.Collection("channel_types"),
			}},
	}
}

func GetMutationResolvers(DB *mongo.Database) ChannelMutationsResolvers {
	return ChannelMutationsResolvers{
		rootAggregate: getAggregateRoot(DB),
	}
}

func GetQueryResolvers(DB *mongo.Database) ChannelQueryResolvers {
	return ChannelQueryResolvers{
		rootAggregate: getAggregateRoot(DB),
	}
}
