package application

import (
	repostiory "chatney-backend/src/application/repository"
	"chatney-backend/src/domains/config/models"
	"context"
	"github.com/google/uuid"
	"go.mongodb.org/mongo-driver/v2/bson"
	"go.mongodb.org/mongo-driver/v2/mongo"
)

var defaultSystemConfigCollection = []models.SystemConfigValue{
	{
		Id:    uuid.NewString(),
		Name:  "messages.sendCooldown",
		Value: "600",
	},
	{
		Id:    uuid.NewString(),
		Name:  "events.typesEnabled",
		Value: "message.sent,message.edited,message.deleted",
	},
}

func InitDefaultSystemConfigValues(DB *mongo.Database) {
	repo := repostiory.NewBaseRepo[models.SystemConfigValue](DB, "system_config")

	existingValues, err := repo.GetAll(context.TODO(), bson.M{})
	if err != nil {
		return
	}

	existingNames := make(map[string]struct{})
	for _, v := range existingValues {
		if v != nil {
			existingNames[v.Name] = struct{}{}
		}
	}

	var toInsert []models.SystemConfigValue
	for _, def := range defaultSystemConfigCollection {
		if _, found := existingNames[def.Name]; !found {
			// Always generate a new ID for insertion
			toInsert = append(toInsert, def)
		}
	}

	if len(toInsert) > 0 {
		_, _ = repo.Collection.InsertMany(context.TODO(), toInsert)
	}
}
