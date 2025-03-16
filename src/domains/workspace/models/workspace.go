package models

type Workspace struct {
	Id   string `bson:"_id,omitempty"`
	Name string `bson:"name,omitempty"`
}
