package models

import "go.mongodb.org/mongo-driver/v2/mongo"

type ChannelRepo struct {
	db *mongo.Collection
}

func (repo *ChannelRepo) SaveChannel(channel Channel) error {
	return nil
}

func (repo *ChannelRepo) GetChannelById(id int) error {
	return nil
}
