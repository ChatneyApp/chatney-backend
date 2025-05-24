package message

import (
	graphql_models "chatney-backend/graph/model"
	"chatney-backend/src/application"
	LogError "chatney-backend/src/application/error_utils"
	"chatney-backend/src/application/repository"
	"chatney-backend/src/domains/message/models"
	"context"
	"errors"
	"time"

	"github.com/google/uuid"
	"go.mongodb.org/mongo-driver/v2/mongo"
)

type MessageMutationsResolvers struct {
	RootAggregate *MessageRootAggregate
}

func (r *MessageMutationsResolvers) CreateMessage(ctx context.Context, input graphql_models.MutateMessageDto) (*graphql_models.Message, error) {
	var reactions []models.Reaction
	for _, react := range input.Reactions {
		if react != nil {
			reactions = append(reactions, models.Reaction{
				UserId:   react.UserID,
				Reaction: react.Reaction,
			})
		}
	}

	newMsg, err := r.RootAggregate.CreateMessage(&models.Message{
		Id:          uuid.NewString(),
		Content:     *input.Content,
		Attachments: input.Attachments,
		ChannelId:   input.ChannelID,
		UserId:      input.UserID,
		Reactions:   reactions,
		CreatedAt:   time.Now(),
		UpdatedAt:   time.Now(),
		Status:      "active",
	})
	if err != nil {
		return nil, err
	}
	return MessageToDTO(newMsg), nil
}

func (r *MessageMutationsResolvers) DeleteMessage(ctx context.Context, messageId string) (bool, error) {
	return r.RootAggregate.DeleteMessage(messageId)
}

func (r *MessageMutationsResolvers) UpdateMessage(ctx context.Context, input graphql_models.MutateMessageDto, messageId string) (*graphql_models.Message, error) {
	message := DTOToMessage(&input)
	if message == nil {
		return nil, nil
	}

	updatedMsg, err := r.RootAggregate.UpdateMessage(messageId, message)
	if err != nil {
		LogError.LogError(LogError.MakeError("MR001", "Error updating message", err))
		return nil, err
	}
	return MessageToDTO(updatedMsg), nil
}

type MessageQueryResolvers struct {
	RootAggregate *MessageRootAggregate
	Bucket        *application.Bucket
}

func (r *MessageQueryResolvers) GetMessagesList(ctx context.Context) ([]*graphql_models.Message, error) {
	messages, err := r.RootAggregate.GetAllMessages()
	if err != nil {
		LogError.LogError(LogError.MakeError("MR002", "Error getting messages list", err))
		return nil, err
	}

	var out = make([]*graphql_models.Message, 0)
	for _, msg := range messages {
		out = append(out, MessageToDTO(msg))
	}
	return out, nil
}

func (r *MessageQueryResolvers) GetPresignedAttachmentURL(ctx context.Context, key string) (string, error) {
	if key == "" {
		return "", errors.New("missing key parameter")
	}
	url, err := r.Bucket.GetPresignedURL(key, 15*time.Minute)
	if err != nil {
		return "", err
	}
	return url, nil
}

func getMessageRootAggregate(DB *mongo.Database) *MessageRootAggregate {
	return &MessageRootAggregate{
		messageRepo: &models.MessageRepo{
			BaseRepo: &repository.BaseRepo[models.Message]{
				Collection: DB.Collection("messages"),
			},
		},
	}
}

func GetMessageMutationResolvers(DB *mongo.Database) MessageMutationsResolvers {
	return MessageMutationsResolvers{
		RootAggregate: getMessageRootAggregate(DB),
	}
}

func GetMessageQueryResolvers(DB *mongo.Database, bucket *application.Bucket) MessageQueryResolvers {
	return MessageQueryResolvers{
		RootAggregate: getMessageRootAggregate(DB),
		Bucket:        bucket,
	}
}
