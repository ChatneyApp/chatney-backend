package channel

import (
	"chatney-backend/src/domains/channel/models"
	"context"
)

func (r *ChannelRootAggregate) CreateChannel(input *models.Channel) (*models.Channel, error) {
	return r.channelRepo.Create(context.TODO(), input)
}

func (r *ChannelRootAggregate) GetChannel(channelId string) (*models.Channel, error) {
	return r.channelRepo.GetByID(context.TODO(), channelId)
}

func (r *ChannelRootAggregate) UpdateChannel(input *models.Channel) (*models.Channel, error) {
	return r.channelRepo.Update(context.TODO(), input.Id, input)
}

func (r *ChannelRootAggregate) GetWorkspaceChannelsList(workspaceId string) ([]*models.Channel, error) {
	return r.channelRepo.GetWorkspaceChannelsList(workspaceId)
}

func (r *ChannelRootAggregate) DeleteChannel(id string) (bool, error) {
	return r.channelRepo.Delete(context.TODO(), id)
}
