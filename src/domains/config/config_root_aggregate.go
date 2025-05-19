package config

import (
	"chatney-backend/src/domains/config/models"
	"context"

	"go.mongodb.org/mongo-driver/v2/bson"
)

type ConfigRootAggregateStruct struct {
	SystemConfigRepo *models.SystemConfigRepo
}

func (root *ConfigRootAggregateStruct) getAllSystemConfigValues(ctx context.Context) ([]*models.SystemConfigValue, error) {
	return root.SystemConfigRepo.GetAll(ctx, bson.M{})
}

func (root *ConfigRootAggregateStruct) UpdateSystemConfigValue(configValue *models.SystemConfigValue) (*models.SystemConfigValue, error) {
	return root.SystemConfigRepo.Update(context.TODO(), configValue.Id, configValue)
}
