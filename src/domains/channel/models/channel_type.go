package models

import "go.mongodb.org/mongo-driver/bson/primitive"

type ChannelType struct {
	ID    primitive.ObjectID   `bson:"_id,omitempty"`
	Label string               `bson:"label,omitempty"`
	Key   string               `bson:"key,omitempty"`
	Roles []primitive.ObjectID `bson:"roles,omitempty"`
}
