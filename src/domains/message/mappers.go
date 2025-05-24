package message

import (
	graphql_models "chatney-backend/graph/model"
	"chatney-backend/src/domains/message/models"
)

func MessageToDTO(model *models.Message) *graphql_models.Message {
	if model == nil {
		return nil
	}

	var reactions []*graphql_models.Reaction
	for _, r := range model.Reactions {
		reactions = append(reactions, &graphql_models.Reaction{
			UserID:   r.UserId,
			Reaction: r.Reaction,
		})
	}

	return &graphql_models.Message{
		ID:          model.Id,
		ChannelID:   model.ChannelId,
		UserID:      model.UserId,
		Content:     model.Content,
		Attachments: model.Attachments,
		Status:      model.Status,
		CreatedAt:   model.CreatedAt,
		UpdatedAt:   model.UpdatedAt,
		Reactions:   reactions,
	}
}

func DTOToMessage(dto *graphql_models.MutateMessageDto) *models.Message {
	if dto == nil {
		return nil
	}

	var reactions []models.Reaction
	for _, r := range dto.Reactions {
		reactions = append(reactions, models.Reaction{
			UserId:   r.UserID,
			Reaction: r.Reaction,
		})
	}

	return &models.Message{
		ChannelId:   dto.ChannelID,
		UserId:      dto.UserID,
		Content:     *dto.Content,
		Attachments: dto.Attachments,
		Status:      dto.Status,
		Reactions:   reactions,
	}
}
