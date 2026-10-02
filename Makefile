dev:
	dotnet watch run --project ChatneyBackend
compose:
	docker compose up -d
restore:
	dotnet restore
db\:dbml:
	npx -y -p @dbml/cli db2dbml postgres "postgresql://root:pass@localhost:5432/chatney?schemas=public" -o db.dbml
db\:svg:
	npx -y -p @softwaretechnik/dbml-renderer dbml-renderer -i db.dbml -o db.svg
db\:watch:
	npx -y nodemon --watch db.dbml --exec "npx -y -p @softwaretechnik/dbml-renderer dbml-renderer -i db.dbml -o db.svg"
db\:serve:
	npx -y live-server --open=db.svg
gen-schema: export ASPNETCORE_ENVIRONMENT = Development
gen-schema:
	dotnet build ChatneyBackend -o ChatneyBackend/bin/schema-export
	cd ChatneyBackend && dotnet bin/schema-export/ChatneyBackend.dll schema export --output ../../chatney-frontend/graphql/schema.graphql
	node -e "const fs=require('fs');const f='../chatney-frontend/graphql/schema.graphql';fs.writeFileSync(f,fs.readFileSync(f,'utf8').replace(/\r\n/g,'\n'))"
