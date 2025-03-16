package models

import (
	"chatney-backend/src/application/repository"
	"context"
	"fmt"

	"go.mongodb.org/mongo-driver/v2/bson"
	"go.mongodb.org/mongo-driver/v2/mongo/options"
)

type UserRepo struct {
	*repository.BaseRepo[User]
}

func (r *UserRepo) GetChannelUsersList(ctx context.Context, channelId string) ([]*User, error) {
	filter := bson.D{
		{"channelSettings.channelId", channelId},
	}

	opts := options.Find()

	cursor, err := r.Collection.Find(ctx, filter, opts)
	if err != nil {
		return nil, fmt.Errorf("failed to find users with channel settings: %v", err)
	}
	defer cursor.Close(ctx)

	var users []*User
	for cursor.Next(ctx) {
		var user User
		if err := cursor.Decode(&user); err != nil {
			return nil, fmt.Errorf("failed to decode user: %v", err)
		}
		users = append(users, &user)
	}

	if err := cursor.Err(); err != nil {
		return nil, fmt.Errorf("cursor error: %v", err)
	}

	return users, nil
}
