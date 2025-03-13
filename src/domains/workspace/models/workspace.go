package models

import "go.mongodb.org/mongo-driver/v2/bson"

type Workspace struct {
	Id   bson.ObjectID `bson:"_id,omitempty"`
	Name string        `bson:"name,omitempty"`
}
