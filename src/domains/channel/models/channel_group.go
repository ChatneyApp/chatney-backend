package models

import "github.com/google/uuid"

type ChannelGroup struct {
	Id        uuid.UUID   `bson:"_id,omitempty"`
	Name      string      `bson:"name,omitempty"`
	Channels  []uuid.UUID `bson:"channels,omitempty"`
	Order     int         `bson:"order,omitempty"`
	Workspace uuid.UUID   `bson:"workspace,omitempty"`
}
