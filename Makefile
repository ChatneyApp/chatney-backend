dev:
	dotnet watch run --project ChatneyBackend
compose:
	docker compose up -d
restore:
	dotnet restore