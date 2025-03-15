package repository

import (
	"context"
	"log"

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

	resId := res.InsertedID

	var insertedGroup T
	err = r.Collection.FindOne(context.TODO(), bson.M{"_id": resId}).Decode(&insertedGroup)
	if err != nil {
		log.Fatal(err)
	}

	return &insertedGroup, nil
}

func (r *BaseRepo[T]) GetByID(ctx context.Context, id string) (*T, error) {
	var entity T
	err := r.Collection.FindOne(ctx, bson.M{"Id": id}).Decode(&entity)
	if err != nil {
		return nil, err
	}
	return &entity, nil
}

func (r *BaseRepo[T]) Update(ctx context.Context, id string, update T) (*T, error) {
	_, err := r.Collection.UpdateOne(ctx, bson.M{"Id": id}, bson.M{"$set": update})
	if err != nil {
		return nil, err
	}

	return r.GetByID(ctx, id)
}

func (r *BaseRepo[T]) Delete(ctx context.Context, id string) (*mongo.DeleteResult, error) {
	return r.Collection.DeleteOne(ctx, bson.M{"Id": id})
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
