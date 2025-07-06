package user

import (
	graphql_models "chatney-backend/graph/model"
	"chatney-backend/src/application"
	chatContext "chatney-backend/src/application/context"
	LogError "chatney-backend/src/application/error_utils"
	"chatney-backend/src/application/repository"
	"chatney-backend/src/domains/user/models"
	"context"
	"crypto/md5"
	"encoding/hex"

	"github.com/google/uuid"
	"go.mongodb.org/mongo-driver/v2/mongo"
)

func getUserFromContext(ctx context.Context) *models.User {
	val := ctx.Value(chatContext.CtxUserKey)
	if val == nil {
		return nil
	}

	userCtx, ok := val.(*chatContext.Ctx)
	if !ok {
		return nil
	}

	return userCtx.User
}

type UserMutationsResolvers struct {
	RootAggregate *UserRootAggregate
}

type UserQueryResolvers struct {
	RootAggregate *UserRootAggregate
}

func (r *UserQueryResolvers) GetUser(ctx context.Context, ID string) (*graphql_models.User, error) {
	userFromCtx := getUserFromContext(ctx)
	println(userFromCtx.Name)

	user, err := r.RootAggregate.UserRepo.GetByID(ctx, ID)
	if err != nil {
		LogError.LogError(LogError.MakeError("UR004", "Get User failed", err))
		return nil, err
	}
	return UserToDTO(*user), nil
}

func (r *UserQueryResolvers) GetChannelUsersList(ctx context.Context, channelId string) ([]*graphql_models.User, error) {
	users, err := r.RootAggregate.getChannelUsersList(channelId)
	if err != nil {
		LogError.LogError(LogError.MakeError("UR003", "Getting channel failed", err))
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
		Id:         uuid.NewString(),
		Name:       userData.Name,
		Status:     models.UserStatus(userData.Status),
		Email:      userData.Email,
		Workspaces: userData.Workspaces,
	})
	if err != nil {
		LogError.LogError(LogError.MakeError("UR002", "Error create failedg", err))
		return nil, err
	}
	return UserToDTO(*newUser), nil
}

func (r *UserMutationsResolvers) UpdateUser(ctx context.Context, input graphql_models.UpdateUserDto, userId string) (*graphql_models.User, error) {
	updatedUser, err := r.RootAggregate.updateUser(userId, &models.User{
		Name:  input.Name,
		Email: input.Email,
	})
	if err != nil {
		LogError.LogError(LogError.MakeError("UR001", "User update failed", err))
		return nil, err
	}
	return UserToDTO(*updatedUser), nil
}

func (r *UserMutationsResolvers) UpdateUserAdmin(ctx context.Context, input graphql_models.UpdateUserAdminDto, userId string) (*graphql_models.User, error) {
	updatedUser, err := r.RootAggregate.updateUser(userId, &models.User{
		Name:       input.Name,
		Status:     models.UserStatus(input.Status),
		Email:      input.Email,
		Workspaces: input.Workspaces,
	})
	if err != nil {
		LogError.LogError(LogError.MakeError("UR001", "User update failed", err))
		return nil, err
	}
	return UserToDTO(*updatedUser), nil
}

func (r *UserMutationsResolvers) DeleteUser(ctx context.Context, userID string) (bool, error) {
	return r.RootAggregate.deleteUser(userID)
}

func (r *UserQueryResolvers) AuthorizeUser(ctx context.Context, login string, password string) (*graphql_models.UserAuthData, error) {
	return r.RootAggregate.authorizeUser(login, password)
}

func GetUserRootAggregate(DB *mongo.Database) *UserRootAggregate {
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
		RootAggregate: GetUserRootAggregate(DB),
	}
}

func GetQueryResolvers(DB *mongo.Database) UserQueryResolvers {
	return UserQueryResolvers{
		RootAggregate: GetUserRootAggregate(DB),
	}
}
