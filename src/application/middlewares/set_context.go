package middlewares

import (
	chatContext "chatney-backend/src/application/context"
	LogError "chatney-backend/src/application/error_utils"
	"chatney-backend/src/domains/user"
	"chatney-backend/src/domains/user/models"
	"context"
	"errors"
	"net/http"
	"strings"

	"github.com/golang-jwt/jwt"
)

func SetUseAndContext(userRootAggr *user.UserRootAggregate) func(http.Handler) http.Handler {
	return func(next http.Handler) http.Handler {
		return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
			token := r.Header.Get("Authorization")
			var user *models.User = nil

			if len(token) > 0 && strings.Contains(token, "Bearer") {
				tokenValue := strings.TrimPrefix(token, "Bearer ")
				userId, err := extractUserIDFromJWT(tokenValue, []byte(userRootAggr.Config.JwtKey))
				if err != nil {
					LogError.LogError(LogError.MakeError("ST001", "User extraction from token failed", err))
					http.Error(w, "Unauthorized", http.StatusUnauthorized)
					return
				}

				fetchedUser, err := userRootAggr.UserRepo.GetByID(context.TODO(), userId)
				if err != nil {
					LogError.LogError(LogError.MakeError("ST002", "User not found using userId:"+userId, err))
					http.Error(w, "Unauthorized", http.StatusUnauthorized)
					return
				}

				user = fetchedUser
			}

			ctx := &chatContext.Ctx{
				RawJson: map[string]interface{}{},
				User:    user,
			}

			overrided_context := context.WithValue(r.Context(), chatContext.CtxUserKey, &ctx)

			// and call the next with our new context
			r = r.WithContext(overrided_context)
			next.ServeHTTP(w, r)
		})
	}
}

func extractUserIDFromJWT(tokenStr string, secretKey []byte) (string, error) {
	token, err := jwt.Parse(tokenStr, func(token *jwt.Token) (interface{}, error) {
		// Checking that algo is what we want
		if _, ok := token.Method.(*jwt.SigningMethodHMAC); !ok {
			return nil, errors.New("unexpected signing method")
		}
		return secretKey, nil
	})
	if err != nil {
		return "", err
	}

	if !token.Valid {
		return "", errors.New("invalid token")
	}

	claims, ok := token.Claims.(jwt.MapClaims)
	if !ok {
		return "", errors.New("invalid claims format")
	}

	sub, ok := claims["sub"].(string)
	if !ok || sub == "" {
		return "", errors.New("sub not found or invalid")
	}

	return sub, nil
}
