package channel

import (
	"chatney-backend/src/domains/channel/models"
	"errors"

	"go.mongodb.org/mongo-driver/bson/primitive"
	"go.mongodb.org/mongo-driver/mongo"
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

/** Channel Group */
func (root *ChannelRootAggregate) CreateChannelGroup(chanelGroup models.ChannelGroup) (*models.ChannelGroup, error) {
	chanGroup, err := root.channelGroupRepo.CreateChannelGroup(chanelGroup)
	if err != nil {
		return nil, err
	}

	return chanGroup, nil
}

func (root *ChannelRootAggregate) DeleteChannelGroup(channelGroupId primitive.ObjectID) (bool, error) {
	err := root.channelGroupRepo.DeleteChannelGroup(channelGroupId)
	if err != nil {
		return false, err
	}
	return true, nil
}

func (root *ChannelRootAggregate) UpdateChannelGroupInfo(user models.ChannelGroup) (*models.ChannelGroup, error) {
	return nil, nil
}

func (root *ChannelRootAggregate) validatePutChannelIntoGroup(channelId primitive.ObjectID, groupId primitive.ObjectID) error {
	// Fetch the channel
	channel, err := root.channelRepo.FindByID(channelId)
	if err != nil {
		if err == mongo.ErrNoDocuments {
			return errors.New("channel not found")
		}
		return err
	}

	// Fetch the channel group
	group, err := root.channelGroupRepo.FindByID(groupId)
	if err != nil {
		if err == mongo.ErrNoDocuments {
			return errors.New("channel group not found")
		}
		return err
	}

	// Compare workspace IDs
	if channel.Workspace != group.Workspace {
		return errors.New("channel and channel group belong to different workspaces")
	}

	return nil
}

func (root *ChannelRootAggregate) PutChannelIntoChannelGroup(channelId primitive.ObjectID, groupId primitive.ObjectID) (bool, error) {
	err := root.validatePutChannelIntoGroup(channelId, groupId)
	if err != nil {
		return false, err
	}

	err = root.channelGroupRepo.PutChannelIntoChannelGroup(channelId, groupId)
	if err != nil {
		return false, err
	}
	return true, nil
}
