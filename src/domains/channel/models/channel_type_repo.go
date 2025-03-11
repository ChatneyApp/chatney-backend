package models

import (
	"context"
	"errors"

	"go.mongodb.org/mongo-driver/bson"
	"go.mongodb.org/mongo-driver/bson/primitive"
	"go.mongodb.org/mongo-driver/v2/mongo"
)

type ChannelTypeRepo struct {
	db *mongo.Collection
}

// CreateChannelType - inserts a new ChannelType into the database
func (repo *ChannelTypeRepo) CreateChannelType(channelType ChannelType) (*ChannelType, error) {
	channelType.Id = primitive.NewObjectID() // Generate new ObjectID

	_, err := repo.db.InsertOne(context.TODO(), channelType)
	if err != nil {
		return nil, err
	}

	return &channelType, nil
}

// DeleteChannelType - deletes a ChannelType by ID
func (repo *ChannelTypeRepo) DeleteChannelType(channelTypeID primitive.ObjectID) error {
	result, err := repo.db.DeleteOne(context.TODO(), bson.M{"_id": channelTypeID})
	if err != nil {
		return err
	}
	if result.DeletedCount == 0 {
		return errors.New("channel type not found")
	}
	return nil
}

// UpdateChannelType - updates a ChannelType by ID with given updates
func (repo *ChannelTypeRepo) UpdateChannelType(channelTypeID primitive.ObjectID, updates bson.M) (*ChannelType, error) {
	filter := bson.M{"_id": channelTypeID}
	update := bson.M{"$set": updates}

	_, err := repo.db.UpdateOne(context.TODO(), filter, update)
	if err != nil {
		return nil, err
	}

	var updatedChannelType ChannelType
	err = repo.db.FindOne(context.TODO(), filter).Decode(&updatedChannelType)
	if err != nil {
		return nil, err
	}

	return &updatedChannelType, nil
}

// AddRoleToChannelType - adds a role to the roles array
func (repo *ChannelTypeRepo) AddRoleToChannelType(channelTypeID, roleID primitive.ObjectID) error {
	filter := bson.M{"_id": channelTypeID}
	update := bson.M{"$addToSet": bson.M{"roles": roleID}} // Prevents duplicates

	_, err := repo.db.UpdateOne(context.TODO(), filter, update)
	return err
}

// RemoveRoleFromChannelType - removes a role from the roles array
func (repo *ChannelTypeRepo) RemoveRoleFromChannelType(channelTypeID, roleID primitive.ObjectID) error {
	filter := bson.M{"_id": channelTypeID}
	update := bson.M{"$pull": bson.M{"roles": roleID}} // Removes the role

	_, err := repo.db.UpdateOne(context.TODO(), filter, update)
	return err
}

func (repo *ChannelTypeRepo) FindByID(channelTypeID primitive.ObjectID) (*Channel, error) {
	var channel Channel
	filter := bson.M{"_id": channelTypeID}

	err := repo.db.FindOne(context.TODO(), filter).Decode(&channel)
	if err != nil {
		if err == mongo.ErrNoDocuments {
			return nil, errors.New("channel type not found")
		}
		return nil, err
	}

	return &channel, nil
}
