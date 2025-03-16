package channel

import (
	graphql_models "chatney-backend/graph/model"
	"chatney-backend/src/domains/channel/models"
)

func channelGroupToDTO(group models.ChannelGroup) *graphql_models.ChannelGroup {
	return &graphql_models.ChannelGroup{
		ID:        group.Id,
		Channels:  group.ChannelsIds,
		Workspace: group.WorkspaceId,
		Order:     int32(group.Order),
		Name:      group.Name,
	}
}

func channelTypeToDTO(chType models.ChannelType) *graphql_models.ChannelType {
	return &graphql_models.ChannelType{
		ID:         chType.Id,
		Key:        chType.Key,
		BaseRoleID: chType.BaseRoleId,
		Label:      chType.Label,
	}
}
func channelToDTO(ch models.Channel) *graphql_models.Channel {
	return &graphql_models.Channel{
		ID:            ch.Id,
		Name:          ch.Name,
		ChannelTypeID: ch.ChannelTypeId,
		WorkspaceID:   ch.WorkspaceId,
	}
}
