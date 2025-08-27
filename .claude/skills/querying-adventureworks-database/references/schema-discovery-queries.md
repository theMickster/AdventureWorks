# Schema Discovery Queries — AdventureWorks

Read-only `INFORMATION_SCHEMA` and `sys.*` queries for discovering tables, columns, relationships, and view definitions in AdventureWorks. All queries are pure SELECTs and pass the read-only PreToolUse hook.

## When to read this file

Read this file when the SKILL.md body's documented schema is insufficient — for example:

- The user asks about a table or column not listed in SKILL.md's "Important Tables" / "Important Views" sections.
- The user asks "what's in schema X?" and you need a comprehensive list.
- You need to **rebuild** the Top-10 tables/views lists in SKILL.md from live data (use the FK fan-out and `MS_Description` queries below).
- You need a view's definition to understand its joins.

If the answer is in SKILL.md's documented schema, prefer that — it's faster and cheaper than running an introspection query.

## Running these queries

All examples below are SQL only. Wrap them in the standard sqlcmd invocation from SKILL.md:

```bash
SQLCMDPASSWORD="$AW_DB_PASSWORD" sqlcmd \
  -S "$AW_DB_SERVER" -U "$AW_DB_USERNAME" -d "$AW_DB_NAME" \
  -C -N m -K ReadOnly \
  -Q "<sql here>"
```

Substitute placeholders like `{{SCHEMA_NAME}}` and `{{TABLE_NAME}}` with concrete values before running.

---

## List all tables in a schema

```sql
SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = '{{SCHEMA_NAME}}' AND TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
```

## List all views in a schema

```sql
SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.VIEWS
WHERE TABLE_SCHEMA = '{{SCHEMA_NAME}}'
ORDER BY TABLE_NAME;
```

## Describe a table's columns

```sql
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = '{{SCHEMA_NAME}}' AND TABLE_NAME = '{{TABLE_NAME}}'
ORDER BY ORDINAL_POSITION;
```

## Find foreign-key relationships for a table

The `OBJECT_SCHEMA_NAME(...)` calls disambiguate same-named tables across schemas (e.g. an `Employee` in two schemas).

```sql
SELECT
    fk.name AS FK_Name,
    OBJECT_SCHEMA_NAME(fk.parent_object_id)     AS ParentSchema,
    tp.name AS ParentTable,
    cp.name AS ParentColumn,
    OBJECT_SCHEMA_NAME(fk.referenced_object_id) AS ReferencedSchema,
    tr.name AS ReferencedTable,
    cr.name AS ReferencedColumn
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
JOIN sys.tables  tp ON fkc.parent_object_id     = tp.object_id
JOIN sys.columns cp ON fkc.parent_object_id     = cp.object_id AND fkc.parent_column_id     = cp.column_id
JOIN sys.tables  tr ON fkc.referenced_object_id = tr.object_id
JOIN sys.columns cr ON fkc.referenced_object_id = cr.object_id AND fkc.referenced_column_id = cr.column_id
WHERE tp.name = '{{TABLE_NAME}}' OR tr.name = '{{TABLE_NAME}}'
ORDER BY fk.name;
```

## Search for columns by name across a schema

```sql
SELECT TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = '{{SCHEMA_NAME}}' AND COLUMN_NAME LIKE '%{{SEARCH_TERM}}%'
ORDER BY TABLE_NAME, COLUMN_NAME;
```

## Get a view's definition

`SET TEXTSIZE` is bumped because the default 4 KB will truncate larger AdventureWorks views.

```sql
SET TEXTSIZE 1000000;
SELECT OBJECT_DEFINITION(OBJECT_ID('{{SCHEMA_NAME}}.{{VIEW_NAME}}')) AS Definition;
```

## List tables in a schema with their `MS_Description` extended property

This is the **discovery primitive** — every well-documented AdventureWorks table has an `MS_Description` extended property explaining its purpose. Use this when seeding or rebuilding the Top-10 tables list in SKILL.md.

```sql
SELECT
    s.name AS SchemaName,
    t.name AS TableName,
    CAST(ep.value AS nvarchar(2000)) AS Description
FROM sys.tables t
JOIN sys.schemas s ON s.schema_id = t.schema_id
LEFT JOIN sys.extended_properties ep
  ON ep.major_id = t.object_id
 AND ep.minor_id = 0
 AND ep.name = 'MS_Description'
 AND ep.class = 1
WHERE s.name = '{{SCHEMA_NAME}}'
ORDER BY t.name;
```

## Top 10 most-joined tables (FK fan-in + fan-out)

A table that's both heavily referenced and references many others is a high-leverage join target. Use this query to **regenerate the Top-10 tables list in SKILL.md**.

```sql
;WITH FkCounts AS (
    SELECT
        t.object_id,
        SUM(CASE WHEN fk.parent_object_id     = t.object_id THEN 1 ELSE 0 END) AS FkOut,
        SUM(CASE WHEN fk.referenced_object_id = t.object_id THEN 1 ELSE 0 END) AS FkIn
    FROM sys.tables t
    LEFT JOIN sys.foreign_keys fk
      ON fk.parent_object_id = t.object_id OR fk.referenced_object_id = t.object_id
    GROUP BY t.object_id
)
SELECT TOP 10
    s.name AS SchemaName,
    t.name AS TableName,
    fc.FkOut,
    fc.FkIn,
    (fc.FkOut + fc.FkIn) AS Total,
    CAST(ep.value AS nvarchar(2000)) AS Description
FROM FkCounts fc
JOIN sys.tables   t ON t.object_id  = fc.object_id
JOIN sys.schemas  s ON s.schema_id  = t.schema_id
LEFT JOIN sys.extended_properties ep
  ON ep.major_id = t.object_id
 AND ep.minor_id = 0
 AND ep.name = 'MS_Description'
 AND ep.class = 1
ORDER BY Total DESC, t.name;
```

## List all views with their `MS_Description` extended property

Use this to **rebuild the Top-10 views list in SKILL.md**.

```sql
SELECT
    s.name AS SchemaName,
    v.name AS ViewName,
    CAST(ep.value AS nvarchar(2000)) AS Description
FROM sys.views v
JOIN sys.schemas s ON s.schema_id = v.schema_id
LEFT JOIN sys.extended_properties ep
  ON ep.major_id = v.object_id
 AND ep.minor_id = 0
 AND ep.name = 'MS_Description'
 AND ep.class = 1
ORDER BY s.name, v.name;
```
