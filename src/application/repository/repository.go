package repostiory

import (
	"context"

	"go.mongodb.org/mongo-driver/v2/bson"
	"go.mongodb.org/mongo-driver/v2/mongo"
)

type BaseRepo[T any] struct {
	Collection *mongo.Collection
}

func NewBaseRepo[T any](db *mongo.Database, collectionName string) *BaseRepo[T] {
	return &BaseRepo[T]{Collection: db.Collection(collectionName)}
}

func (r *BaseRepo[T]) Create(ctx context.Context, entity *T) (*T, error) {
	res, err := r.Collection.InsertOne(ctx, entity)
	if err != nil {
		return nil, err
	}

	return r.GetByID(ctx, res.InsertedID.(bson.ObjectID))
}

func (r *BaseRepo[T]) GetByID(ctx context.Context, id bson.ObjectID) (*T, error) {
	var entity T
	err := r.Collection.FindOne(ctx, bson.M{"_id": id}).Decode(&entity)
	if err != nil {
		return nil, err
	}
	return &entity, nil
}

func (r *BaseRepo[T]) Update(ctx context.Context, id bson.ObjectID, update bson.M) (*mongo.UpdateResult, error) {
	return r.Collection.UpdateOne(ctx, bson.M{"_id": id}, bson.M{"$set": update})
}

func (r *BaseRepo[T]) Delete(ctx context.Context, id bson.ObjectID) (*mongo.DeleteResult, error) {
	return r.Collection.DeleteOne(ctx, bson.M{"_id": id})
}

func (r *BaseRepo[T]) GetAll(ctx context.Context, filter bson.M) ([]T, error) {
	var entities []T
	cursor, err := r.Collection.Find(ctx, filter)
	if err != nil {
		return nil, err
	}
	defer cursor.Close(ctx)

	if err := cursor.All(ctx, &entities); err != nil {
		return nil, err
	}
	return entities, nil
}
