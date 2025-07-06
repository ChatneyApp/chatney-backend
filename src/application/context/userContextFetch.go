package application

import "chatney-backend/src/domains/user/models"

type Ctx struct {
	RawJson map[string]interface{}
	User    *models.User
}

type CtxUserKeyType struct {
	name string
}

var CtxUserKey = CtxUserKeyType{name: "user"}
