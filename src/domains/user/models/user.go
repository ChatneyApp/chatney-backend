package models

type UserStatus string

const (
	StatusActive   UserStatus = "active"
	StatusInactive UserStatus = "inactive"
	StatusBanned   UserStatus = "banned"
	StatusMuted    UserStatus = "muted"
)

type Role struct {
	Global    string `bson:"global" json:"global"`
	Workspace []struct {
		WorkspaceID string `bson:"workspaceId" json:"workspaceId"`
		RoleID      string `bson:"roleId" json:"roleId"`
	} `bson:"workspace" json:"workspace"`
	Channel []struct {
		ChannelID string `bson:"channelId" json:"channelId"`
		RoleID    string `bson:"roleId" json:"roleId"`
	} `bson:"channel" json:"channel"`
	ChannelTypes []struct {
		ChannelTypeID string `bson:"channelTypeId" json:"channelTypeId"`
		RoleID        string `bson:"roleId" json:"roleId"`
	} `bson:"channel_types" json:"channel_types"`
}

type ChannelSettings struct {
	ChannelID       string `bson:"channelId" json:"channelId"`
	LastSeenMessage string `bson:"lastSeenMessage" json:"lastSeenMessage"`
	Muted           bool   `bson:"muted" json:"muted"`
}

type User struct {
	Id               string            `bson:"_id" json:"_id"`
	Name             string            `bson:"name" json:"name"`
	Status           UserStatus        `bson:"status" json:"status"`
	Email            string            `bson:"email" json:"email"`
	Roles            Role              `bson:"roles" json:"roles"`
	ChannelsSettings []ChannelSettings `bson:"channelsSettings" json:"channelsSettings"`
	Workspaces       []string          `bson:"workspaces" json:"workspaces"`
	Password         string            `bson:"password" json:"password"`
}
