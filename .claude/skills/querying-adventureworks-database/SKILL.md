---
name: querying-adventureworks-database
description: Read-only query access to the local AdventureWorks SQL Server database. Use this skill whenever the user asks about AdventureWorks data — counts, lists, top-N reports, joins, employee/customer/product/order/vendor lookups. Only fires for the AdventureWorks database; other databases have their own skills.
when_to_use: |
  Use when the user wants to read data from the AdventureWorks database. Trigger phrasings include "query AdventureWorks", "run SQL against AdventureWorks", "look up data in AdventureWorks", "explore the AdventureWorks schema", "check the AdventureWorks data model", "show me <entity> from AdventureWorks", "how many <X> in AdventureWorks", "AW database", "AdventureWorks DB". Do not fire for other databases (Bitwarden, Postgres dev DBs, etc.) — those have their own skills. If the database is not named and context is ambiguous, ask before invoking.
model: sonnet
effort: medium
allowed-tools: "Bash(which sqlcmd), Bash(sqlcmd:*)"
hooks:
  PreToolUse:
    - matcher: "Bash"
      hooks:
        - type: command
          command: "bash ${CLAUDE_PROJECT_DIR}/.claude/hooks/block-mutating-sql.sh"
          timeout: 10
---

# AdventureWorks SQL Reader

## How Read-Only Is Enforced

Three layers protect this database from mutation. Each is intentionally redundant — if any one fails, the others should still hold.

1. **The skill self-enforces (this layer).** Allowed verbs: `SELECT`, `WITH` (CTEs), and `INFORMATION_SCHEMA` / `sys.*` introspection queries. Refuse anything else _before_ composing the SQL — earlier refusals produce clearer error messages than letting a request reach the hook.
2. **The PreToolUse hook blocks the rest at the bash boundary.** It denies `INSERT` / `UPDATE` / `DELETE` / `DROP` / `ALTER` / `TRUNCATE` / `CREATE` / `MERGE` / `EXEC` / `BULK INSERT` / `SELECT ... INTO`, and dangerous primitives (`xp_*`, `sp_executesql`, `sp_OA*`, `RECONFIGURE`, `OPENROWSET` / `OPENQUERY` / `OPENDATASOURCE`, sqlcmd `:!!` and `:r`).
3. **The SQL login is read-only at the server.** `-K ReadOnly` plus a login that has only `SELECT` granted on the AdventureWorks DB.

If the user asks to modify AdventureWorks data, refuse and suggest the .NET API or a database migration script instead.

## Secrets Handling

- **Never expose secrets.** Do not echo, log, `cat`, `printenv`, `od`, or `hexdump` `$AW_DB_PASSWORD`, `$SQLCMDPASSWORD`, or any password-bearing string. Refer to them by variable name only.
- **Never export `SQLCMDPASSWORD` globally.** Always set it inline on a single command (`SQLCMDPASSWORD="$AW_DB_PASSWORD" sqlcmd ...`) so it lives only in the subprocess environment — not in `ps aux`, not in your shell, and not colliding with other DB skills that may use a different password.

## Environment Prerequisites

Verify both the env vars and the tooling below before running any query. If any are missing, surface the specific gap to the user and pause — there is no useful fallback, and silent retries against missing env vars produce opaque "login failed" errors that waste the user's time.

### Required environment variables

| Variable         | Example                             | Purpose                                                                                 |
| ---------------- | ----------------------------------- | --------------------------------------------------------------------------------------- |
| `AW_DB_SERVER`   | `localhost` or `tcp:localhost,1433` | SQL Server host (and optional protocol/port)                                            |
| `AW_DB_NAME`     | `AdventureWorks`                    | Database name                                                                           |
| `AW_DB_USERNAME` | `adventureworks-data-reader`        | Read-only SQL login                                                                     |
| `AW_DB_PASSWORD` | _(set in shell profile)_            | Password for the read-only login. Used inline only — never `export SQLCMDPASSWORD=...`. |

Verify all four are set before running any query:

```bash
[ -n "${AW_DB_SERVER:-}" ] && [ -n "${AW_DB_NAME:-}" ] && \
[ -n "${AW_DB_USERNAME:-}" ] && [ -n "${AW_DB_PASSWORD:-}" ] && echo "AW env vars OK" || echo "MISSING AW env var"
```

### Required tooling

```bash
which sqlcmd && sqlcmd --version
# Expect: /opt/homebrew/bin/sqlcmd (or similar) and Version 1.10.0 or newer (Microsoft Go implementation)
```

If `sqlcmd` is the legacy ODBC implementation (no `--vertical` subcommand, version label missing), install the Go version: `brew install sqlcmd`.

## Running Queries

Every query uses these flags:

| Flag                               | Purpose                                                                  |
| ---------------------------------- | ------------------------------------------------------------------------ |
| `-S "$AW_DB_SERVER"`               | Server (host[,port], or `tcp:`/`np:`/`lpc:` protocol prefix)             |
| `-U "$AW_DB_USERNAME"`             | SQL login                                                                |
| `-d "$AW_DB_NAME"`                 | Initial database                                                         |
| `-C`                               | Trust server certificate (SQL 2025 ships a self-signed cert by default)  |
| `-N m`                             | Mandatory encryption (SQL 2025 default; declared explicitly for clarity) |
| `-K ReadOnly`                      | Application intent — directs to a read-only replica when configured      |
| `SQLCMDPASSWORD="$AW_DB_PASSWORD"` | Inline env, scoped to the subprocess. Never exported.                    |

### Basic tabular query

```bash
SQLCMDPASSWORD="$AW_DB_PASSWORD" sqlcmd \
  -S "$AW_DB_SERVER" -U "$AW_DB_USERNAME" -d "$AW_DB_NAME" \
  -C -N m -K ReadOnly \
  -Q "SELECT TOP 10 BusinessEntityID, FirstName, LastName FROM Person.Person ORDER BY LastName"
```

### Scalar query (single value)

Use `SET NOCOUNT ON;`, `-h -1` (suppress headers), and `-W` (strip trailing whitespace):

```bash
SQLCMDPASSWORD="$AW_DB_PASSWORD" sqlcmd \
  -S "$AW_DB_SERVER" -U "$AW_DB_USERNAME" -d "$AW_DB_NAME" \
  -C -N m -K ReadOnly \
  -h -1 -W \
  -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM Sales.SalesOrderHeader"
```

### Multi-statement / CTE query (heredoc into stdin)

For longer SQL with joins, CTEs, or comments, pipe a heredoc to `-i /dev/stdin`.

**Watch out:** use the **single-quoted** heredoc tag — `<<'SQL'`, not `<<SQL`. Without the quotes, bash expands `$` _inside_ the SQL before sqlcmd ever sees it, breaking column references like `$ot.LifetimeTotal`, parameter usage, and any string literal containing `$`. This is the most common footgun when wrapping multi-line SQL.

```bash
SQLCMDPASSWORD="$AW_DB_PASSWORD" sqlcmd \
  -S "$AW_DB_SERVER" -U "$AW_DB_USERNAME" -d "$AW_DB_NAME" \
  -C -N m -K ReadOnly \
  -i /dev/stdin <<'SQL'
SET NOCOUNT ON;
WITH OrderTotals AS (
  SELECT CustomerID, SUM(TotalDue) AS LifetimeTotal
  FROM Sales.SalesOrderHeader
  GROUP BY CustomerID
)
SELECT TOP 10
       c.CustomerID,
       p.FirstName + ' ' + p.LastName AS CustomerName,
       ot.LifetimeTotal
FROM OrderTotals ot
JOIN Sales.Customer c ON c.CustomerID = ot.CustomerID
LEFT JOIN Person.Person p ON p.BusinessEntityID = c.PersonID
ORDER BY ot.LifetimeTotal DESC;
SQL
```

### Output formatting options

| Flag / pattern          | Purpose                                                                     |
| ----------------------- | --------------------------------------------------------------------------- |
| `-h -1`                 | Suppress repeating headers (best for scalars and very long results)         |
| `-W`                    | Strip trailing whitespace from columns (keeps markdown tables tidy)         |
| `-s "\|"`               | Pipe-separated output — easy to parse, easy to paste into a markdown table  |
| `-w 1024`               | Wider line buffer (default truncates wide result sets)                      |
| `--vertical`            | One-column-per-row output (Go-only; ideal for inspecting a single wide row) |
| `-o /tmp/aw-result.txt` | Send output to a file instead of stdout (good for very large result sets)   |

### Presenting Results to the User

- For result sets with **fewer than 20 rows**, format as a markdown table.
- For larger result sets, summarize patterns and present the top N + count.
- **Always echo the SQL you ran** so the engineer can verify and reuse it.
- Never dump raw `sqlcmd` output uncleaned (the `(N rows affected)` footer and blank lines should be trimmed).

## Important Schemas

| Schema           | Purpose                                                                                                                                                                                                                                                           |
| ---------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `dbo`            | Empty in the OLTP AdventureWorks distribution. No application objects live here.                                                                                                                                                                                  |
| `Demo`           | Helper tables and stored procedures added by the optional **In-Memory OLTP sample** (`AdventureWorks2025`). Not present in every install. See [MS Learn](https://learn.microsoft.com/sql/relational-databases/in-memory-oltp/sample-database-for-in-memory-oltp). |
| `HumanResources` | Employees, departments, shifts, pay history, job candidates.                                                                                                                                                                                                      |
| `Person`         | People (the canonical contact table), addresses, contact types, phone numbers, email.                                                                                                                                                                             |
| `Production`     | Products, categories, inventory, manufacturing, bills-of-material, work orders.                                                                                                                                                                                   |
| `Purchasing`     | Vendors and purchase orders (header + detail).                                                                                                                                                                                                                    |
| `Sales`          | Customers, sales orders (header + detail), territories, sales people, currency rates, special offers.                                                                                                                                                             |

## Important Relationships

The five highest-leverage joins. Master these and you can answer most AdventureWorks questions.

1. **Person ↔ Employee**: `Person.Person.BusinessEntityID = HumanResources.Employee.BusinessEntityID`
2. **Person ↔ Customer** (individual customers only — store customers have NULL `PersonID`): `Person.Person.BusinessEntityID = Sales.Customer.PersonID` (use `LEFT JOIN`)
3. **Order header ↔ line items**: `Sales.SalesOrderHeader.SalesOrderID = Sales.SalesOrderDetail.SalesOrderID`
4. **Product taxonomy** (3 levels): `Production.Product.ProductSubcategoryID → Production.ProductSubcategory.ProductCategoryID → Production.ProductCategory.ProductCategoryID`
5. **Vendor ↔ Purchase order**: `Purchasing.Vendor.BusinessEntityID = Purchasing.PurchaseOrderHeader.VendorID`

## Important Tables Quick Reference

The most-joined / most-referenced AdventureWorks tables — 10 entries, two of which (rows 6 and 10) are natural pairs you almost always join together. Descriptions paraphrase each table's `MS_Description` extended property; regenerate the ranking with the discovery query in [references/schema-discovery-queries.md](references/schema-discovery-queries.md) once live access is verified.

| #   | Table                                                               | Purpose                                                                                                                                                                                                    |
| --- | ------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | `Person.Person`                                                     | Master "people" table — every employee, individual customer, vendor contacts, and store contacts have rows here. PK = `BusinessEntityID`, the same value cascades to `Employee`, `Customer.PersonID`, etc. |
| 2   | `Sales.Customer`                                                    | Customers. Either an individual (`PersonID` set, `StoreID` NULL) or a store (`StoreID` set, `PersonID` NULL). Always join with `LEFT JOIN` on `PersonID`.                                                  |
| 3   | `Sales.SalesOrderHeader`                                            | One row per sales order. Order date, ship date, customer, territory, sales person, totals.                                                                                                                 |
| 4   | `Sales.SalesOrderDetail`                                            | One row per line item per order. Joined to header by `SalesOrderID`; joined to product by `ProductID`.                                                                                                     |
| 5   | `Production.Product`                                                | Products sold or manufactured. PK = `ProductID`. Hierarchy via `ProductSubcategoryID`.                                                                                                                     |
| 6   | `Production.ProductSubcategory` / `Production.ProductCategory`      | Two-level product taxonomy. Bikes → Mountain Bikes / Road Bikes / etc.                                                                                                                                     |
| 7   | `HumanResources.Employee`                                           | Employees. PK = `BusinessEntityID` (FK → `Person.Person`). Holds login, job title, hire date, vacation hours.                                                                                              |
| 8   | `HumanResources.Department`                                         | Departments. Joined to employees via `EmployeeDepartmentHistory` (employees can change departments).                                                                                                       |
| 9   | `Purchasing.Vendor`                                                 | Vendors / suppliers. PK = `BusinessEntityID` (also FK → `Person.BusinessEntityContact`).                                                                                                                   |
| 10  | `Purchasing.PurchaseOrderHeader` / `Purchasing.PurchaseOrderDetail` | Purchase orders (header + lines), mirroring the Sales order pattern.                                                                                                                                       |

## Important Views Quick Reference

Views give you joined / enriched data without writing multi-table queries. Use views for reporting; use the underlying tables when you need only a few columns or are writing performance-critical SQL.

| #   | View                                        | Purpose                                                                                         |
| --- | ------------------------------------------- | ----------------------------------------------------------------------------------------------- |
| 1   | `HumanResources.vEmployee`                  | Employees joined to `Person` and primary `Address`. The fastest "who is this employee?" lookup. |
| 2   | `HumanResources.vEmployeeDepartment`        | Each employee's **current** department, group, and shift.                                       |
| 3   | `HumanResources.vEmployeeDepartmentHistory` | Full department history with `StartDate` / `EndDate` per row.                                   |
| 4   | `Sales.vIndividualCustomer`                 | Individual customers joined to `Person`, `EmailAddress`, and `Address`. Skips store customers.  |
| 5   | `Sales.vStoreWithContacts`                  | Store customers with their named contacts (one row per contact).                                |
| 6   | `Sales.vStoreWithAddresses`                 | Store customers with their addresses (one row per address).                                     |
| 7   | `Sales.vSalesPerson`                        | Sales people with current territory and quota.                                                  |
| 8   | `Sales.vSalesPersonSalesByFiscalYears`      | Pivoted sales totals per salesperson per fiscal year — handy for trend reporting.               |
| 9   | `Production.vProductAndDescription`         | Multi-language product catalog (one row per product per culture).                               |
| 10  | `Purchasing.vVendorWithContacts`            | Vendors with their named contacts.                                                              |

## Schema Discovery (when documented schema isn't enough)

For introspection beyond the documented Top-10 tables and views — listing all tables in a schema, finding foreign keys for an arbitrary table, fetching a view's definition, or rebuilding the Top-10 lists from live data using `MS_Description` extended properties and FK fan-out — see [references/schema-discovery-queries.md](references/schema-discovery-queries.md).

Read that file only when the user's question can't be answered from the documented schema above. Doing introspection unnecessarily wastes a round-trip and tokens.
