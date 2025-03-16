package workspace

import (
	graphql_models "chatney-backend/graph/model"
	"chatney-backend/src/application/repository"
	"chatney-backend/src/domains/workspace/models"
	"context"

	"github.com/google/uuid"
	"go.mongodb.org/mongo-driver/v2/mongo"
)

type WorkspaceMutationsResolvers struct {
	RootAggregate *WorkspaceRootAggregate
}

type WorkspaceQueryResolvers struct {
	RootAggregate *WorkspaceRootAggregate
}

func (r *WorkspaceQueryResolvers) GetWorkspacesList(ctx context.Context) ([]*graphql_models.Workspace, error) {
	workspaces, err := r.RootAggregate.GetAllWorkspaces()
	if err != nil {
		return nil, err
	}

	var out []*graphql_models.Workspace
	for _, ws := range workspaces {
		out = append(out, WorkspaceToDTO(ws))
	}
	return out, nil
}

func (r *WorkspaceMutationsResolvers) DeleteWorkspace(ctx context.Context, workspaceId string) (bool, error) {
	return r.RootAggregate.DeleteWorkspace(workspaceId)
}

func (r *WorkspaceMutationsResolvers) CreateWorkspace(ctx context.Context, input graphql_models.MutateWorkspaceDto) (*graphql_models.Workspace, error) {
	newWs, err := r.RootAggregate.CreateWorkspace(&models.Workspace{
		Id:   uuid.NewString(),
		Name: input.Name,
	})
	if err != nil {
		return nil, err
	}
	return WorkspaceToDTO(newWs), nil
}

func (r *WorkspaceMutationsResolvers) UpdateWorkspace(ctx context.Context, input graphql_models.MutateWorkspaceDto, workspaceId string) (*graphql_models.Workspace, error) {
	updatedWs, err := r.RootAggregate.UpdateWorkspace(workspaceId, &models.Workspace{
		Name: input.Name,
	})
	if err != nil {
		return nil, err
	}
	return WorkspaceToDTO(updatedWs), nil
}

func getRootAggregate(DB *mongo.Database) *WorkspaceRootAggregate {
	return &WorkspaceRootAggregate{
		workspaceRepo: &models.WorkspaceRepo{
			BaseRepo: &repository.BaseRepo[models.Workspace]{
				Collection: DB.Collection("workspaces"),
			},
		},
	}
}

func GetMutationResolvers(DB *mongo.Database) WorkspaceMutationsResolvers {
	return WorkspaceMutationsResolvers{
		RootAggregate: getRootAggregate(DB),
	}
}

func GetQueryResolvers(DB *mongo.Database) WorkspaceQueryResolvers {
	return WorkspaceQueryResolvers{
		RootAggregate: getRootAggregate(DB),
	}
}
