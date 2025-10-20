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

---

## Sales Orders

Controller: `ReadSalesOrderController`  
Route prefix: `/sales-orders`

| Name                           | Method | Path                  | Expected Status | Assertions                                                                                          |
| ------------------------------ | ------ | --------------------- | --------------- | --------------------------------------------------------------------------------------------------- |
| Sales Order List               | GET    | `/sales-orders`       | 200             | Response is a paginated object with `results`, `pageNumber`, `totalRecords`                         |
| Sales Order Detail             | GET    | `/sales-orders/43659` | 200             | `response.salesOrderId == 43659`; `lineItems` is array; `billToAddress` and `shipToAddress` present |
| Sales Order Detail - Not Found | GET    | `/sales-orders/999999`| 404             | Not found                                                                                           |
| Sales Order Detail - Invalid   | GET    | `/sales-orders/0`     | 400             | Bad request (salesOrderId must be > 0)                                                              |

Response shape for detail: `{ salesOrderId, salesOrderNumber, orderDate, dueDate, shipDate, status, statusDescription, subTotal, taxAmt, freight, totalDue, purchaseOrderNumber, salesPersonName, territoryName, billToAddress: { addressLine1, addressLine2, city, stateProvince, postalCode }, shipToAddress: {...}, lineItems: [{ salesOrderDetailId, productName, orderQty, unitPrice, lineTotal }] }`
