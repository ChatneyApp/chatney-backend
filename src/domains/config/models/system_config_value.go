package models

type SystemConfigValue struct {
	Name  string `bson:"name,omitempty"`
	Value string `bson:"value,omitempty"`
}
