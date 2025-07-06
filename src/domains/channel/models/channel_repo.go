package models

import (
	"chatney-backend/src/application/repository"
	"context"

	"go.mongodb.org/mongo-driver/v2/bson"
)

type ChannelRepo struct {
	*repository.BaseRepo[Channel]
}

func (repo *ChannelRepo) GetWorkspaceChannelsList(workspaceId string) ([]*Channel, error) {
	return repo.GetAll(context.TODO(), &bson.M{"workspaceId": workspaceId})
}

// ChangeChannelType - updates the channel's type
func (repo *ChannelRepo) ChangeChannelType(channelID, newChannelTypeID string) error {
	filter := bson.M{"_id": channelID}
	update := bson.M{"$set": bson.M{"channel_type": newChannelTypeID}}

	_, err := repo.Collection.UpdateOne(context.TODO(), filter, update)
	return err
}
