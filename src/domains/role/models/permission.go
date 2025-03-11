package models

import "go.mongodb.org/mongo-driver/bson/primitive"

type Permission struct {
	ID    primitive.ObjectID `bson:"_id,omitempty"`
	Key   string             `bson:"key,omitempty"`
	Label string             `bson:"label,omitempty"`
}
