package channel

import (
	"chatney-backend/src/domains/channel/models"
)

type ChannelRootAggregate struct {
	channelRepo      *models.ChannelRepo
	channelGroupRepo *models.ChannelGroupRepo
	channelTypeRepo  *models.ChannelTypeRepo
}

func (root *ChannelRootAggregate) NewChannel(user models.Channel) (*models.Channel, error) {
	return nil, nil
}

func (root *ChannelRootAggregate) GetChannelByID(id int) (*models.Channel, error) {
	return nil, nil
}
