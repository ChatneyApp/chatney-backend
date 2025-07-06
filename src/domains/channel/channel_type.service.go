package channel

import (
	"chatney-backend/src/domains/channel/models"
	"context"

	"go.mongodb.org/mongo-driver/v2/bson"
)

func (r *ChannelRootAggregate) CreateChannelType(channelType *models.ChannelType) (*models.ChannelType, error) {
	return r.channelTypeRepo.Create(context.TODO(), channelType)
}

func (r *ChannelRootAggregate) UpdateChannelType(channelType *models.ChannelType) (*models.ChannelType, error) {
	return r.channelTypeRepo.Update(context.TODO(), channelType.Id, channelType)
}

func (r *ChannelRootAggregate) DeleteChannelType(channelTypeId string) (bool, error) {
	return r.channelTypeRepo.Delete(context.TODO(), channelTypeId)
}

func (r *ChannelRootAggregate) GetChannelTypesList() ([]*models.ChannelType, error) {
	return r.channelTypeRepo.GetAll(context.TODO(), &bson.M{})
}
