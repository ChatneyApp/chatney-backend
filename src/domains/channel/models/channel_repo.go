package models

import (
	"chatney-backend/src/application/repository"
	"context"

	"github.com/google/uuid"
	"go.mongodb.org/mongo-driver/v2/bson"
)

type ChannelRepo struct {
	*repository.BaseRepo[Channel]
}

// ChangeChannelType - updates the channel's type
func (repo *ChannelRepo) ChangeChannelType(channelID, newChannelTypeID uuid.UUID) error {
	filter := bson.M{"_id": channelID}
	update := bson.M{"$set": bson.M{"channel_type": newChannelTypeID}}

	_, err := repo.Collection.UpdateOne(context.TODO(), filter, update)
	return err
}
