package models

import "go.mongodb.org/mongo-driver/v2/bson"

type Channel struct {
	Id          bson.ObjectID `bson:"_id,omitempty"`
	Name        string        `bson:"name,omitempty"`
	ChannelType bson.ObjectID `bson:"channel_type,omitempty"`
	Workspace   bson.ObjectID `bson:"workspace,omitempty"`
}
