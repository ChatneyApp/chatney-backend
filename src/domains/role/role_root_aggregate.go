package channel

import (
	"chatney-backend/src/domains/channel"
	"chatney-backend/src/domains/channel/models"
)

type PermissionsGroups struct {
	channel *channel.ChannelPermissionsGroup
}

type RoleRootAggregate struct {
	channelRepo      *models.ChannelRepo
	channelGroupRepo *models.ChannelGroupRepo
	channelTypeRepo  *models.ChannelTypeRepo
}

func (root *RoleRootAggregate) NewChannel(user models.Channel) (*models.Channel, error) {
	return nil, nil
}

func (root *RoleRootAggregate) GetChannelByID(id int) (*models.Channel, error) {
	return nil, nil
}
