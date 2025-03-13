package models

import "go.mongodb.org/mongo-driver/v2/bson"

type ChannelType struct {
	Id    bson.ObjectID   `bson:"_id,omitempty"`
	Label string          `bson:"label,omitempty"`
	Key   string          `bson:"key,omitempty"`
	Roles []bson.ObjectID `bson:"roles,omitempty"`
}
