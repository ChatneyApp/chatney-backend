package models

type ChannelGroup struct {
	Id          string   `bson:"_id,omitempty"`
	Name        string   `bson:"name,omitempty"`
	ChannelsIds []string `bson:"channelsIds,omitempty"`
	Order       int32    `bson:"order,omitempty"`
	WorkspaceId string   `bson:"workspaceId,omitempty"`
}
