package models

import (
	"chatney-backend/src/application/repository"
	"context"

	"github.com/google/uuid"
	"go.mongodb.org/mongo-driver/v2/bson"
)

type RoleRepo struct {
	*repository.BaseRepo[Role]
}

func (repo *RoleRepo) ChangeRoleType(roleID string, newRoleTypeID uuid.UUID) error {
	filter := bson.M{"_id": roleID}
	update := bson.M{"$set": bson.M{"type": newRoleTypeID}}
	_, err := repo.Collection.UpdateOne(context.TODO(), filter, update)
	return err
}
