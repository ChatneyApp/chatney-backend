package models

import (
	"github.com/google/uuid"
)

type PermissionKey string

type RoleSettings struct {
	Base bool `bson:"base,omitempty"`
}

type Role struct {
	Id          uuid.UUID       `bson:"_id,omitempty"`
	Name        string          `bson:"name,omitempty"`
	Settings    *RoleSettings   `bson:"settings,omitempty"`
	Permissions []PermissionKey `bson:"permissions,omitempty"`
}
