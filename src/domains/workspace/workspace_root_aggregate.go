package workspace

import (
	"chatney-backend/src/domains/workspace/models"
	"context"
)

type WorkspaceRootAggregate struct {
	workspaceRepo *models.WorkspaceRepo
}

func (root *WorkspaceRootAggregate) NewWs(ws models.Workspace) (*models.Workspace, error) {
	return root.workspaceRepo.Create(context.TODO(), &ws)
}

func (root *WorkspaceRootAggregate) DeleteWs(id string) (bool, error) {
	_, err := root.workspaceRepo.Delete(context.TODO(), id)
	if err != nil {
		return false, err
	}

	return true, nil
}

func (root *WorkspaceRootAggregate) GetWsById(id string) (*models.Workspace, error) {
	return root.workspaceRepo.GetByID(context.TODO(), id)
}
