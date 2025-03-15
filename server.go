package main

import (
	"chatney-backend/graph"
	database "chatney-backend/src/application"
	"chatney-backend/src/application/middlewares"
	"chatney-backend/src/application/repository"
	"chatney-backend/src/application/resolver"
	"chatney-backend/src/domains/user"
	"chatney-backend/src/domains/user/models"
	"log"
	"net/http"

	"github.com/99designs/gqlgen/graphql/handler"
	"github.com/99designs/gqlgen/graphql/handler/extension"
	"github.com/99designs/gqlgen/graphql/handler/lru"
	"github.com/99designs/gqlgen/graphql/handler/transport"
	"github.com/99designs/gqlgen/graphql/playground"
	"github.com/go-chi/chi"
	"github.com/vektah/gqlparser/v2/ast"

	"github.com/rs/cors"
)

func main() {
	config, err := database.LoadEnvConfig()
	if err != nil {
		panic("Error loading env var" + err.Error())
	}

	bucket := database.NewBucketConnection(config)
	// Adding file example
	res := bucket.PutFile("gqlgen.yml", "gqlgen.yml", "text/plain")
	println(res.Key+" size ", res.Size)

	router := chi.NewRouter()
	router.Use(cors.New(cors.Options{
		AllowedOrigins:   []string{"*"},
		AllowCredentials: true,
		Debug:            true,
		AllowedHeaders:   []string{"auth", "content-type"},
	}).Handler)

	db := database.NewDatabase(config)

	userRootAggr := &user.UserRootAggregate{
		UserRepo: &models.UserRepo{BaseRepo: &repository.BaseRepo[models.User]{
			Collection: db.Client.Collection("users"),
		}},
	}

	router.Use(middlewares.SetUseAndContext(userRootAggr))

	srv := handler.New(graph.NewExecutableSchema(graph.Config{Resolvers: &resolver.Resolver{
		DB: db.Client,
	}}))

	srv.AddTransport(transport.Options{})
	srv.AddTransport(transport.GET{})
	srv.AddTransport(transport.POST{})

	srv.SetQueryCache(lru.New[*ast.QueryDocument](1000))

	srv.Use(extension.Introspection{})
	srv.Use(extension.AutomaticPersistedQuery{
		Cache: lru.New[string](100),
	})

	router.Handle("/", playground.Handler("GraphQL playground", "/query"))
	router.Handle("/query", srv)

	log.Printf("connect to http://localhost:%s/ for GraphQL playground", config.ApiPort)
	err = http.ListenAndServe(":8080", router)
	if err != nil {
		panic(err)
	}
}
