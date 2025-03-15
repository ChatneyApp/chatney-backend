package models

import (
	"chatney-backend/src/application/repository"
	"context"
	"errors"

	"github.com/google/uuid"
	"go.mongodb.org/mongo-driver/v2/bson"
	"go.mongodb.org/mongo-driver/v2/mongo"
)

type ChannelTypeRepo struct {
	*repository.BaseRepo[ChannelType]
}

// AddRoleToChannelType - adds a role to the roles array
func (repo *ChannelTypeRepo) AddRoleToChannelType(channelTypeID, roleID uuid.UUID) error {
	filter := bson.M{"_id": channelTypeID}
	update := bson.M{"$addToSet": bson.M{"roles": roleID}} // Prevents duplicates

	_, err := repo.Collection.UpdateOne(context.TODO(), filter, update)
	return err
}

// RemoveRoleFromChannelType - removes a role from the roles array
func (repo *ChannelTypeRepo) RemoveRoleFromChannelType(channelTypeID, roleID uuid.UUID) error {
	filter := bson.M{"_id": channelTypeID}
	update := bson.M{"$pull": bson.M{"roles": roleID}} // Removes the role

	_, err := repo.Collection.UpdateOne(context.TODO(), filter, update)
	return err
}

func (repo *ChannelTypeRepo) FindByID(channelTypeID uuid.UUID) (*Channel, error) {
	var channel Channel
	filter := bson.M{"_id": channelTypeID}

	err := repo.Collection.FindOne(context.TODO(), filter).Decode(&channel)
	if err != nil {
		if err == mongo.ErrNoDocuments {
			return nil, errors.New("channel type not found")
		}
		return nil, err
	}

	return &channel, nil
}
