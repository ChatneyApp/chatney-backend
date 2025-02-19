package middlewares

import (
	"chatney-backend/src/domains/user"
	"chatney-backend/src/domains/user/models"
	"context"
	"net/http"
	"strconv"
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
			userId, err := strconv.Atoi(r.Header.Get("auth"))
			if err != nil {
				return
			}

			user, err := userRootAggr.GetUserByID(userId)
			if err != nil {
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
