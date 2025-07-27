dev:
	env-cmd -f .env.test wgo run server.go
gen:
	go run github.com/99designs/gqlgen generate
install:
	docker compose up -d