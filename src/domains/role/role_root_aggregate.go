package role

import (
	"chatney-backend/src/domains/role/models"
	"context"

	"go.mongodb.org/mongo-driver/v2/bson"
)

type RoleRootAggregateStruct struct {
	roleRepo *models.RoleRepo
}

func (root *RoleRootAggregateStruct) getAllRoles(ctx context.Context) ([]*models.Role, error) {
	return root.roleRepo.GetAll(ctx, bson.M{})
}

func (root *RoleRootAggregateStruct) CreateNewRole(role *models.Role) (*models.Role, error) {
	return root.roleRepo.Create(context.TODO(), role)
}

func (root *RoleRootAggregateStruct) UpdateRole(role models.Role) (*models.Role, error) {
	return root.roleRepo.Update(context.TODO(), role.Id, &role)
}

func (root *RoleRootAggregateStruct) deleteRole(id string) (bool, error) {
	return root.roleRepo.Delete(context.TODO(), id)
}
