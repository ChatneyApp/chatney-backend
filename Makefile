dev:
	cd ChatneyBackend && dotnet run watch
compose:
	docker compose up -d
restore:
	dotnet restore