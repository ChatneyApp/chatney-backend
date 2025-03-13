package models

import (
	"go.mongodb.org/mongo-driver/v2/bson"
)

type userStatus string

const (
	StatusActive   userStatus = "active"
	StatusInactive userStatus = "inactive"
	StatusBanned   userStatus = "banned"
	StatusMuted    userStatus = "muted"
)

type roles struct {
	Global    bson.ObjectID            `bson:"global,omitempty"`
	Workspace map[bson.ObjectID]string `bson:"workspace,omitempty"` // workspaceID -> roleID
	Channel   map[bson.ObjectID]string `bson:"channel,omitempty"`   // channelID -> roleID
}

type channel struct {
	LastSeenMessage bson.ObjectID `bson:"lastSeenMessage,omitempty"`
	Muted           bool          `bson:"muted,omitempty"`
}

type User struct {
	Id         bson.ObjectID      `bson:"_id,omitempty"`
	Name       string             `bson:"name,omitempty"`
	Status     userStatus         `bson:"status,omitempty"`
	Email      string             `bson:"email,omitempty"`
	Roles      roles              `bson:"roles,omitempty"`
	Channels   map[string]channel `bson:"channels,omitempty"` // channelID -> Channel struct
	Workspaces []bson.ObjectID    `bson:"workspaces,omitempty"`
}
