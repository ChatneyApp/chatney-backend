package models

import "go.mongodb.org/mongo-driver/v2/mongo"

type ChannelGroupRepo struct {
	db *mongo.Collection
}

func (repo *ChannelGroupRepo) SaveChannelGroup(channelGroup ChannelGroup) error {
	return nil
}

func (repo *ChannelGroupRepo) GetChannelGroupById(id int) error {
	return nil
}
