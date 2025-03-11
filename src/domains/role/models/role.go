package models

import (
	"go.mongodb.org/mongo-driver/bson/primitive"
)

type PermissionKey string

type Role struct {
	Id          primitive.ObjectID `bson:"_id,omitempty"`
	Name        string             `bson:"key,omitempty"`
	Type        string             `bson:"label,omitempty"`
	Permissions []PermissionKey    `bson:"permissions"`
}
