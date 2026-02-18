# chatney-backend
Open-source Chat Api 

## Installation and running
0. Install docker, env-cmd (`npm i env-cmd -g`) and wgo (dev runner for go with file watch).
1. Setup MongoDB and Minio `docker compose up -d` or just run `make install`
2. Start dev server `env-cmd -f .env.test wgo run server.go` or just run `make dev`

## Playground
1. GraphQl Playground hosted on `localhost:8080`, but I recommend using Altair Chrome Extension.
2. Test Minio bucket browser - `localhost:9001`. User: `minioadmin`, pass: `miniopass`.
3. Mongo database Admin dashboard - `localhost:8081`
4. Mongo Connection URI `mongodb://root:pass@mongo:27017/chatney`

## Development

## Minio

```shell
$env:aws_access_key_id="minioadmin"
$env:aws_secret_access_key="minioadminpass"
$env:aws_endpoint_url="http://localhost:9000"
aws s3 ls

# test:
curl -H "Content-Type: image/png" -sS --data-binary @mario.png -X PUT "${URL}"
```
