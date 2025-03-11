package models

import (
	"go.mongodb.org/mongo-driver/bson/primitive"
)

type Workspace struct {
	Id   primitive.ObjectID `bson:"_id,omitempty"`
	Name string             `bson:"name,omitempty"`
}
