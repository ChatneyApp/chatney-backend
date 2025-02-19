package models

import "go.mongodb.org/mongo-driver/v2/mongo"

type UserRepo struct {
	Collection *mongo.Collection
}

func (repo *UserRepo) SaveUser(user User) error {
	return nil
}

func (repo *UserRepo) GetUserById(id int) error {
	return nil
}
