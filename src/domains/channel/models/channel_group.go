package models

import "go.mongodb.org/mongo-driver/bson/primitive"

type ChannelGroup struct {
	Id        primitive.ObjectID   `bson:"_id,omitempty"`
	Name      string               `bson:"name,omitempty"`
	Channels  []primitive.ObjectID `bson:"channels,omitempty"`
	Order     int                  `bson:"order,omitempty"`
	Workspace primitive.ObjectID   `bson:"workspace,omitempty"`
}
