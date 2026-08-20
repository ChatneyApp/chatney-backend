# chatney-backend

[![Unit Tests](https://github.com/ChatneyApp/chatney-backend/actions/workflows/unit-tests.yml/badge.svg)](https://github.com/ChatneyApp/chatney-backend/actions/workflows/unit-tests.yml)

.NET 9 GraphQL API for Chatney — PostgreSQL, HotChocolate, WebSockets, and S3-compatible file storage.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/)

## Quick start

```bash
make compose    # start Postgres, RustFS (S3), and Kafka
make restore    # restore NuGet packages
make dev        # run API with hot reload
```

API: **http://localhost:3001**

## Local services

| Service | URL | Credentials |
| --- | --- | --- |
| GraphQL | http://localhost:3001/query | Banana Cake Pop (dev only) |
| WebSocket | `ws://localhost:3001/ws?userId={guid}` | — |
| PostgreSQL | `localhost:5432/chatney` | `root` / `pass` |
| RustFS (S3 API) | http://localhost:9000 | `admin` / `admin` |
| RustFS console | http://localhost:9001 | `admin` / `admin` |

## Configuration

Dev settings: `ChatneyBackend/appsettings.Development.json`

Required values: Postgres connection string, `UserPasswordSalt`, `JwtSecret`, and AWS/S3 credentials for attachments.

## First-time setup

After the API is running, call the `installWizard { installSystem }` GraphQL mutation to run migrations and seed base roles.

## Tests

```bash
dotnet test Tests/Tests.csproj
```

## Project layout

- `ChatneyBackend/Domains/` — models, queries, mutations
- `ChatneyBackend/Infra/` — repo layer, middleware, migrations
- `Tests/` — unit tests

See `ChatneyBackend/AGENTS.md` for coding conventions.
