---
applyTo: "apps/functions-rust/**/*"
---

Read `apps/functions-rust/.claude/CLAUDE.md` first. Rules the compiler and tests will not enforce:

- Keep `domain/` free of I/O; reach infrastructure only through the traits in `ports.rs`.
- `cargo fmt --check` and `cargo clippy --locked --all-targets --all-features -- -D warnings` must pass. No `unwrap` or `expect` on runtime paths.
- SQL is read-only and parameterized. Verify table and column names with the `querying-adventureworks-database` skill before adding a query.
- Do not add `bb8-tiberius` (it pulls in `tiberius` 0.12; the crate uses 0.13).
- Never cache what-if results. Cache invalidation is TTL-only unless the team decides otherwise.
- A batch message with any failed pair must return non-2xx so the host retries and dead-letters it.
- No secrets in code or committed config. Only `.example` files are committed.
- Postman sends `Cache-Control: no-cache` by default, so cache-sensitive requests must set their own header.
