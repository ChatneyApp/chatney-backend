package user

import "chatney-backend/src/domains/user/models"

type UserRootAggregate struct {
	userRepo *models.UserRepo
}

func (root *UserRootAggregate) NewUser(user models.User) (*models.User, error) {
	return nil, nil
}

func (root *UserRootAggregate) GetUserByID(id int) (*models.User, error) {
	return nil, nil
}
