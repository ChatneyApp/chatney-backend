package message

import (
	"chatney-backend/src/domains/message/models"
	"context"

	"go.mongodb.org/mongo-driver/v2/bson"
)

type MessageRootAggregate struct {
	messageRepo *models.MessageRepo
}

func (root *MessageRootAggregate) CreateMessage(msg *models.Message) (*models.Message, error) {
	return root.messageRepo.Create(context.TODO(), msg)
}

func (root *MessageRootAggregate) DeleteMessage(messageId string) (bool, error) {
	return root.messageRepo.Delete(context.TODO(), messageId)
}

func (root *MessageRootAggregate) UpdateMessage(messageId string, update *models.Message) (*models.Message, error) {
	return root.messageRepo.Update(context.TODO(), messageId, update)
}

func (root *MessageRootAggregate) GetAllMessages() ([]*models.Message, error) {
	return root.messageRepo.GetAll(context.TODO(), &bson.M{})
}
