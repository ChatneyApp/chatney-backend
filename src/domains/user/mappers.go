package user

import (
	graphql_models "chatney-backend/graph/model"
	"chatney-backend/src/domains/user/models"
)

func UserToDTO(model models.User) *graphql_models.User {
	return &graphql_models.User{
		ID:         model.ID,
		Name:       model.Name,
		Status:     graphql_models.UserStatus(model.Status),
		Email:      model.Email,
		Workspaces: model.Workspaces,
	}
}
