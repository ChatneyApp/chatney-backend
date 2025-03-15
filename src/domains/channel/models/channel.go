package models

type Channel struct {
	Id            string `bson:"_id,omitempty"`
	Name          string `bson:"name,omitempty"`
	ChannelTypeId string `bson:"channelTypeId,omitempty"`
	WorkspaceId   string `bson:"workspaceId,omitempty"`
}
