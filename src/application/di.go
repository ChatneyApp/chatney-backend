package application

import (
	"chatney-backend/src/domains/role"
	"chatney-backend/src/domains/role/models"
)

var RoleRootAggregate *role.RoleRootAggregateStruct

func makeDI() {
	config, err := LoadEnvConfig()
	if err != nil {
		panic("Error loading env var" + err.Error())
	}

	db := NewDatabase(config)

	RoleRootAggregate = &role.RoleRootAggregateStruct{
		RoleRepo: &models.RoleRepo{Collection: db.Client.Collection("roles")},
	}
}
