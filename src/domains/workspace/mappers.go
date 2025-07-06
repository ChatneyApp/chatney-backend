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

func WorkspacesToDTO(models []*models.Workspace) []*graphql_models.Workspace {
	if len(models) == 0 {
		return []*graphql_models.Workspace{}
	}

	result := make([]*graphql_models.Workspace, 0, len(models))
	for _, m := range models {
		result = append(result, WorkspaceToDTO(m))
	}
	return result
}
