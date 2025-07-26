package workspace

import (
	"chatney-backend/src/domains/workspace/models"
	"context"

	"go.mongodb.org/mongo-driver/v2/bson"
)

type WorkspaceRootAggregate struct {
	WorkspaceRepo *models.WorkspaceRepo
}

func (root *WorkspaceRootAggregate) CreateWorkspace(ws *models.Workspace) (*models.Workspace, error) {
	return root.WorkspaceRepo.Create(context.TODO(), ws)
}

func (root *WorkspaceRootAggregate) DeleteWorkspace(workspaceId string) (bool, error) {
	return root.WorkspaceRepo.Delete(context.TODO(), workspaceId)
}

func (root *WorkspaceRootAggregate) UpdateWorkspace(workspaceId string, update *models.Workspace) (*models.Workspace, error) {
	return root.WorkspaceRepo.Update(context.TODO(), workspaceId, update)
}

func (root *WorkspaceRootAggregate) GetAllWorkspaces() ([]*models.Workspace, error) {
	return root.WorkspaceRepo.GetAll(context.TODO(), &bson.M{})
}

func (root *WorkspaceRootAggregate) GetFilteredWorkspaces(filter *bson.M) ([]*models.Workspace, error) {
	return root.WorkspaceRepo.GetAll(context.TODO(), filter)
}
