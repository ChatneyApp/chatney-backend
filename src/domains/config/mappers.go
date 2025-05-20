package config

import (
	graphql_models "chatney-backend/graph/model"
	"chatney-backend/src/domains/config/models"
)

func systemConfigValueToDTO(systemConfigValue *models.SystemConfigValue) *graphql_models.SystemConfigValue {
	return &graphql_models.SystemConfigValue{
		Name:  systemConfigValue.Name,
		Value: systemConfigValue.Value,
	}
}
