package user

import (
	graphql_models "chatney-backend/graph/model"
	"chatney-backend/src/application"
	LogError "chatney-backend/src/application/error_utils"
	"chatney-backend/src/domains/user/models"
	"context"
	"crypto/md5"
	"encoding/hex"
	"fmt"
	"time"

	"github.com/golang-jwt/jwt/v5"
	"go.mongodb.org/mongo-driver/v2/bson"
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

func (root *UserRootAggregate) authorizeUser(login string, password string) (*graphql_models.UserAuthData, error) {
	hash := md5.Sum([]byte(password + root.Config.PasswordSalt))

	// find user by password and login
	userRows, err := root.UserRepo.GetAll(context.TODO(), bson.M{
		"password": hex.EncodeToString(hash[:]),
		"email":    login,
	})

	if err != nil {
		LogError.LogError(LogError.MakeError("URA003", "User not found", err))
		return nil, err
	}

	if len(userRows) < 1 {
		LogError.LogError(LogError.MakeError("URA002", "User not found", err))
		return nil, fmt.Errorf("user not found")
	}

	userRow := userRows[0]

	// creating token
	token := jwt.NewWithClaims(jwt.SigningMethodHS256, jwt.MapClaims{
		"sub": userRow.Id,
		"exp": time.Now().Add(7 * 24 * time.Hour).Unix(),
	})

	tokenString, err := token.SignedString([]byte(root.Config.JwtKey))

	if err != nil {
		LogError.LogError(LogError.MakeError("URA001", "Error siging string", err))
		return nil, err
	}

	return &graphql_models.UserAuthData{
		Token: tokenString,
		ID:    userToDTO(*userRow).ID,
	}, nil
}
