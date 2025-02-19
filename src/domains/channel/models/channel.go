package models

import (
	"go.mongodb.org/mongo-driver/bson/primitive"
)

type Channel struct {
	Id          primitive.ObjectID `bson:"_id,omitempty"`
	Name        string             `bson:"name,omitempty"`
	ChannelType primitive.ObjectID `bson:"channel_type,omitempty"`
	Workspace   primitive.ObjectID `bson:"workspace,omitempty"`
}
