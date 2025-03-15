package models

import "github.com/google/uuid"

type userStatus string

const (
	StatusActive   userStatus = "active"
	StatusInactive userStatus = "inactive"
	StatusBanned   userStatus = "banned"
	StatusMuted    userStatus = "muted"
)

type roles struct {
	Global    string            `bson:"global,omitempty"`
	Workspace map[string]string `bson:"workspace,omitempty"` // workspaceID -> roleID
	Channel   map[string]string `bson:"channel,omitempty"`   // channelID -> roleID
}

type channel struct {
	LastSeenMessage string `bson:"lastSeenMessage,omitempty"`
	Muted           bool   `bson:"muted,omitempty"`
}

type User struct {
	Id         uuid.UUID             `bson:"_id,omitempty"`
	Name       string                `bson:"name,omitempty"`
	Status     userStatus            `bson:"status,omitempty"`
	Email      string                `bson:"email,omitempty"`
	Roles      roles                 `bson:"roles,omitempty"`
	Channels   map[uuid.UUID]channel `bson:"channels,omitempty"` // channelID -> Channel struct
	Workspaces []uuid.UUID           `bson:"workspaces,omitempty"`
}
