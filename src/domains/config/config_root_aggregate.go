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

func (root *ConfigRootAggregateStruct) UpdateSystemConfigValue(configName string, configValue string) (*models.SystemConfigValue, error) {
	filter := bson.M{"name": configName}
	recordToUpdate, err := root.SystemConfigRepo.GetOne(context.TODO(), filter)
	if err != nil {
		return nil, err
	}

	recordToUpdate.Value = configValue
	updatedRecord, err := root.SystemConfigRepo.Update(context.TODO(), recordToUpdate.Id, recordToUpdate)

	if err != nil {
		return nil, err
	}

	return updatedRecord, nil
}
