# Database schema as DBML

`db.dbml` describes the Chatney database schema in [DBML](https://dbml.dbdiagram.io/docs/) format. It is generated from the **live** database, so it always reflects what the migrations actually produced.

## Prerequisites

- [Node.js](https://nodejs.org/) (npx is used below; no global installs needed)
- The Postgres container running: `make compose`
- A migrated database — on a fresh DB, start the API (`make dev`) and run the `installWizard { installSystem }` GraphQL mutation once so the tables exist

Connection credentials come from `docker-compose.yaml`: database `chatney`, user `root`, password `pass`, port `5432`.

## Generate db.dbml from the database

Run from the repo root:

```bash
make db:dbml
# or directly:
npx -y -p @dbml/cli db2dbml postgres 'postgresql://root:pass@localhost:5432/chatney?schemas=public' -o db.dbml
```

`db2dbml` (from the official [@dbml/cli](https://www.npmjs.com/package/@dbml/cli) package) introspects the running Postgres instance — tables, columns, types, defaults, PKs, FKs, indexes — and writes them as DBML. The `?schemas=public` filter limits the output to the `public` schema, excluding bookkeeping tables like `dbo.migrations`.

Re-run this command after adding or changing migrations to keep `db.dbml` in sync.

## Render the diagram as SVG

```bash
make db:svg
# or directly:
npx -y -p @softwaretechnik/dbml-renderer dbml-renderer -i db.dbml -o db.svg
```

[dbml-renderer](https://www.npmjs.com/package/@softwaretechnik/dbml-renderer) is pure JS (bundled Graphviz) — no system Graphviz installation required. Open `db.svg` in any browser.

## Local live-preview server

For a diagram that refreshes automatically while you edit `db.dbml`, run these in two terminals:

**Terminal 1 — watch & re-render:**

```bash
make db:watch
# or directly:
npx -y nodemon --watch db.dbml --exec "npx -y -p @softwaretechnik/dbml-renderer dbml-renderer -i db.dbml -o db.svg"
```

**Terminal 2 — serve with auto-reload:**

```bash
make db:serve
# or directly:
npx -y live-server --open=db.svg
```

`live-server` opens the SVG in your browser and reloads the page whenever `db.svg` is regenerated.

## Alternative: dbdiagram.io

For interactive editing/sharing, paste the contents of `db.dbml` into [dbdiagram.io](https://dbdiagram.io) — it renders the same DBML with drag-and-drop layout.
