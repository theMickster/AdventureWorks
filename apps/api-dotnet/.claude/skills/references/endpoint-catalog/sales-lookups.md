# Endpoint Catalog — Sales Lookups

Group: `sales-lookups`  
Implemented in: commits `3633027..634d545` (Feature #918)  
Base path: `/api/v1`  
Auth: Bearer token required on all endpoints (`[Authorize]`)

---

## SalesReason

Controller: `ReadSalesReasonController`  
Route prefix: `/sales-reasons`  
By-ID param: `{id:int}`

| Name                          | Method | Path                        | Expected Status | Assertions                                                 |
| ----------------------------- | ------ | --------------------------- | --------------- | ---------------------------------------------------------- |
| SalesReason List              | GET    | `/sales-reasons`            | 200             | Response is non-empty array; each item has `salesReasonId` |
| SalesReason by Id             | GET    | `/sales-reasons/1`          | 200             | `response.salesReasonId == 1`                              |
| SalesReason by Id - Not Found | GET    | `/sales-reasons/2147483647` | 404             | Body: `"Unable to locate the sales reason."`               |
| SalesReason by Id - Invalid   | GET    | `/sales-reasons/-1`         | 400             | Body: `"A valid sales reason id must be specified."`       |

Response model fields: `salesReasonId`, `name`, `reasonType`, `modifiedDate`

---

## Currency

Controller: `ReadCurrencyController`  
Route prefix: `/currencies`  
By-code param: `{code}` (string — NOT int; validated via `IsNullOrWhiteSpace`)

| Name                           | Method | Path              | Expected Status | Assertions                                                                                                  |
| ------------------------------ | ------ | ----------------- | --------------- | ----------------------------------------------------------------------------------------------------------- |
| Currency List                  | GET    | `/currencies`     | 200             | Response is non-empty array; each item has `currencyCode`                                                   |
| Currency by Code (USD)         | GET    | `/currencies/USD` | 200             | `response.currencyCode == "USD"`; `name` and `modifiedDate` present                                         |
| Currency by Code - Not Found   | GET    | `/currencies/ZZZ` | 404             | Body: `"Unable to locate the currency."`                                                                    |
| Currency by Code - Bad Request | GET    | `/currencies/%20` | 400             | Body: `"A valid currency code must be specified."` — whitespace (`%20`) triggers `IsNullOrWhiteSpace` check |

Response model fields: `currencyCode`, `name`, `modifiedDate`

**Note:** The bad-request test uses `%20` (URL-encoded space). ASP.NET Core decodes it to `" "` and the controller's `IsNullOrWhiteSpace(" ")` check returns true → 400.

---

## SpecialOffer

Controller: `ReadSpecialOfferController`  
Route prefix: `/special-offers`  
By-ID param: `{id:int}`  
Special field: `isActive` (computed — `StartDate.Date <= DateTime.Now.Date && EndDate.Date >= DateTime.Now.Date`)

| Name                                | Method | Path                         | Expected Status | Assertions                                                                |
| ----------------------------------- | ------ | ---------------------------- | --------------- | ------------------------------------------------------------------------- |
| SpecialOffer List                   | GET    | `/special-offers`            | 200             | Response is non-empty array; each item has property `isActive`            |
| SpecialOffer by Id - Expired (id=1) | GET    | `/special-offers/1`          | 200             | `response.isActive == false` — id=1 is "No Discount" (EndDate 2014-11-30) |
| SpecialOffer by Id - Not Found      | GET    | `/special-offers/2147483647` | 404             | Body: `"Unable to locate the special offer."`                             |
| SpecialOffer by Id - Invalid        | GET    | `/special-offers/-1`         | 400             | Body: `"A valid special offer id must be specified."`                     |

Response model fields: `specialOfferId`, `description`, `discountPct`, `type`, `category`, `startDate`, `endDate`, `minQty`, `maxQty`, `isActive`, `modifiedDate`

**Note on isActive:** All standard AdventureWorks SpecialOffer data has EndDate in 2011–2014, so every offer will return `isActive: false` in a stock database. The `isActive=true` path is covered by unit tests only (`SpecialOfferEntityToModelProfileTests`). To test `isActive=true` live, update a row's StartDate/EndDate to cover today.

---

## ShipMethod

Controller: `ReadShipMethodController`  
Route prefix: `/ship-methods`  
By-ID param: `{id:int}`  
Note: Entity lives in `Purchasing.ShipMethod` but controller is under Sales (ShipMethodId is FK on `Sales.SalesOrderHeader`)

| Name                         | Method | Path                       | Expected Status | Assertions                                                           |
| ---------------------------- | ------ | -------------------------- | --------------- | -------------------------------------------------------------------- |
| ShipMethod List              | GET    | `/ship-methods`            | 200             | Response is non-empty array; each item has `shipBase` and `shipRate` |
| ShipMethod by Id             | GET    | `/ship-methods/1`          | 200             | `response.shipMethodId == 1`; `shipBase` and `shipRate` present      |
| ShipMethod by Id - Not Found | GET    | `/ship-methods/2147483647` | 404             | Body: `"Unable to locate the ship method."`                          |
| ShipMethod by Id - Invalid   | GET    | `/ship-methods/-1`         | 400             | Body: `"A valid ship method id must be specified."`                  |

Response model fields: `shipMethodId`, `name`, `shipBase`, `shipRate`, `modifiedDate`
