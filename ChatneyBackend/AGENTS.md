# AGENTS.md

## Project Context
This is a .NET 9 backend using GraphQL and PostgreSQL.
- **GraphQL Library:** HotChocolate (https://chillicream.com/docs/hotchocolate)
- **Database:** Dapper with PostgreSQL.
- **Architecture:** Clean Architecture / Domain-Driven Design.

## Core Principles
1. **Schema-First Approaches:** Use code-first approaches via HotChocolate type extensions.
2. **Async Always:** All database and service methods must be `async`.
3. **Nullability:** Strict C# nullable reference types.
4. **Error Handling:** Use GraphQL exceptions to return meaningful errors, not generic 500s.

## Project Structure
- `/Domains/`: Contains Domains and their Models. Also, GraphQL Queries and Mutations.
- `/Domain/`: Domain models and interfaces.
- `/Infra/`: Middleware, Repo generic & WebSockets.

## GraphQL Conventions
- **Naming:** Follow camelCase for GraphQL fields, PascalCase for C# models.
- **Queries:** Place in `/Domains/<Domain_Name>`.
- **Mutations:** Place in `/Domains/<Domain_Name>`.

## Key Commands
- **Run API:** `dotnet run`
- **Run Tests:** `dotnet test` in the `../Tests` directory.

## Boundaries
- Do not modify files in `../Tests` without understanding the testing strategy.
- Do not add packages without checking if they are compatible with .NET 9.
