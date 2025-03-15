package role

import (
	"chatney-backend/src/domains/role/models"
)

type RoleRootAggregateStruct struct {
	RoleRepo *models.RoleRepo
}

func (root *RoleRootAggregateStruct) CreateNewRole(role models.Role) (*models.Role, error) {
	newRole, err := root.RoleRepo.CreateRole(role)
	if err != nil {
		return nil, err
	}

	return newRole, nil
}

func (root *RoleRootAggregateStruct) UpdateRole(role models.Role) (*models.Role, error) {
	newRole, err := root.RoleRepo.UpdateRole(role.Id, role)
	if err != nil {
		return nil, err
	}

	return newRole, nil
}
