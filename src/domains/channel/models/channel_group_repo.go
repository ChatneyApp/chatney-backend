package models

import (
	"context"
	"errors"

	"go.mongodb.org/mongo-driver/bson"
	"go.mongodb.org/mongo-driver/bson/primitive"
	"go.mongodb.org/mongo-driver/v2/mongo"
)

type ChannelGroupRepo struct {
	db *mongo.Collection
}

func (root *ChannelGroupRepo) CreateChannelGroup(group ChannelGroup) (*ChannelGroup, error) {
	group.Id = primitive.NewObjectID()

	_, err := root.db.InsertOne(context.TODO(), group)
	if err != nil {
		return nil, err
	}

	return &group, nil
}

func (root *ChannelGroupRepo) DeleteChannelGroup(groupID primitive.ObjectID) error {
	result, err := root.db.DeleteOne(context.TODO(), bson.M{"_id": groupID})
	if err != nil {
		return err
	}
	if result.DeletedCount == 0 {
		return errors.New("channel group not found")
	}
	return nil
}

func (root *ChannelGroupRepo) UpdateChannelGroupInfo(groupID primitive.ObjectID, updates bson.M) (*ChannelGroup, error) {
	filter := bson.M{"_id": groupID}
	update := bson.M{"$set": updates}

	_, err := root.db.UpdateOne(context.TODO(), filter, update)
	if err != nil {
		return nil, err
	}

	var updatedGroup ChannelGroup
	err = root.db.FindOne(context.TODO(), filter).Decode(&updatedGroup)
	if err != nil {
		return nil, err
	}

	return &updatedGroup, nil
}

func (root *ChannelGroupRepo) PutChannelIntoChannelGroup(groupID, channelID primitive.ObjectID) error {
	filter := bson.M{"_id": groupID}
	update := bson.M{"$addToSet": bson.M{"channels": channelID}} // `$addToSet` prevents duplication

	_, err := root.db.UpdateOne(context.TODO(), filter, update)
	return err
}

func (repo *ChannelGroupRepo) FindByID(channelGroupID primitive.ObjectID) (*Channel, error) {
	var channel Channel
	filter := bson.M{"_id": channelGroupID}

	err := repo.db.FindOne(context.TODO(), filter).Decode(&channel)
	if err != nil {
		if err == mongo.ErrNoDocuments {
			return nil, errors.New("channel group not found")
		}
		return nil, err
	}

	return &channel, nil
}
