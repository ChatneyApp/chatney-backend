package models

import (
	"chatney-backend/src/application/repository"
)

type SystemConfigRepo struct {
	*repository.BaseRepo[SystemConfigValue]
}
