package models

import "go.mongodb.org/mongo-driver/v2/mongo"

type WorkspaceRepo struct {
	Collection *mongo.Collection
}

func (repo *WorkspaceRepo) SaveUser(user Workspace) error {
	return nil
}

func (repo *WorkspaceRepo) GetUserById(id int) error {
	return nil
}
