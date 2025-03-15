package models

import (
	"chatney-backend/src/application/repository"
	"context"

	"go.mongodb.org/mongo-driver/v2/bson"
)

type ChannelGroupRepo struct {
	*repository.BaseRepo[ChannelGroup]
}

func (root *ChannelGroupRepo) PutChannelIntoChannelGroup(groupID, channelID string) error {
	filter := bson.M{"_id": groupID}
	update := bson.M{"$addToSet": bson.M{"channels": channelID}} // `$addToSet` prevents duplication

	_, err := root.Collection.UpdateOne(context.TODO(), filter, update)
	return err
}
