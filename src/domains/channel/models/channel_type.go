package models

import "github.com/google/uuid"

type ChannelType struct {
	Id    uuid.UUID   `bson:"_id,omitempty"`
	Label string      `bson:"label,omitempty"`
	Key   string      `bson:"key,omitempty"`
	Roles []uuid.UUID `bson:"roles,omitempty"`
}
