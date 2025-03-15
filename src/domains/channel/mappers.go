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
