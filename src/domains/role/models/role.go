package models

import "go.mongodb.org/mongo-driver/bson/primitive"

type Role struct {
	ID          primitive.ObjectID   `bson:"_id,omitempty"`
	Name        string               `bson:"key,omitempty"`
	Type        string               `bson:"label,omitempty"`
	Permissions []primitive.ObjectID `bson:"permissions"`
}
