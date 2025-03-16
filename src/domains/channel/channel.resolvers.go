package channel

import (
	graphql_models "chatney-backend/graph/model"
	"chatney-backend/src/domains/channel/models"
	"context"

	"github.com/google/uuid"
)

func (r *ChannelMutationsResolvers) CreateChannel(ctx context.Context, input graphql_models.MutateChannelDto) (*graphql_models.Channel, error) {
	res, err := r.rootAggregate.CreateChannel(&models.Channel{
		Id:            uuid.NewString(),
		Name:          input.Name,
		WorkspaceId:   input.WorkspaceID,
		ChannelTypeId: input.ChannelTypeID,
	})

	if err != nil {
		return nil, err
	}

	return channelToDTO(*res), nil
}

func (r *ChannelMutationsResolvers) UpdateChannel(ctx context.Context, input graphql_models.MutateChannelDto, channelId string) (*graphql_models.Channel, error) {
	res, err := r.rootAggregate.UpdateChannel(&models.Channel{
		Id:            channelId,
		Name:          input.Name,
		WorkspaceId:   input.WorkspaceID,
		ChannelTypeId: input.ChannelTypeID,
	})

	if err != nil {
		return nil, err
	}

	return channelToDTO(*res), nil
}

func (r *ChannelMutationsResolvers) DeleteChannel(ctx context.Context, channelId string) (bool, error) {
	return r.rootAggregate.DeleteChannel(channelId)
}

func (r *ChannelQueryResolvers) GetWorkspaceChannelsList(ctx context.Context, workspaceId string) ([]*graphql_models.Channel, error) {
	list, err := r.rootAggregate.GetWorkspaceChannelsList(workspaceId)
	if err != nil {
		return nil, err
	}

	output := make([]*graphql_models.Channel, len(list))
	for _, ch := range list {
		output = append(output, channelToDTO(*ch))
	}

	return output, nil
}
