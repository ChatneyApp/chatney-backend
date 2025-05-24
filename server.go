package main

import (
	"chatney-backend/graph"
	"chatney-backend/src/application"
	database "chatney-backend/src/application"
	"chatney-backend/src/application/middlewares"
	"chatney-backend/src/application/resolver"
	"chatney-backend/src/domains/user"
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
	config := database.LoadEnvConfig()

	go func() {
		application.RunWebSocket()
	}()

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
	router.Use(middlewares.SetUseAndContext(user.GetUserRootAggregate(db.Client)))

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

	database.InitDefaultSystemConfigValues(db.Client)

	log.Printf("connect to http://localhost:%s/ for GraphQL playground", config.ApiPort)
	err := http.ListenAndServe(":8080", router)
	if err != nil {
		panic(err)
	}
}
