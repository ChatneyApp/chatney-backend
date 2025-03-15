package models

import (
	repostiory "chatney-backend/src/application/repository"
	"context"

	"go.mongodb.org/mongo-driver/v2/bson"
)

type ChannelRepo struct {
	*repostiory.BaseRepo[Channel]
}

// ChangeChannelType - updates the channel's type
func (repo *ChannelRepo) ChangeChannelType(channelID, newChannelTypeID bson.ObjectID) error {
	filter := bson.M{"_id": channelID}
	update := bson.M{"$set": bson.M{"channel_type": newChannelTypeID}}

	_, err := repo.Collection.UpdateOne(context.TODO(), filter, update)
	return err
}
