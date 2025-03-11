package models

import (
	"context"
	"errors"

	"go.mongodb.org/mongo-driver/bson"
	"go.mongodb.org/mongo-driver/bson/primitive"
	"go.mongodb.org/mongo-driver/v2/mongo"
)

type ChannelRepo struct {
	db *mongo.Collection
}

func (repo *ChannelRepo) FindByID(channelID primitive.ObjectID) (*Channel, error) {
	var channel Channel
	filter := bson.M{"_id": channelID}

	err := repo.db.FindOne(context.TODO(), filter).Decode(&channel)
	if err != nil {
		if err == mongo.ErrNoDocuments {
			return nil, errors.New("channel not found")
		}
		return nil, err
	}

	return &channel, nil
}

// CreateChannel - inserts a new Channel into MongoDB
func (repo *ChannelRepo) CreateChannel(channel Channel) (*Channel, error) {
	channel.Id = primitive.NewObjectID() // Generate new ObjectID

	_, err := repo.db.InsertOne(context.TODO(), channel)
	if err != nil {
		return nil, err
	}

	return &channel, nil
}

// DeleteChannel - deletes a Channel by ID
func (repo *ChannelRepo) DeleteChannel(channelID primitive.ObjectID) error {
	result, err := repo.db.DeleteOne(context.TODO(), bson.M{"_id": channelID})
	if err != nil {
		return err
	}
	if result.DeletedCount == 0 {
		return errors.New("channel not found")
	}
	return nil
}

// UpdateChannel - updates a Channel by replacing it with a strict structure
func (repo *ChannelRepo) UpdateChannel(channelID primitive.ObjectID, updatedChannel Channel) (*Channel, error) {
	filter := bson.M{"_id": channelID}

	// Ensure only allowed fields are updated by replacing the whole document (excluding ID)
	update := bson.M{
		"$set": bson.M{
			"name":         updatedChannel.Name,
			"channel_type": updatedChannel.ChannelType,
			"workspace":    updatedChannel.Workspace,
		},
	}

	_, err := repo.db.UpdateOne(context.TODO(), filter, update)
	if err != nil {
		return nil, err
	}

	var newChannel Channel
	err = repo.db.FindOne(context.TODO(), filter).Decode(&newChannel)
	if err != nil {
		return nil, err
	}

	return &newChannel, nil
}

// ChangeChannelType - updates the channel's type
func (repo *ChannelRepo) ChangeChannelType(channelID, newChannelTypeID primitive.ObjectID) error {
	filter := bson.M{"_id": channelID}
	update := bson.M{"$set": bson.M{"channel_type": newChannelTypeID}}

	_, err := repo.db.UpdateOne(context.TODO(), filter, update)
	return err
}
