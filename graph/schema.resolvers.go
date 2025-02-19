package graph

// This file will be automatically regenerated based on the schema, any resolver implementations
// will be copied through when generating and any unknown code will be moved to the end.
// Code generated by github.com/99designs/gqlgen version v0.17.66

import (
	"chatney-backend/graph/model"
	"context"
)

// CreateTodo is the resolver for the createTodo field.
func (r *mutationResolver) CreateTodo(ctx context.Context, input model.NewTodo) (*model.Todo, error) {
	println("rendering logs with print ln")
	return &model.Todo{
		ID:   "234324sdg3w",
		Text: "sdfsdfsd",
		Done: true,
	}, nil
}

// Todos is the resolver for the todos field.
func (r *queryResolver) Todos(ctx context.Context) ([]*model.Todo, error) {
	println("create todos failed")
	return []*model.Todo{
		{
			ID:   "234324sdg3w",
			Text: "sdfsdfsd",
			Done: true,
			User: &model.User{ID: "sdfsdf", Name: "sdfsdfsdf"},
		},
	}, nil
}

// Mutation returns MutationResolver implementation.
func (r *Resolver) Mutation() MutationResolver { return &mutationResolver{r} }

// Query returns QueryResolver implementation.
func (r *Resolver) Query() QueryResolver { return &queryResolver{r} }

type mutationResolver struct{ *Resolver }
type queryResolver struct{ *Resolver }
