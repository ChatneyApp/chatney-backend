package models

import "github.com/google/uuid"

type Channel struct {
	Id          uuid.UUID `bson:"_id,omitempty"`
	Name        string    `bson:"name,omitempty"`
	ChannelType uuid.UUID `bson:"channel_type,omitempty"`
	Workspace   uuid.UUID `bson:"workspace,omitempty"`
}
