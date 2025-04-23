package channel

import (
	graphql_models "chatney-backend/graph/model"
	"chatney-backend/src/domains/channel/models"
	"context"

	"github.com/google/uuid"
)

func (r *ChannelMutationsResolvers) CreateChannelType(ctx context.Context, input graphql_models.MutateChannelTypeDto) (*graphql_models.ChannelType, error) {
	res, err := r.rootAggregate.CreateChannelType(&models.ChannelType{
		Id:         uuid.NewString(),
		Label:      input.Label,
		Key:        input.Key,
		BaseRoleId: input.BaseRoleID,
	})

	if err != nil {
		return nil, err
	}

	return channelTypeToDTO(*res), nil
}

func (r *ChannelMutationsResolvers) UpdateChannelType(ctx context.Context, input graphql_models.MutateChannelTypeDto, id string) (*graphql_models.ChannelType, error) {
	res, err := r.rootAggregate.UpdateChannelType(&models.ChannelType{
		Id:         id,
		Label:      input.Label,
		Key:        input.Key,
		BaseRoleId: input.BaseRoleID,
	})

	if err != nil {
		return nil, err
	}

	return channelTypeToDTO(*res), nil
}

func (r *ChannelMutationsResolvers) DeleteChannelType(ctx context.Context, channelId string) (bool, error) {
	return r.rootAggregate.DeleteChannelType(channelId)
}

func (r *ChannelQueryResolvers) GetAllChannelTypesList(ctx context.Context) ([]*graphql_models.ChannelType, error) {
	list, err := r.rootAggregate.GetChannelTypesList()
	if err != nil {
		return nil, err
	}

	var output []*graphql_models.ChannelType
	for _, chType := range list {
		output = append(output, channelTypeToDTO(*chType))
	}

	return output, nil
}
