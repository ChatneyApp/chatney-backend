package models

import (
	"chatney-backend/src/application/repository"
)

type MessageRepo struct {
	*repository.BaseRepo[Message]
}
