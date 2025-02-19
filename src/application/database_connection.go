package database

import (
	"log"

	"go.mongodb.org/mongo-driver/v2/mongo"
	"go.mongodb.org/mongo-driver/v2/mongo/options"
)

type DB struct {
	db *mongo.Database
}

func NewDatabase(connectionUri string, dbName string) *DB {
	client, err := mongo.Connect(options.Client().ApplyURI(connectionUri))

	if err != nil {
		log.Fatal("Set your proper 'MONGODB_URI' environment variable.", err.Error())
	}

	return &DB{db: client.Database(dbName)}
}
