package models

import (
	"chatney-backend/src/application/repository"
)

type UserRepo struct {
	*repository.BaseRepo[User]
}
