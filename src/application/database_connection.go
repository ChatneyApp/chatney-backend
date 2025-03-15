package application

import (
	"log"

	"go.mongodb.org/mongo-driver/v2/mongo"
	"go.mongodb.org/mongo-driver/v2/mongo/options"
)

type DB struct {
	Client *mongo.Database
}

func NewDatabase(config *EnvConfig) DB {
	client, err := mongo.Connect(options.Client().ApplyURI(config.MongoConnectionUri + "/" + config.MongoDbName + "?authSource=admin"))

	if err != nil {
		log.Fatal("Error connecting to DB.", err.Error())
	}

	return DB{Client: client.Database(config.MongoDbName)}
}
