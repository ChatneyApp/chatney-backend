package config

import (
	"chatney-backend/src/domains/role/models"
)

const (
	updateSystemConfigValue models.PermissionKey = "systemConfig.updateValue"
)

var SystemConfigPermissions = []models.PermissionKey{
	updateSystemConfigValue,
}
