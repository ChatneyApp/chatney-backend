dev:
	dotnet watch run --project ChatneyBackend
compose:
	docker compose up -d
restore:
	dotnet restore
db\:dbml:
	npx -y -p @dbml/cli db2dbml postgres "postgresql://root:pass@localhost:5432/chatney" -o db.dbml
db\:svg:
	npx -y -p @softwaretechnik/dbml-renderer dbml-renderer -i db.dbml -o db.svg
db\:watch:
	npx -y nodemon --watch db.dbml --exec "npx -y -p @softwaretechnik/dbml-renderer dbml-renderer -i db.dbml -o db.svg"
db\:serve:
	npx -y live-server --open=db.svg