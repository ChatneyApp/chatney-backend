package user

import (
	graphql_models "chatney-backend/graph/model"
	"chatney-backend/src/application"
	"chatney-backend/src/application/repository"
	"chatney-backend/src/domains/user/models"
	"context"
	"crypto/md5"
	"encoding/hex"

	"github.com/google/uuid"
	"go.mongodb.org/mongo-driver/v2/mongo"
)

type UserMutationsResolvers struct {
	RootAggregate *UserRootAggregate
}

type UserQueryResolvers struct {
	RootAggregate *UserRootAggregate
}

func (r *UserQueryResolvers) GetUser(ctx context.Context, userID string) (*graphql_models.User, error) {
	user, err := r.RootAggregate.UserRepo.GetByID(ctx, userID)
	if err != nil {
		return nil, err
	}
	return UserToDTO(*user), nil
}

func (r *UserQueryResolvers) GetChannelUsersList(ctx context.Context, channelId string) ([]*graphql_models.User, error) {
	users, err := r.RootAggregate.getChannelUsersList(channelId)
	if err != nil {
		return nil, err
	}

	var out []*graphql_models.User
	for _, user := range users {
		out = append(out, UserToDTO(*user))
	}
	return out, nil
}

func (r *UserMutationsResolvers) CreateUser(ctx context.Context, userData graphql_models.CreateUserDto) (*graphql_models.User, error) {
	hash := md5.Sum([]byte(userData.Password + r.RootAggregate.Config.PasswordSalt))

	newUser, err := r.RootAggregate.createUser(&models.User{
		Password:   hex.EncodeToString(hash[:]),
		ID:         uuid.NewString(),
		Name:       userData.Name,
		Status:     models.UserStatus(userData.Status),
		Email:      userData.Email,
		Workspaces: userData.Workspaces,
	})
	if err != nil {
		return nil, err
	}
	return UserToDTO(*newUser), nil
}

func (r *UserMutationsResolvers) UpdateUser(ctx context.Context, input graphql_models.UpdateUserDto, userId string) (*graphql_models.User, error) {
	updatedUser, err := r.RootAggregate.updateUser(userId, &models.User{
		Name:       input.Name,
		Status:     models.UserStatus(input.Status),
		Email:      input.Email,
		Workspaces: input.Workspaces,
	})
	if err != nil {
		return nil, err
	}
	return UserToDTO(*updatedUser), nil
}

func (r *UserMutationsResolvers) DeleteUser(ctx context.Context, userID string) (bool, error) {
	return r.RootAggregate.deleteUser(userID)
}

func getUserRootAggregate(DB *mongo.Database) *UserRootAggregate {
	return &UserRootAggregate{
		Config: application.Config,
		UserRepo: &models.UserRepo{
			BaseRepo: &repository.BaseRepo[models.User]{
				Collection: DB.Collection("users"),
			},
		},
	}
}

func GetMutationResolvers(DB *mongo.Database) UserMutationsResolvers {
	return UserMutationsResolvers{
		RootAggregate: getUserRootAggregate(DB),
	}
}

func GetQueryResolvers(DB *mongo.Database) UserQueryResolvers {
	return UserQueryResolvers{
		RootAggregate: getUserRootAggregate(DB),
	}
}
