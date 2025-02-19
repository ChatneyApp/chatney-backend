package models

import (
	"go.mongodb.org/mongo-driver/bson/primitive"
)

type userStatus string

const (
	StatusActive   userStatus = "active"
	StatusInactive userStatus = "inactive"
	StatusBanned   userStatus = "banned"
	StatusMuted    userStatus = "muted"
)

type roles struct {
	Global    primitive.ObjectID            `bson:"global,omitempty"`
	Workspace map[primitive.ObjectID]string `bson:"workspace,omitempty"` // workspaceID -> roleID
	Channel   map[primitive.ObjectID]string `bson:"channel,omitempty"`   // channelID -> roleID
}

type channel struct {
	LastSeenMessage primitive.ObjectID `bson:"lastSeenMessage,omitempty"`
	Muted           bool               `bson:"muted,omitempty"`
}

type User struct {
	ID         primitive.ObjectID   `bson:"_id,omitempty"`
	Name       string               `bson:"name,omitempty"`
	Status     userStatus           `bson:"status,omitempty"`
	Email      string               `bson:"email,omitempty"`
	Roles      roles                `bson:"roles,omitempty"`
	Channels   map[string]channel   `bson:"channels,omitempty"` // channelID -> Channel struct
	Workspaces []primitive.ObjectID `bson:"workspaces,omitempty"`
}
