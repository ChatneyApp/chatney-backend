package models

import "github.com/google/uuid"

type Workspace struct {
	Id   uuid.UUID `bson:"_id,omitempty"`
	Name string    `bson:"name,omitempty"`
}
