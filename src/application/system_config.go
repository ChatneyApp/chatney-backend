package application

import (
	repostiory "chatney-backend/src/application/repository"
	"chatney-backend/src/domains/config/models"
	"context"
	"go.mongodb.org/mongo-driver/v2/bson"
	"go.mongodb.org/mongo-driver/v2/mongo"
)

var defaultSystemConfigCollection = []models.SystemConfigValue{
	{
		Name:  "messages.sendCooldown",
		Value: "600",
		Type:  "int",
	},
	{
		Name:  "events.typesEnabled",
		Value: "message.sent,message.edited,message.deleted",
		Type:  "string[]",
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
			toInsert = append(toInsert, def)
		}
	}

	if len(toInsert) > 0 {
		_, _ = repo.Collection.InsertMany(context.TODO(), toInsert)
	}
}
