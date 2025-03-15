package role

import (
	graphql_models "chatney-backend/graph/model"
	repostiory "chatney-backend/src/application/repository"
	"chatney-backend/src/domains/role/models"
	"context"

	"github.com/google/uuid"
	"go.mongodb.org/mongo-driver/v2/mongo"
)

type RoleMutationsResolvers struct {
	RootAggregate *RoleRootAggregateStruct
}

type RoleQueryResolvers struct {
	RootAggregate *RoleRootAggregateStruct
}

func (r *RoleQueryResolvers) GetRolesList(ctx context.Context) ([]*graphql_models.Role, error) {
	roles, err := r.RootAggregate.getAllRoles(ctx)
	if err != nil {
		return nil, err
	}

	var out []*graphql_models.Role
	for _, group := range roles {
		out = append(out, RoleToDTO(group))
	}

	return out, nil
}

func (r *RoleMutationsResolvers) DeleteRole(ctx context.Context, roleId *string) (bool, error) {
	return r.RootAggregate.deleteRole(*roleId)
}

// CreateRole is the resolver for the createRole field.
func (r *RoleMutationsResolvers) CreateRole(ctx context.Context, roleData graphql_models.CreateRoleDto) (*graphql_models.Role, error) {
	permKeyList := make([]models.PermissionKey, len(roleData.Permissions))
	for i, s := range roleData.Permissions {
		permKeyList[i] = models.PermissionKey(s)
	}
	newRole, err := r.RootAggregate.CreateNewRole(&models.Role{
		Id:          uuid.NewString(),
		Name:        roleData.Name,
		Settings:    (*models.RoleSettings)(roleData.Settings),
		Permissions: permKeyList,
	})
	if err != nil {
		return nil, err
	}

	return RoleToDTO(newRole), nil
}

// EditRole is the resolver for the editRole field.
func (r *RoleMutationsResolvers) EditRole(ctx context.Context, roleData graphql_models.EditRoleDto) (*graphql_models.Role, error) {
	permKeyList := make([]models.PermissionKey, len(roleData.Permissions))
	for i, s := range roleData.Permissions {
		permKeyList[i] = models.PermissionKey(s)
	}
	updatedRole, err := r.RootAggregate.UpdateRole(models.Role{
		Id:          roleData.ID,
		Name:        roleData.Name,
		Settings:    (*models.RoleSettings)(roleData.Settings),
		Permissions: permKeyList,
	})
	if err != nil {
		return nil, err
	}

	return RoleToDTO(updatedRole), nil
}

func GetMutationResolvers(DB *mongo.Database) RoleMutationsResolvers {
	return RoleMutationsResolvers{
		RootAggregate: &RoleRootAggregateStruct{
			roleRepo: &models.RoleRepo{
				BaseRepo: &repostiory.BaseRepo[models.Role]{
					Collection: DB.Collection("roles"),
				}},
		}}
}

func GetQueryResolvers(DB *mongo.Database) RoleQueryResolvers {
	return RoleQueryResolvers{
		RootAggregate: &RoleRootAggregateStruct{
			roleRepo: &models.RoleRepo{
				BaseRepo: &repostiory.BaseRepo[models.Role]{
					Collection: DB.Collection("roles"),
				}},
		}}
}
