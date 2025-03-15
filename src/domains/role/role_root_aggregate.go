package role

import (
	"chatney-backend/src/domains/role/models"
	"context"
)

type RoleRootAggregateStruct struct {
	roleRepo *models.RoleRepo
}

func (root *RoleRootAggregateStruct) CreateNewRole(role *models.Role) (*models.Role, error) {
	return root.roleRepo.Create(context.TODO(), role)
}

func (root *RoleRootAggregateStruct) UpdateRole(role models.Role) (*models.Role, error) {
	return root.roleRepo.Update(context.TODO(), role.Id, role)
}
