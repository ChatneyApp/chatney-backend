package models

import "go.mongodb.org/mongo-driver/v2/bson"

type PermissionKey string

type Role struct {
	Id          bson.ObjectID   `bson:"_id,omitempty"`
	Name        string          `bson:"key,omitempty"`
	Type        string          `bson:"label,omitempty"`
	Permissions []PermissionKey `bson:"permissions"`
}
