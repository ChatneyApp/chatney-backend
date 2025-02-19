# chatney-backend
Open-source Chat Api 

## Installation and running
0. Install docker, env-cmd (`npm i env-cmd -g`) and wgo (dev runner for go with file watch).
1. Setup MongoDB and Minio `docker compose up -d`
2. Start dev server `env-cmd -f .env.test wgo run server.go`

## Playground
1. GraphQl Playground hosted on `localhost:8080`, but I recommend using Altair Chrome Extension.
2. Test Minio bucket browser - `localhost:9001`. User: `minioadmin`, pass: `miniopass`.
3. Mongo database Admin dashboard - `localhost:8081`
4. Mongo Connection URI `mongodb://root:pass@mongo:27017/chatney`
