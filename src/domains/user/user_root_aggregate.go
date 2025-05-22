package user

import (
	"chatney-backend/src/application"
	"chatney-backend/src/domains/user/models"
	"context"
)

type UserRootAggregate struct {
	UserRepo *models.UserRepo
	Config   *application.EnvConfig
}

func (root *UserRootAggregate) createUser(user *models.User) (*models.User, error) {
	return root.UserRepo.Create(context.TODO(), user)
}

func (root *UserRootAggregate) deleteUser(userId string) (bool, error) {
	return root.UserRepo.Delete(context.TODO(), userId)
}

func (root *UserRootAggregate) updateUser(id string, update *models.User) (*models.User, error) {
	return root.UserRepo.Update(context.TODO(), id, update)
}

func (root *UserRootAggregate) getChannelUsersList(channelId string) ([]*models.User, error) {
	return root.UserRepo.GetChannelUsersList(context.TODO(), channelId)
}
