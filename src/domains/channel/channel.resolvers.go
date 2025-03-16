package channel

import (
	graphql_models "chatney-backend/graph/model"
	"chatney-backend/src/domains/channel/models"

	"github.com/google/uuid"
)

func (r *ChannelMutationsResolvers) CreateChannel(input *graphql_models.MutateChannelDto) (*graphql_models.Channel, error) {
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

func (r *ChannelMutationsResolvers) UpdateChannel(input *graphql_models.MutateChannelDto, channelTypeId string) (*graphql_models.Channel, error) {
	res, err := r.rootAggregate.UpdateChannel(&models.Channel{
		Id:            channelTypeId,
		Name:          input.Name,
		WorkspaceId:   input.WorkspaceID,
		ChannelTypeId: input.ChannelTypeID,
	})

	if err != nil {
		return nil, err
	}

	return channelToDTO(*res), nil
}

func (r *ChannelMutationsResolvers) DeleteChannel(id string) (bool, error) {
	return r.rootAggregate.DeleteChannel(id)
}

func (r *ChannelQueryResolvers) GetWorkspaceChannelsList(workspaceId string) ([]*models.Channel, error) {
	return r.rootAggregate.GetWorkspaceChannelsList(workspaceId)
}
