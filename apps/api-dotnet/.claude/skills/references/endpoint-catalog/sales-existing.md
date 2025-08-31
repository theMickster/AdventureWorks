# Endpoint Catalog — Sales (Existing)

Group: `sales-existing`  
These endpoints predate Feature #918. Use for basic reachability smoke tests only — not for full functional testing (Postman covers that).  
Base path: `/api/v1`  
Auth: Bearer token required on all endpoints (`[Authorize]`)

---

## Sales Person

Controller: `ReadSalesPersonController`  
Route prefix: `/salesPersons` (legacy camelCase — predates kebab-case convention)

| Name             | Method | Path            | Expected Status | Assertions                                                                  |
| ---------------- | ------ | --------------- | --------------- | --------------------------------------------------------------------------- |
| SalesPerson List | GET    | `/salesPersons` | 200             | Response is a paginated object with `results`, `pageNumber`, `totalRecords` |

Response shape: `{ results: [...], pageNumber: int, pageSize: int, totalPages: int, totalRecords: int, hasPreviousPage: bool, hasNextPage: bool }`

---

## Sales Territory

Controller: `ReadSalesTerritoryController`  
Route prefix: `/territories`

| Name            | Method | Path             | Expected Status | Assertions                                                         |
| --------------- | ------ | ---------------- | --------------- | ------------------------------------------------------------------ |
| Territory List  | GET    | `/territories`   | 200             | Response is array of 10 items; item id=1 has `name == "Northwest"` |
| Territory by Id | GET    | `/territories/1` | 200             | `response.id == 1`, `response.name == "Northwest"`                 |

Response model fields: `id`, `name`, `group`, `salesYtd`, `salesLastYear`, `costYtd`, `costLastYear`, `modifiedDate`

---

## Stores

Controller: `ReadStoreController`  
Route prefix: `/stores`

| Name       | Method | Path      | Expected Status | Assertions                                            |
| ---------- | ------ | --------- | --------------- | ----------------------------------------------------- |
| Store List | GET    | `/stores` | 200             | Response is paginated object with `results` non-empty |

Response shape: paginated (`results`, `pageNumber`, `pageSize`, `totalPages`, `totalRecords`)
