package channel

import (
	"chatney-backend/src/domains/channel/models"
	"context"

	"go.mongodb.org/mongo-driver/v2/bson"
)

func (root *ChannelRootAggregate) getChannelGroupById(id string) (*models.ChannelGroup, error) {
	return root.channelGroupRepo.GetByID(context.TODO(), id)
}

func (root *ChannelRootAggregate) getChannelGroupListWithinWorkspace(workspaceId string) ([]*models.ChannelGroup, error) {
	return root.channelGroupRepo.GetAll(context.TODO(), &bson.M{"workspaceId": workspaceId})
}

func (root *ChannelRootAggregate) createChannelGroup(channelGroup *models.ChannelGroup) (*models.ChannelGroup, error) {
	return root.channelGroupRepo.Create(context.TODO(), channelGroup)
}

func (root *ChannelRootAggregate) deleteChannelGroup(channelGroupId string) (bool, error) {
	_, err := root.channelGroupRepo.Delete(context.TODO(), channelGroupId)
	if err != nil {
		return false, err
	}
	return true, nil
}

func (root *ChannelRootAggregate) updateChannelGroup(group *models.ChannelGroup) (*models.ChannelGroup, error) {
	return root.channelGroupRepo.Update(context.TODO(), group.Id, group)
}
