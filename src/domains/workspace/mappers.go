package workspace

import (
	graphql_models "chatney-backend/graph/model"
	"chatney-backend/src/domains/workspace/models"
)

// dto_mappers/workspace.go
func WorkspaceToDTO(model *models.Workspace) *graphql_models.Workspace {
	if model == nil {
		return nil
	}
	return &graphql_models.Workspace{
		ID:   model.Id,
		Name: model.Name,
	}
}
