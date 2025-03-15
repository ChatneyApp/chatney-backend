package channel

import (
	"chatney-backend/src/domains/channel/models"
	"context"
	"errors"

	"github.com/google/uuid"
	"go.mongodb.org/mongo-driver/v2/mongo"
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
func (root *ChannelRootAggregate) CreateChannelGroup(channelGroup models.ChannelGroup) (*models.ChannelGroup, error) {
	return root.channelGroupRepo.Create(context.TODO(), &channelGroup)
}

func (root *ChannelRootAggregate) DeleteChannelGroup(channelGroupId uuid.UUID) (bool, error) {
	_, err := root.channelGroupRepo.Delete(context.TODO(), channelGroupId)
	if err != nil {
		return false, err
	}
	return true, nil
}

func (root *ChannelRootAggregate) UpdateChannelGroupInfo(user models.ChannelGroup) (*models.ChannelGroup, error) {
	return nil, nil
}

func (root *ChannelRootAggregate) validatePutChannelIntoGroup(channelId uuid.UUID, groupId uuid.UUID) error {
	// Fetch the channel
	channel, err := root.channelRepo.GetByID(context.TODO(), channelId)
	if err != nil {
		if err == mongo.ErrNoDocuments {
			return errors.New("channel not found")
		}
		return err
	}

	// Fetch the channel group
	group, err := root.channelGroupRepo.GetByID(context.TODO(), groupId)
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

func (root *ChannelRootAggregate) PutChannelIntoChannelGroup(channelId uuid.UUID, groupId uuid.UUID) (bool, error) {
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
