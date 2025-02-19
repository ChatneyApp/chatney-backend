package models

import "go.mongodb.org/mongo-driver/v2/mongo"

type ChannelTypeRepo struct {
	db *mongo.Collection
}

func (repo *ChannelTypeRepo) SaveChannelType(channelType ChannelType) error {
	return nil
}

func (repo *ChannelTypeRepo) GetChannelTypeById(id int) error {
	return nil
}
