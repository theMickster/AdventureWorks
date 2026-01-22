---
paths:
  - "apps/angular-web/libs/purchasing/**"
---

# Purchasing Library Rules

Rules the specs and code comments in `libs/purchasing/**` do not already enforce. Cross-domain list, store, and testing rules are in `apps/angular-web/.claude/CLAUDE.md`.

- Keep `VendorStore` list-only (`loadPage`). Vendor detail, purchase order history, purchase order detail, and purchasing analytics call `PurchasingApiService` directly and own their own loading and error signals.
- Keep the vendor Status badge map and the Risk badge map separate. `badge-success` means "preferred" in one column and "low risk" in the other, so one shared map would misread.
