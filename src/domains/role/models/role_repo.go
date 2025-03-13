package models

import (
	"context"
	"errors"

	"go.mongodb.org/mongo-driver/v2/bson"
	"go.mongodb.org/mongo-driver/v2/mongo"
)

type RoleRepo struct {
	Collection *mongo.Collection
}

func (repo *RoleRepo) FindByID(roleID bson.ObjectID) (*Role, error) {
	var role Role
	filter := bson.M{"_id": roleID}
	err := repo.Collection.FindOne(context.TODO(), filter).Decode(&role)
	if err != nil {
		if err == mongo.ErrNoDocuments {
			return nil, errors.New("role not found")
		}
		return nil, err
	}
	return &role, nil
}

func (repo *RoleRepo) CreateRole(role Role) (*Role, error) {
	role.Id = bson.NewObjectID()
	_, err := repo.Collection.InsertOne(context.TODO(), role)
	if err != nil {
		return nil, err
	}
	return &role, nil
}

func (repo *RoleRepo) DeleteRole(roleID bson.ObjectID) error {
	result, err := repo.Collection.DeleteOne(context.TODO(), bson.M{"_id": roleID})
	if err != nil {
		return err
	}
	if result.DeletedCount == 0 {
		return errors.New("role not found")
	}
	return nil
}

func (repo *RoleRepo) UpdateRole(roleID bson.ObjectID, updatedRole Role) (*Role, error) {
	filter := bson.M{"_id": roleID}
	update := bson.M{
		"$set": bson.M{
			"name":        updatedRole.Name,
			"settings":    updatedRole.Settings,
			"permissions": updatedRole.Permissions,
		},
	}
	_, err := repo.Collection.UpdateOne(context.TODO(), filter, update)
	if err != nil {
		return nil, err
	}
	var newRole Role
	err = repo.Collection.FindOne(context.TODO(), filter).Decode(&newRole)
	if err != nil {
		return nil, err
	}
	return &newRole, nil
}

func (repo *RoleRepo) ChangeRoleType(roleID bson.ObjectID, newRoleTypeID string) error {
	filter := bson.M{"_id": roleID}
	update := bson.M{"$set": bson.M{"type": newRoleTypeID}}
	_, err := repo.Collection.UpdateOne(context.TODO(), filter, update)
	return err
}
