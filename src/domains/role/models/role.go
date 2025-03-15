package models

type PermissionKey string

type RoleSettings struct {
	Base bool `bson:"base,omitempty"`
}

type Role struct {
	Id          string          `bson:"_id,omitempty"`
	Name        string          `bson:"name,omitempty"`
	Settings    *RoleSettings   `bson:"settings,omitempty"`
	Permissions []PermissionKey `bson:"permissions,omitempty"`
}
