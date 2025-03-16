package middlewares

import (
	"chatney-backend/src/domains/user"
	"chatney-backend/src/domains/user/models"
	"context"
	"net/http"
)

type Ctx struct {
	rawJson map[string]interface{}
	user    *models.User
}

type CtxUserKeyType struct {
	name string
}

var CtxUserKey = CtxUserKeyType{name: "user"}

func SetUseAndContext(userRootAggr *user.UserRootAggregate) func(http.Handler) http.Handler {
	return func(next http.Handler) http.Handler {
		return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
			//userId := r.Header.Get("auth")
			userId := "3463434"

			println(userId, "auth header in context")

			user, err := userRootAggr.UserRepo.GetByID(context.TODO(), "sdsfsdf")
			if err != nil {
				println(err.Error())

				return
			}

			ctx := &Ctx{
				rawJson: map[string]interface{}{},
				user:    user,
			}

			overrided_context := context.WithValue(r.Context(), CtxUserKey, &ctx)

			// and call the next with our new context
			r = r.WithContext(overrided_context)
			next.ServeHTTP(w, r)
		})
	}
}
