package models

type userStatus string

const (
	StatusActive   userStatus = "active"
	StatusInactive userStatus = "inactive"
	StatusBanned   userStatus = "banned"
	StatusMuted    userStatus = "muted"
)

type roles struct {
	Global        string            `bson:"global,omitempty"`
	WorkspacesMap map[string]string `bson:"workspace,omitempty"` // workspaceID -> roleID
	ChannelsMap   map[string]string `bson:"channel,omitempty"`   // channelID -> roleID
}

type channel struct {
	LastSeenMessage string `bson:"lastSeenMessage,omitempty"`
	Muted           bool   `bson:"muted,omitempty"`
}

type User struct {
	Id            string             `bson:"_id,omitempty"`
	Name          string             `bson:"name,omitempty"`
	Status        userStatus         `bson:"status,omitempty"`
	Email         string             `bson:"email,omitempty"`
	Roles         roles              `bson:"roles,omitempty"`
	ChannelsIds   map[string]channel `bson:"channels,omitempty"` // channelID -> Channel struct
	WorkspacesIds []string           `bson:"workspaces,omitempty"`
}
