package user

import (
	"chatney-backend/src/domains/user/models"
	"context"

	"github.com/google/uuid"
)

type UserRootAggregate struct {
	UserRepo *models.UserRepo
}

func (root *UserRootAggregate) NewUser(user models.User) (*models.User, error) {
	return nil, nil
}

func (root *UserRootAggregate) GetUserByID(id uuid.UUID) (*models.User, error) {
	return nil, nil
}

func (root *UserRootAggregate) DeleteUser(id string) (bool, error) {
	return root.UserRepo.Delete(context.TODO(), id)
}
