package models

import "go.mongodb.org/mongo-driver/v2/bson"

type PermissionKey string

type RoleSettings struct {
	Base bool `bson:"base,omitempty"`
}

type Role struct {
	Id          bson.ObjectID   `bson:"_id,omitempty"`
	Name        string          `bson:"name,omitempty"`
	Settings    RoleSettings    `bson:"settings,omitempty"`
	Permissions []PermissionKey `bson:"permissions,omitempty"`
}
