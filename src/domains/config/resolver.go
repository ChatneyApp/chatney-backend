package config

import (
	graphql_models "chatney-backend/graph/model"
	repostiory "chatney-backend/src/application/repository"
	"chatney-backend/src/domains/config/models"
	"context"

	"go.mongodb.org/mongo-driver/v2/mongo"
)

type ConfigMutationsResolvers struct {
	RootAggregate *ConfigRootAggregateStruct
}

type ConfigQueryResolvers struct {
	RootAggregate *ConfigRootAggregateStruct
}

func (r *ConfigQueryResolvers) GetSystemConfig(ctx context.Context) ([]*graphql_models.SystemConfigValue, error) {
	configValues, err := r.RootAggregate.getAllSystemConfigValues(ctx)
	if err != nil {
		return nil, err
	}

	var out []*graphql_models.SystemConfigValue
	for _, configValue := range configValues {
		out = append(out, systemConfigValueToDTO(configValue))
	}

	return out, nil
}

func (r *ConfigMutationsResolvers) UpdateSystemConfigValue(ctx context.Context, configName string, configValue string) (*graphql_models.SystemConfigValue, error) {
	res, err := r.RootAggregate.UpdateSystemConfigValue(configName, configValue)

	if err != nil {
		return nil, err
	}

	return systemConfigValueToDTO(res), nil
}

func getRootAggregate(DB *mongo.Database) *ConfigRootAggregateStruct {
	return &ConfigRootAggregateStruct{
		SystemConfigRepo: &models.SystemConfigRepo{
			BaseRepo: &repostiory.BaseRepo[models.SystemConfigValue]{
				Collection: DB.Collection("system_config"),
			}},
	}
}

func GetMutationResolvers(DB *mongo.Database) ConfigMutationsResolvers {
	return ConfigMutationsResolvers{
		RootAggregate: getRootAggregate(DB),
	}
}

func GetQueryResolvers(DB *mongo.Database) ConfigQueryResolvers {
	return ConfigQueryResolvers{
		RootAggregate: getRootAggregate(DB),
	}
}
