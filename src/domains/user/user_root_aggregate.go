package user

import (
	"chatney-backend/src/domains/user/models"

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
