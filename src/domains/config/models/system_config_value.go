package models

type SystemConfigValue struct {
	Id    string `bson:"_id,omitempty"`
	Name  string `bson:"name,omitempty"`
	Value string `bson:"value,omitempty"`
}
