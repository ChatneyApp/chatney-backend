package workspace

import "chatney-backend/src/domains/workspace/models"

type WorkspaceRootAggregate struct {
	UserRepo *models.WorkspaceRepo
}

func (root *WorkspaceRootAggregate) NewUser(user models.Workspace) (*models.Workspace, error) {
	return nil, nil
}

func (root *WorkspaceRootAggregate) GetUserByID(id int) (*models.Workspace, error) {
	return nil, nil
}
