package models

type ChannelType struct {
	Id       string   `bson:"_id,omitempty"`
	Label    string   `bson:"label,omitempty"`
	Key      string   `bson:"key,omitempty"`
	RolesIds []string `bson:"rolesIds,omitempty"`
}
