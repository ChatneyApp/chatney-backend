package models

import "go.mongodb.org/mongo-driver/v2/bson"

type ChannelGroup struct {
	Id        bson.ObjectID   `bson:"_id,omitempty"`
	Name      string          `bson:"name,omitempty"`
	Channels  []bson.ObjectID `bson:"channels,omitempty"`
	Order     int             `bson:"order,omitempty"`
	Workspace bson.ObjectID   `bson:"workspace,omitempty"`
}
