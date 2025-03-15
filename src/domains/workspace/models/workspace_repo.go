package models

import (
	"chatney-backend/src/application/repository"
)

type WorkspaceRepo struct {
	*repository.BaseRepo[Workspace]
}
