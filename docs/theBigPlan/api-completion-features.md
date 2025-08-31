# API Completion -- Features & User Stories

**Epic**: #873 (AdventureWorks API - Round 2: API Completion) — successor to closed Epic #552
**Initiative parent**: #559 (AdventureWorks Web Application)
**Date**: 2026-03-20 (planned), 2026-04-27 (created in ADO)
**Scope**: 4 waves of API completion work -- Store Manager, HR Processes, Person Foundation, Lookup Endpoints

## ADO IDs at a glance

| Wave | Feature                          | ADO ID | Stories | Story IDs                               |
| ---- | -------------------------------- | ------ | ------- | --------------------------------------- |
| 1    | Store Contact Management         | #874   | 3       | #875-#877 — **Done 2026-04-27**         |
| 1    | Store Address Management         | #878   | 3       | #879-#881 — **Done 2026-04-29**         |
| 1    | Store Analytics & Insights       | #882   | 3       | #883-#885                               |
| 1    | Sales Person Assignment Tracking | #886   | 3       | #887-#889                               |
| 2    | Employee Department Transfer     | #890   | 1       | #891 (Story 2.2 → existing #751)        |
| 2    | Employee Pay Management          | #892   | 1       | #893 (Story 2.4 → existing #750)        |
| 2    | Department Reporting             | #894   | 3       | #895-#897                               |
| 3    | Person Email Management          | #898   | 4       | #899-#902                               |
| 3    | Person Phone Management          | #903   | 4       | #904-#907                               |
| 3    | Person Directory & Search        | #908   | 2       | #909-#910                               |
| 3    | PersonCreditCard DbContext Fix   | #911   | 1       | #912 — **Done 2026-04-27**              |
| 4    | Production Lookup Endpoints      | #913   | 4       | #914-#917 (Stories 4.1+4.2 → Done #699) |
| 4    | Sales Lookup Endpoints           | #918   | 4       | #919-#922 — **Done 2026-05-07**         |

**Reparented enabler Features** (now under #873, formerly under #561/#562):

- #715 (Sales Database Views & Indexes) — 3 stories: #746-#748
- #716 (HR Additional API Endpoints) — 5 stories: #749-#753
- #722 (HR Database Views for Dashboard and Org Chart) — 2 stories: #771-#772

**Skipped as duplicates:**

- Story 2.2 (Get Employee Department History) → existing **#751** under Feature #716
- Story 2.4 (Get Employee Pay History) → existing **#750** under Feature #716
- Story 4.1 (ProductCategory Lookup) → already Done as **#699** under Feature #526
- Story 4.2 (ProductSubcategory Lookup) → already Done as **#699** under Feature #526

---

## Wave 1: Store Manager Completion

### Feature: Store Contact Management — **Done 2026-04-27**

**Parent**: Epic #873 (closed Epic #552 superseded)
**Status**: Done — Stories #875, #876, #877 completed and merged.
**Description**: Expose CRUD operations for managing contact persons associated with a store. Contacts link a `PersonEntity` to a `StoreEntity` via the `BusinessEntityContactEntity` junction table with a `ContactTypeId` (e.g., Owner, Purchasing Agent). This enables the Angular UI to manage who the points of contact are for each store.

**Technical scope**: New controllers under `Controllers/v1/Stores/`, new commands/queries under `Application/Features/Sales/`, repository methods on `BusinessEntityContactEntity`. All writes require `[Authorize]`.

**Acceptance Criteria**:
See Stories 1.1-1.3 for detailed acceptance criteria.
Key invariants:

- All writes require `[Authorize]` (401 if unauthenticated)
- Duplicate PersonId+ContactTypeId on same store is rejected
- Non-existent store, person, or contact type returns appropriate error

#### Story 1.1: Add a Contact Person to a Store — **Done 2026-04-27** (#875)

**Description**: As an API consumer, I want to add a contact person to a store by posting a PersonId and ContactTypeId, so that the store's points of contact are tracked in the system.

**Acceptance Criteria**:

```gherkin
Scenario: Successfully add a contact to a store
  Given a store with BusinessEntityId 292 exists
  AND a person with BusinessEntityId 1000 exists
  AND a valid ContactTypeId 1 exists
  When POST /api/v1.0/stores/292/contacts is called with {"personId": 1000, "contactTypeId": 1}
  Then a 201 Created response is returned
  AND the response body contains the personId, contactTypeId, person name, and contact type name
  AND the BusinessEntityContactEntity row is persisted with Rowguid and ModifiedDate set

Scenario: Reject POST with non-existent store
  When POST is called with a storeId that does not exist
  Then a 404 Not Found is returned

Scenario: Reject POST with non-existent person
  When POST is called with a PersonId that does not exist
  Then a 400 Bad Request is returned with error code "Rule-01"

Scenario: Reject POST with non-existent contact type
  When POST is called with a ContactTypeId that does not exist
  Then a 400 Bad Request is returned with error code "Rule-02"

Scenario: Reject POST with duplicate person+contactType combination
  When POST is called with a PersonId+ContactTypeId combination that already exists on this store
  Then a 400 Bad Request is returned with error code "Rule-03"

Scenario: Authentication required
  Given the user is not authenticated
  When POST is called
  Then a 401 Unauthorized is returned
```

#### Story 1.2: Update a Store Contact's Type — **Done 2026-04-27** (#876)

**Description**: As an API consumer, I want to change the contact type of an existing store contact, so that contact roles stay current when responsibilities shift. The route includes both PersonId and current ContactTypeId to uniquely identify the junction record (a person may hold multiple roles at one store).

**Acceptance Criteria**:

```gherkin
Scenario: Successfully update a contact's type
  Given store 292 has a contact with PersonId 1000 and ContactTypeId 1
  When PATCH /api/v1.0/stores/292/contacts/1000/1 is called with {"contactTypeId": 2}
  Then a 200 OK response is returned with the updated contact details
  AND the ModifiedDate is updated

Scenario: Reject PATCH with non-existent contact type
  When PATCH is called with a ContactTypeId that does not exist
  Then a 400 Bad Request is returned

Scenario: Reject PATCH for non-existent contact
  When PATCH is called for a PersonId that is not a contact of this store
  Then a 404 Not Found is returned

Scenario: Authentication required
  Given the user is not authenticated
  When PATCH is called
  Then a 401 Unauthorized is returned
```

#### Story 1.3: Remove a Contact from a Store — **Done 2026-04-27** (#877)

**Description**: As an API consumer, I want to remove a contact person from a store, so that outdated contact relationships are cleaned up. The route includes both PersonId and ContactTypeId to uniquely identify the junction record.

**Acceptance Criteria**:

```gherkin
Scenario: Successfully remove a contact
  Given store 292 has a contact with PersonId 1000 and ContactTypeId 1
  When DELETE /api/v1.0/stores/292/contacts/1000/1 is called
  Then a 204 No Content response is returned
  AND the BusinessEntityContactEntity row is deleted

Scenario: Reject removal of non-existent contact
  When DELETE is called for a PersonId that is not a contact of this store
  Then a 404 Not Found is returned

Scenario: Authentication required
  Given the user is not authenticated
  When DELETE is called
  Then a 401 Unauthorized is returned
```

---

### Feature: Store Address Management — **Done 2026-04-29**

**Parent**: Epic #873 (closed Epic #552 superseded)
**Status**: Done — Stories #879, #880, #881 completed and merged.
**Description**: Expose CRUD operations for managing addresses associated with a store. Addresses link an `AddressEntity` to a `StoreEntity` via the `BusinessEntityAddressEntity` junction table with an `AddressTypeId` (e.g., Main Office, Shipping). This enables the Angular UI to display and manage store locations.

**Technical scope**: New controllers under `Controllers/v1/Stores/`, new commands/queries under `Application/Features/Sales/`, repository methods on `BusinessEntityAddressEntity`. All writes require `[Authorize]`.

**Acceptance Criteria**:
See Stories 1.4-1.6 for detailed acceptance criteria.
Key invariants:

- All writes require `[Authorize]` (401 if unauthenticated)
- Duplicate AddressId+AddressTypeId on same store is rejected
- Non-existent store, address, or address type returns appropriate error

#### Story 1.4: Add an Address to a Store — **Done 2026-04-29** (#879)

**Description**: As an API consumer, I want to add an address to a store by posting an AddressId and AddressTypeId, so that store locations are recorded in the system.

**Acceptance Criteria**:

```gherkin
Scenario: Successfully add an address to a store
  Given a store with BusinessEntityId 292 exists
  AND an address with AddressId 500 exists
  AND a valid AddressTypeId 3 exists
  When POST /api/v1.0/stores/292/addresses is called with {"addressId": 500, "addressTypeId": 3}
  Then a 201 Created response is returned
  AND the response body includes the address line, city, state, postal code, and address type name

Scenario: Reject POST with non-existent store
  When POST is called with a storeId that does not exist
  Then a 404 Not Found is returned

Scenario: Reject POST with non-existent address
  When POST is called with an AddressId that does not exist
  Then a 400 Bad Request is returned

Scenario: Reject POST with non-existent address type
  When POST is called with an AddressTypeId that does not exist
  Then a 400 Bad Request is returned

Scenario: Reject POST with duplicate address+type combination
  When POST is called with an AddressId+AddressTypeId that already exists on this store
  Then a 400 Bad Request is returned with a duplicate address error

Scenario: Authentication required
  Given the user is not authenticated
  When POST is called
  Then a 401 Unauthorized is returned
```

#### Story 1.5: Update a Store Address Type — **Done 2026-04-29** (#880)

**Description**: As an API consumer, I want to change the address type of an existing store address, so that address classifications remain accurate.

**Acceptance Criteria**:

```gherkin
Scenario: Successfully update an address type
  Given store 292 has address 500 with AddressTypeId 3
  When PATCH /api/v1.0/stores/292/addresses/500 is called with {"addressTypeId": 5}
  Then a 200 OK response is returned AND the ModifiedDate is updated

Scenario: Reject PATCH with non-existent address type
  When PATCH is called with an AddressTypeId that does not exist
  Then a 400 Bad Request is returned

Scenario: Reject PATCH for non-linked address
  When PATCH is called for an AddressId that is not linked to this store
  Then a 404 Not Found is returned

Scenario: Authentication required
  Given the user is not authenticated
  When PATCH is called
  Then a 401 Unauthorized is returned
```

#### Story 1.6: Remove an Address from a Store — **Done 2026-04-29** (#881)

**Description**: As an API consumer, I want to remove an address from a store, so that obsolete location records are cleaned up.

**Acceptance Criteria**:

```gherkin
Scenario: Successfully remove an address
  Given store 292 has address 500
  When DELETE /api/v1.0/stores/292/addresses/500 is called
  Then a 204 No Content response is returned AND the junction row is deleted

Scenario: Reject removal of non-existent address link
  When DELETE is called for an AddressId not linked to this store
  Then a 404 Not Found is returned

Scenario: Authentication required
  Given the user is not authenticated
  When DELETE is called
  Then a 401 Unauthorized is returned
```

---

### Feature: Store Analytics & Insights

**Parent**: Epic #873 (closed Epic #552 superseded)
**Description**: Provide read-only analytical endpoints for stores that aggregate data from `SalesOrderHeader`, `CustomerEntity`, and the store's XML `Demographics` column. These power the Store Detail page dashboard panels in the Angular UI. No writes -- all queries use `.AsNoTracking()` with projection for performance.

**Technical scope**: New query handlers under `Application/Features/Sales/Queries/`. New DTOs: `StorePerformanceModel`, `StoreDemographicsModel`, `StoreCustomerModel`. May require a new `IStoreAnalyticsRepository` or extension methods on the existing store repository for complex aggregations.

**Acceptance Criteria**:
See Stories 1.7-1.9 for detailed acceptance criteria.
Key invariants:

- All endpoints are read-only and require [Authorize] consistent with every other Store read endpoint
- Stores with no data return zero/null/empty (never 500)
- Non-existent store returns 404

#### Story 1.7: Get Store Performance Summary — **Done 2026-04-30** (#883)

**Description**: As an API consumer, I want to retrieve a store's performance summary (revenue YTD, order count, average order value, customer count), so that the store detail page can display key business metrics.

**Acceptance Criteria**:

```gherkin
Scenario: Return aggregated performance data
  Given store 292 has customers with SalesOrderHeaders in the current calendar year
  When GET /api/v1.0/stores/292/performance is called
  Then a 200 OK is returned with:
    AND revenueYtd is the sum of TotalDue for orders with OrderDate in the current year
    AND orderCount is the count of those orders
    AND averageOrderValue is revenueYtd / orderCount
    AND customerCount is the distinct count of CustomerId values linked to this store

Scenario: Store with no orders returns zero values
  Given store 999 exists but has no associated orders
  When GET /api/v1.0/stores/999/performance is called
  Then a 200 OK is returned with revenueYtd=0, orderCount=0, averageOrderValue=0, customerCount=0

Scenario: Non-existent store returns 404
  When GET /api/v1.0/stores/999999/performance is called
  Then a 404 Not Found is returned
```

#### Story 1.8: Get Store Demographics — **Done 2026-04-30** (#884)

**Description**: As an API consumer, I want to retrieve a store's demographics parsed from the XML column into a typed DTO, so that the store detail page can show demographic insights.

**Acceptance Criteria**:

```gherkin
Scenario: Parse demographics XML into structured response
  Given store 292 has a populated Demographics XML column
  When GET /api/v1.0/stores/292/demographics is called
  Then a 200 OK is returned with a StoreDemographicsModel containing:
    AND annualSales, annualRevenue, yearOpened, specialty, squareFeet
    AND internet (connectivity descriptor: 56kb | ISDN | DSL | T1 | T2 | T3), numberOfEmployees, brands
```

> **Correction (2026-04-30, #884):** the original AC labelled this field `internetSales flag`. The actual `Sales.StoreSurveySchemaCollection` XSD declares `Internet` as a connectivity-class enum (`56kb`, `ISDN`, `DSL`, `T1`, `T2`, `T3`), not a boolean — the model surfaces it as a nullable string.

```gherkin
Scenario: Store with null Demographics returns empty model
  Given store 800 exists with Demographics = NULL
  When GET /api/v1.0/stores/800/demographics is called
  Then a 200 OK is returned with all demographic fields as null

Scenario: Non-existent store returns 404
  When GET /api/v1.0/stores/999999/demographics is called
  Then a 404 Not Found is returned
```

#### Story 1.9: Get Store Customer List — **Done 2026-04-30** (#885)

**Description**: As an API consumer, I want to retrieve a paginated list of customers associated with a store including their lifetime spend, so that the store detail page can show customer activity.

**Acceptance Criteria**:

```gherkin
Scenario: Return paginated customer list with spend data
  Given store 292 has customers with order history
  When GET /api/v1.0/stores/292/customers?pageNumber=1&pageSize=20 is called
  Then a 200 OK is returned with a paginated list
    AND each customer includes customerId, accountNumber, personName, lifetimeSpend, and orderCount
    AND the list is sorted by lifetimeSpend descending by default

Scenario: Pagination parameters are clamped to valid ranges
  When pageNumber=0 or pageSize=0 is provided
  Then pageNumber normalizes to 1 and pageSize to default. (Codebase convention.)

Scenario: Excessive page size is clamped
  When pageSize > 50 is provided
  Then pageSize clamps to 50. (Codebase convention; MaxTake=50 in QueryStringParamsBase.)

Scenario: Store with no customers returns empty list
  Given store 800 exists but has no customers
  When the endpoint is called
  Then a 200 OK is returned with an empty items array and totalCount=0

Scenario: Non-existent store returns 404
  When GET /api/v1.0/stores/999999/customers is called
  Then a 404 Not Found is returned
```

---

### Feature: Sales Person Assignment Tracking

**Parent**: Epic #873 (closed Epic #552 superseded)
**Description**: Track historical sales person assignments for stores. Currently `StoreEntity.SalesPersonId` only records the current assignment with no history. This feature adds a mechanism to reassign a store's sales person while preserving the history of past assignments, enabling reporting on territory and personnel changes over time.

**Technical scope**: This requires a new `StoreSalesPersonHistory` table (DbUp migration), a new entity, repository, commands, and queries. The reassignment command updates `StoreEntity.SalesPersonId` AND inserts a history record. The history query returns all past assignments with date ranges.

**Acceptance Criteria**:
See Stories 1.10-1.12 for detailed acceptance criteria.
Key invariants:

- Reassignment updates `StoreEntity.SalesPersonId` AND creates history records in a single transaction
- Self-assignment (same SalesPersonId) is rejected
- Non-existent store or sales person returns appropriate error
- All writes require `[Authorize]` (401 if unauthenticated)

#### Story 1.10: Create DbUp Migration for StoreSalesPersonHistory

**Description**: As a developer, I want a new `Sales.StoreSalesPersonHistory` table created via DbUp migration, so that sales person assignment changes are tracked over time.

**Acceptance Criteria**:

```gherkin
Scenario: Migration creates the history table
  Given the DbUp migration runner executes
  When the StoreSalesPersonHistory migration runs
  Then the table Sales.StoreSalesPersonHistory exists with columns:
    AND BusinessEntityId (int, FK to Sales.Store)
    AND SalesPersonId (int, FK to Sales.SalesPerson)
    AND StartDate (datetime2, not null)
    AND EndDate (datetime2, nullable)
    AND ModifiedDate (datetime2, not null)
    AND Rowguid (uniqueidentifier, not null, default newid())
    AND a composite primary key on (BusinessEntityId, SalesPersonId, StartDate)
```

#### Story 1.11: Reassign a Store's Sales Person

**Depends on**: Story 1.10 (DbUp migration must be applied first)

**Description**: As an API consumer, I want to reassign a store to a different sales person, so that territory changes are applied and historically tracked.

**Acceptance Criteria**:

```gherkin
Scenario: Successfully reassign sales person
  Given store 292 is currently assigned to SalesPersonId 275
  AND SalesPersonId 276 exists and is active
  When POST /api/v1.0/stores/292/sales-person-assignments is called with {"salesPersonId": 276}
  Then a 201 Created is returned
  AND the store's SalesPersonId is now 276
  AND the previous history record for SalesPersonId 275 has its EndDate set to today
  AND a new history record exists for SalesPersonId 276 with StartDate = today and EndDate = null

Scenario: Reject reassignment with non-existent sales person
  When the salesPersonId does not exist
  Then a 400 Bad Request is returned

Scenario: Reject reassignment to same sales person
  When the salesPersonId is the same as the store's current SalesPersonId
  Then a 400 Bad Request is returned with a "same assignment" error

Scenario: First reassignment creates initial history for previous assignment
  Given store 292 has SalesPersonId 275 but no StoreSalesPersonHistory records exist
  When POST /api/v1.0/stores/292/sales-person-assignments is called with {"salesPersonId": 276}
  Then a history record is created for the outgoing SalesPersonId 275 with StartDate inferred from store's ModifiedDate AND EndDate = today
  AND a history record is created for the incoming SalesPersonId 276 with StartDate = today AND EndDate = null
  AND the store's SalesPersonId is updated to 276

Scenario: Non-existent store returns 404
  When the store does not exist
  Then a 404 Not Found is returned

Scenario: Authentication required
  Given the user is not authenticated
  When POST is called
  Then a 401 Unauthorized is returned
```

#### Story 1.12: Get Sales Person Assignment History

**Depends on**: Story 1.10 (DbUp migration must be applied first)

**Description**: As an API consumer, I want to retrieve the sales person assignment history for a store, so that personnel changes can be audited.

**Acceptance Criteria**:

```gherkin
Scenario: Return assignment history
  Given store 292 has assignment history records
  When GET /api/v1.0/stores/292/sales-person-assignments is called
  Then a 200 OK is returned with a list of assignments
  AND each record includes salesPersonId, salesPersonName, territory, startDate, endDate
  AND the list is ordered by startDate descending

Scenario: Store with no history returns empty list
  Given store 800 has never been reassigned
  When the endpoint is called
  Then a 200 OK is returned with an empty list

Scenario: Non-existent store returns 404
  When the endpoint is called for a store that does not exist
  Then a 404 Not Found is returned
```

---

## Wave 2: HR Process Completion

### Feature: Employee Department Transfer

**Parent**: Epic #873 (closed Epic #552 superseded)
**Description**: Enable transferring an employee to a new department and/or shift. The `EmployeeDepartmentHistory` table tracks all department/shift assignments with `StartDate` and `EndDate`. A transfer closes the current open record (sets `EndDate`) and creates a new record with the target department and shift. The read endpoint exposes the full history for an employee.

**Technical scope**: New commands/queries under `Application/Features/HumanResources/`. Repository operations on `EmployeeDepartmentHistory`. The transfer is a single transaction: close old record + open new record. All writes require `[Authorize]`.

**Acceptance Criteria**:
See Stories 2.1-2.2 for detailed acceptance criteria.
Key invariants:

- Transfer closes current open record (EndDate) AND opens new record in a single transaction
- Inactive employees (CurrentFlag=false) cannot be transferred
- Transfer to same department+shift is rejected
- All writes require `[Authorize]` (401 if unauthenticated)

#### Story 2.1: Transfer Employee to New Department/Shift

**Description**: As an HR administrator, I want to transfer an employee to a different department and/or shift, so that organizational changes are recorded with an audit trail.

**Acceptance Criteria**:

```gherkin
Scenario: Successfully transfer an employee
  Given employee 1 is active with an open department history record for DepartmentId 1, ShiftId 1
  AND DepartmentId 2 exists AND ShiftId 1 exists
  When POST /api/v1.0/employees/1/department-transfers is called with {"departmentId": 2, "shiftId": 1}
  Then a 201 Created is returned
  AND the previous history record has EndDate set to today
  AND a new history record exists with DepartmentId 2, ShiftId 1, StartDate = today, EndDate = null

Scenario: Reject transfer with non-existent department
  When the departmentId does not exist
  Then a 400 Bad Request is returned with error code "Rule-01"

Scenario: Reject transfer with non-existent shift
  When the shiftId does not exist
  Then a 400 Bad Request is returned with error code "Rule-02"

Scenario: Reject transfer for inactive employee
  When the employee is not active (CurrentFlag=false)
  Then a 400 Bad Request is returned with error code "Rule-03"

Scenario: Reject transfer to same department and shift
  When the target departmentId AND shiftId match the employee's current assignment
  Then a 400 Bad Request is returned with error code "Rule-04"

Scenario: Reject transfer when no open department history exists
  Given employee 1 has no EmployeeDepartmentHistory record with EndDate IS NULL
  When POST /api/v1.0/employees/1/department-transfers is called
  Then a 409 Conflict is returned indicating no active department assignment to close

Scenario: Non-existent employee returns 404
  When the employeeId does not exist
  Then a 404 Not Found is returned

Scenario: Authentication required
  Given the user is not authenticated
  When POST is called
  Then a 401 Unauthorized is returned
```

#### Story 2.2: Get Employee Department History

**Description**: As an API consumer, I want to retrieve the full department/shift history for an employee, so that organizational tenure and transfers are visible.

**Acceptance Criteria**:

```gherkin
Scenario: Return department history
  Given employee 1 has multiple department history records
  When GET /api/v1.0/employees/1/department-history is called
  Then a 200 OK is returned with a list of records
  AND each record includes departmentId, departmentName, shiftId, shiftName, startDate, endDate
  AND records are ordered by startDate descending
  AND the current assignment has endDate = null

Scenario: Non-existent employee returns 404
  When the endpoint is called for an employee that does not exist
  Then a 404 Not Found is returned
```

---

### Feature: Employee Pay Management

**Parent**: Epic #873 (closed Epic #552 superseded)
**Description**: Expose endpoints for recording pay rate changes and viewing pay history. The `EmployeePayHistory` table stores rate changes with `RateChangeDate`, `Rate`, and `PayFrequency` (1=Monthly, 2=Biweekly). Recording a new pay change creates a new row; it does not update existing rows. The read endpoint returns the full history for compensation auditing.

**Technical scope**: New commands/queries under `Application/Features/HumanResources/`. Repository operations on `EmployeePayHistory`. New DTOs: `EmployeePayHistoryModel`, `EmployeePayChangeCreateModel`. FluentValidation enforces Rate > 0, Rate <= 500, PayFrequency in {1, 2}.

**Acceptance Criteria**:
See Stories 2.3-2.4 for detailed acceptance criteria.
Key invariants:

- Pay changes create new rows (append-only, never update existing history)
- Rate must be > 0 and <= 500; PayFrequency must be 1 (Monthly) or 2 (Biweekly)
- Inactive employees (CurrentFlag=false) cannot have pay changes recorded
- All writes require `[Authorize]` (401 if unauthenticated)

#### Story 2.3: Record a Pay Rate Change

**Description**: As an HR administrator, I want to record a pay rate change for an employee, so that compensation changes are tracked over time.

**Acceptance Criteria**:

```gherkin
Scenario: Successfully record a pay change
  Given employee 1 is active (CurrentFlag=true)
  When POST /api/v1.0/employees/1/pay-changes is called with {"rate": 50.00, "payFrequency": 1}
  Then a 201 Created is returned
  AND the EmployeePayHistory record is persisted with RateChangeDate = now, Rate = 50.00, PayFrequency = 1

Scenario: Reject pay change with zero or negative rate
  When rate is 0 or negative
  Then a 400 Bad Request is returned with error code "Rule-01"

Scenario: Reject pay change with rate exceeding maximum
  When rate exceeds 500
  Then a 400 Bad Request is returned with error code "Rule-02"

Scenario: Reject pay change with invalid pay frequency
  When payFrequency is not 1 or 2
  Then a 400 Bad Request is returned with error code "Rule-03"

Scenario: Reject pay change for inactive employee
  When the employee is not active (CurrentFlag=false)
  Then a 400 Bad Request is returned with error code "Rule-04"

Scenario: Non-existent employee returns 404
  When the employeeId does not exist
  Then a 404 Not Found is returned

Scenario: Authentication required
  Given the user is not authenticated
  When POST is called
  Then a 401 Unauthorized is returned
```

#### Story 2.4: Get Employee Pay History

**Description**: As an API consumer, I want to retrieve the full pay history for an employee, so that compensation changes are auditable.

**Acceptance Criteria**:

```gherkin
Scenario: Return pay history
  Given employee 1 has multiple pay history records
  When GET /api/v1.0/employees/1/pay-history is called
  Then a 200 OK is returned with a list of records
  AND each record includes rateChangeDate, rate, payFrequency, and payFrequencyDescription ("Monthly" or "Biweekly")
  AND records are ordered by rateChangeDate descending

Scenario: Non-existent employee returns 404
  When the endpoint is called for an employee that does not exist
  Then a 404 Not Found is returned
```

---

### Feature: Department Reporting

**Parent**: Epic #873 (closed Epic #552 superseded)
**Description**: Provide department-level reporting endpoints for headcount and employee listings. These aggregate data from `EmployeeDepartmentHistory` (filtering for `EndDate IS NULL` to get current assignments) and `EmployeeEntity` (filtering for `CurrentFlag=true`). These power the HR dashboard in the Angular UI.

**Technical scope**: New query handlers under `Application/Features/HumanResources/Queries/`. New DTOs: `DepartmentHeadcountModel`, `DepartmentHeadcountSummaryModel`. The employees-by-department query reuses the existing `EmployeeModel` DTO. All read-only, `.AsNoTracking()`.

**Acceptance Criteria**:
See Stories 2.5-2.7 for detailed acceptance criteria.
Key invariants:

- All endpoints are read-only using `.AsNoTracking()`
- Only active employees (CurrentFlag=true) with open department history (EndDate IS NULL) are counted
- Departments with zero employees are included in summaries
- Non-existent department returns 404

#### Story 2.5: Get Department Headcount

**Description**: As an API consumer, I want to retrieve the active employee count for a specific department, so that staffing levels are visible.

**Acceptance Criteria**:

```gherkin
Scenario: Return headcount for a department
  Given department 1 has active employees assigned to it
  When GET /api/v1.0/departments/1/headcount is called
  Then a 200 OK is returned with {"departmentId": 1, "departmentName": "Engineering", "activeEmployeeCount": 15}

Scenario: Department with no employees returns zero
  Given department 99 exists but has no current assignments
  When the endpoint is called
  Then a 200 OK is returned with activeEmployeeCount = 0

Scenario: Non-existent department returns 404
  When the endpoint is called for a department that does not exist
  Then a 404 Not Found is returned
```

#### Story 2.6: Get Department Headcount Summary

**Description**: As an API consumer, I want to retrieve a summary of all departments with their active headcount, so that the HR dashboard can display an org-wide staffing overview.

**Acceptance Criteria**:

```gherkin
Scenario: Return headcount summary across all departments
  When GET /api/v1.0/departments/headcount-summary is called
  Then a 200 OK is returned with a list of all departments
  AND each entry includes departmentId, departmentName, groupName, and activeEmployeeCount
  AND the list is sorted by activeEmployeeCount descending
  AND departments with zero employees are included
```

#### Story 2.7: Get Employees by Department

**Description**: As an API consumer, I want to retrieve a paginated list of active employees in a specific department, so that department rosters can be viewed.

**Acceptance Criteria**:

```gherkin
Scenario: Return paginated employee list for a department
  Given department 1 has active employees
  When GET /api/v1.0/departments/1/employees?page=1&pageSize=20 is called
  Then a 200 OK is returned with a paginated list of active employees
  AND each employee includes businessEntityId, firstName, lastName, jobTitle, hireDate
  AND only employees with CurrentFlag=true and an open department history record (EndDate IS NULL) for this department are included

Scenario: Reject invalid pagination parameters
  When page=0 or pageSize=0 is provided
  Then a 400 Bad Request is returned

Scenario: Reject excessive page size
  When pageSize > 100 is provided
  Then a 400 Bad Request is returned

Scenario: Non-existent department returns 404
  When the endpoint is called for a department that does not exist
  Then a 404 Not Found is returned
```

---

## Wave 3: Person Foundation

### Feature: Person Email Management

**Parent**: Epic #873 (closed Epic #552 superseded)
**Description**: Expose CRUD operations for managing email addresses associated with a person. The `EmailAddressEntity` table supports multiple emails per person with a sequential `EmailAddressId` per `BusinessEntityId`. This is a foundational capability needed by the Person Directory and by store/employee contact management screens.

**Technical scope**: New controllers under `Controllers/v1/Persons/`, new commands/queries under `Application/Features/Person/`. Repository operations on `EmailAddressEntity`. New DTOs: `PersonEmailModel`, `PersonEmailCreateModel`, `PersonEmailUpdateModel`. All writes require `[Authorize]`.

**Acceptance Criteria**:
See Stories 3.1-3.4 for detailed acceptance criteria.
Key invariants:

- All writes require `[Authorize]` (401 if unauthenticated)
- Email format is validated; duplicate emails per person are rejected
- Non-existent person returns 404

#### Story 3.1: List Emails for a Person

**Description**: As an API consumer, I want to list all email addresses for a person, so that contact information is accessible.

**Acceptance Criteria**:

```gherkin
Scenario: Return all emails for a person
  Given person 1000 has 2 email addresses
  When GET /api/v1.0/persons/1000/emails is called
  Then a 200 OK is returned with a list of 2 emails
  AND each email includes emailAddressId, emailAddress, and modifiedDate

Scenario: Person with no emails returns empty list
  Given person 2000 exists but has no emails
  When the endpoint is called
  Then a 200 OK is returned with an empty list

Scenario: Non-existent person returns 404
  When the endpoint is called for a person that does not exist
  Then a 404 Not Found is returned
```

#### Story 3.2: Add Email to a Person

**Description**: As an API consumer, I want to add an email address to a person, so that contact information is expanded.

**Acceptance Criteria**:

```gherkin
Scenario: Successfully add an email
  Given person 1000 exists
  When POST /api/v1.0/persons/1000/emails is called with {"emailAddress": "john@example.com"}
  Then a 201 Created is returned
  AND the response includes the new emailAddressId AND the persisted email address
  AND Rowguid and ModifiedDate are set

Scenario: Reject empty or null email address
  When emailAddress is empty or null
  Then a 400 Bad Request is returned with error code "Rule-01"

Scenario: Reject invalid email format
  When emailAddress is not a valid email format
  Then a 400 Bad Request is returned with error code "Rule-02"

Scenario: Reject duplicate email address
  When the same emailAddress already exists for this person
  Then a 400 Bad Request is returned with error code "Rule-03"

Scenario: Non-existent person returns 404
  When the personId does not exist
  Then a 404 Not Found is returned

Scenario: Authentication required
  Given the user is not authenticated
  When POST is called
  Then a 401 Unauthorized is returned
```

#### Story 3.3: Update an Email

**Description**: As an API consumer, I want to update an existing email address for a person, so that outdated contact info is corrected.

**Acceptance Criteria**:

```gherkin
Scenario: Successfully update an email
  Given person 1000 has an email with emailAddressId 1
  When PUT /api/v1.0/persons/1000/emails/1 is called with {"emailAddress": "newemail@example.com"}
  Then a 200 OK is returned with the updated email AND ModifiedDate is refreshed

Scenario: Reject invalid email format on update
  When the new emailAddress is not a valid format
  Then a 400 Bad Request is returned

Scenario: Reject duplicate email on update
  When the new emailAddress already exists for this person (duplicate)
  Then a 400 Bad Request is returned

Scenario: Reject update for non-existent email
  When the emailAddressId does not belong to this person
  Then a 404 Not Found is returned

Scenario: Reject update for non-existent person
  When the personId does not exist
  Then a 404 Not Found is returned

Scenario: Authentication required
  Given the user is not authenticated
  When PUT is called
  Then a 401 Unauthorized is returned
```

#### Story 3.4: Delete an Email

**Description**: As an API consumer, I want to delete an email address from a person, so that obsolete contact information is removed.

**Acceptance Criteria**:

```gherkin
Scenario: Successfully delete an email
  Given person 1000 has an email with emailAddressId 1
  When DELETE /api/v1.0/persons/1000/emails/1 is called
  Then a 204 No Content is returned AND the row is deleted

Scenario: Non-existent email returns 404
  When the emailAddressId does not belong to this person
  Then a 404 Not Found is returned

Scenario: Authentication required
  Given the user is not authenticated
  When DELETE is called
  Then a 401 Unauthorized is returned
```

---

### Feature: Person Phone Management

**Parent**: Epic #873 (closed Epic #552 superseded)
**Description**: Expose CRUD operations for managing phone numbers associated with a person. The `PersonPhone` table uses a composite key of `BusinessEntityId + PhoneNumber + PhoneNumberTypeId`. A person can have multiple phones but not duplicate phone+type combinations. This is foundational for contact management across the application.

**Technical scope**: New controllers under `Controllers/v1/Persons/`, new commands/queries under `Application/Features/Person/`. Repository operations on `PersonPhone`. The composite key is `(BusinessEntityId, PhoneNumber, PhoneNumberTypeId)`. Updates and deletes use PhoneNumberTypeId as the route parameter. A validation rule enforces one phone number per type per person (prevents ambiguous lookups). New DTOs: `PersonPhoneModel`, `PersonPhoneCreateModel`, `PersonPhoneUpdateModel`. All writes require `[Authorize]`.

**Acceptance Criteria**:
See Stories 3.5-3.8 for detailed acceptance criteria.
Key invariants:

- All writes require `[Authorize]` (401 if unauthenticated)
- Duplicate phone+type combination per person is rejected
- Phone number max length is 25 characters
- Non-existent person or phone type returns appropriate error

#### Story 3.5: List Phones for a Person

**Description**: As an API consumer, I want to list all phone numbers for a person, so that phone contact information is accessible.

**Acceptance Criteria**:

```gherkin
Scenario: Return all phones for a person
  Given person 1000 has 2 phone numbers
  When GET /api/v1.0/persons/1000/phones is called
  Then a 200 OK is returned with a list of 2 phones
  AND each phone includes phoneNumber, phoneNumberTypeId, phoneNumberTypeName, and modifiedDate

Scenario: Non-existent person returns 404
  When the endpoint is called for a person that does not exist
  Then a 404 Not Found is returned
```

#### Story 3.6: Add Phone to a Person

**Description**: As an API consumer, I want to add a phone number to a person, so that phone contact details are recorded.

**Acceptance Criteria**:

```gherkin
Scenario: Successfully add a phone
  Given person 1000 exists AND PhoneNumberTypeId 1 ("Cell") exists
  When POST /api/v1.0/persons/1000/phones is called with {"phoneNumber": "555-0100", "phoneNumberTypeId": 1}
  Then a 201 Created is returned
  AND the response includes phoneNumber, phoneNumberTypeId, and phoneNumberTypeName

Scenario: Reject empty phone number
  When phoneNumber is empty
  Then a 400 Bad Request is returned with error code "Rule-01"

Scenario: Reject phone number exceeding max length
  When phoneNumber exceeds 25 characters
  Then a 400 Bad Request is returned with error code "Rule-02"

Scenario: Reject non-existent phone number type
  When phoneNumberTypeId does not exist
  Then a 400 Bad Request is returned with error code "Rule-03"

Scenario: Reject duplicate phone+type combination
  When the phone+type combination already exists for this person
  Then a 400 Bad Request is returned with error code "Rule-04"

Scenario: Non-existent person returns 404
  When the personId does not exist
  Then a 404 Not Found is returned

Scenario: Authentication required
  Given the user is not authenticated
  When POST is called
  Then a 401 Unauthorized is returned
```

#### Story 3.7: Update a Phone Number

**Description**: As an API consumer, I want to update an existing phone number for a person (identified by phone number type), so that phone records stay current.

**Acceptance Criteria**:

```gherkin
Scenario: Successfully update a phone
  Given person 1000 has a phone with PhoneNumberTypeId 1
  When PUT /api/v1.0/persons/1000/phones/1 is called with {"phoneNumber": "555-0200"}
  Then a 200 OK is returned with the updated phone details AND ModifiedDate is refreshed
  AND the old composite key row is replaced (delete old + insert new, since PhoneNumber is part of the key)

Scenario: Reject invalid phone number on update
  When the phoneNumber is empty or exceeds 25 characters
  Then a 400 Bad Request is returned

Scenario: Reject update for non-existent phone type
  When the PhoneNumberTypeId does not exist for this person
  Then a 404 Not Found is returned

Scenario: Reject update for non-existent person
  When the personId does not exist
  Then a 404 Not Found is returned

Scenario: Authentication required
  Given the user is not authenticated
  When PUT is called
  Then a 401 Unauthorized is returned
```

#### Story 3.8: Delete a Phone Number

**Description**: As an API consumer, I want to delete a phone number from a person (identified by phone number type), so that obsolete phone records are removed.

**Acceptance Criteria**:

```gherkin
Scenario: Successfully delete a phone
  Given person 1000 has a phone with PhoneNumberTypeId 1
  When DELETE /api/v1.0/persons/1000/phones/1 is called
  Then a 204 No Content is returned AND the row is deleted

Scenario: Non-existent phone type returns 404
  When the PhoneNumberTypeId does not match any phone for this person
  Then a 404 Not Found is returned

Scenario: Authentication required
  Given the user is not authenticated
  When DELETE is called
  Then a 401 Unauthorized is returned
```

---

### Feature: Person Directory & Search

**Parent**: Epic #873 (closed Epic #552 superseded)
**Description**: Provide endpoints for searching and viewing persons. The search endpoint supports filtering by name, email, and person type with pagination. The detail endpoint returns a consolidated view of a person including their emails, phones, and person type context (whether they are an employee, store contact, individual customer, etc.). This is foundational for any UI that needs to look up a person by name or email.

**Technical scope**: New query handlers under `Application/Features/Person/Queries/`. New DTOs: `PersonSearchModel`, `PersonSearchResultModel`, `PersonDetailModel`. The search uses `POST /persons/search` (matching the existing search pattern for stores/employees). The detail endpoint uses `.Include()` for emails and phones. All read-only.

**Acceptance Criteria**:
See Stories 3.9-3.10 for detailed acceptance criteria.
Key invariants:

- Search requires at least one filter criterion (no unfiltered full-table scans)
- Person detail includes consolidated emails, phones, and person type context
- Non-existent person returns 404

#### Story 3.9: Search Persons

**Description**: As an API consumer, I want to search for persons by name, email, or person type, so that I can find the right person across the system.

**Acceptance Criteria**:

```gherkin
Scenario: Search with name filter
  When POST /api/v1.0/persons/search is called with {"firstName": "John", "page": 1, "pageSize": 20}
  Then a 200 OK is returned with a paginated list of persons whose first name contains "John"
  AND each result includes businessEntityId, firstName, lastName, personTypeName, and primaryEmail

Scenario: Search with multiple filters
  When POST is called with {"lastName": "Smith", "personTypeCode": "EM", "page": 1, "pageSize": 20}
  Then only persons matching ALL filters are returned

Scenario: Search requires at least one filter
  When POST is called with no filter criteria (only page and pageSize)
  Then a 400 Bad Request is returned with a message requiring at least one search criterion

Scenario: Reject invalid pagination parameters
  When page=0 or pageSize=0 or pageSize > 100
  Then a 400 Bad Request is returned
```

#### Story 3.10: Get Person by ID

**Description**: As an API consumer, I want to retrieve a consolidated view of a person by their BusinessEntityId, so that all contact and type information is available in one call.

**Acceptance Criteria**:

```gherkin
Scenario: Return consolidated person detail
  Given person 1000 exists with emails and phones
  When GET /api/v1.0/persons/1000 is called
  Then a 200 OK is returned with:
    AND businessEntityId, firstName, lastName, middleName, title, suffix
    AND personTypeName (e.g., "Store Contact", "Individual Customer", "Employee")
    AND emailAddresses array with all emails
    AND phoneNumbers array with all phones and their type names
    AND emailPromotion value

Scenario: Non-existent person returns 404
  When GET /api/v1.0/persons/999999 is called
  Then a 404 Not Found is returned
```

---

### Feature: PersonCreditCard DbContext Fix — **Done 2026-04-27**

**Parent**: Epic #873 (closed Epic #552 superseded)
**Status**: Done — Story #912 completed and merged.
**Description**: Bug fix -- the `PersonCreditCard` entity is configured in EF Core model configuration but is missing as a `DbSet<PersonCreditCard>` on `AdventureWorksDbContext`. This prevents direct querying of the junction table. Add the missing DbSet property.

**Technical scope**: Single-line change in `AdventureWorksDbContext.cs`. Verify with a build and confirm no EF model snapshot changes are needed (DbUp handles migrations, not EF migrations).

**Acceptance Criteria**:
See Story 3.11 for detailed acceptance criteria.
Key invariants:

- `DbSet<PersonCreditCard>` is queryable on `AdventureWorksDbContext`
- Existing `OnModelCreating` configuration for PersonCreditCard is unchanged

#### Story 3.11: Add PersonCreditCard DbSet to DbContext — **Done 2026-04-27** (#912)

**Description**: As a developer, I want the `PersonCreditCard` entity registered as a `DbSet` on `AdventureWorksDbContext`, so that the junction table is queryable without workarounds.

**Acceptance Criteria**:

```gherkin
Scenario: DbSet property exists and is functional
  Given the AdventureWorksDbContext class
  When a public DbSet<PersonCreditCard> PersonCreditCards property is added
  Then the project builds without errors
  AND the existing OnModelCreating configuration for PersonCreditCard is unchanged
  AND a unit test confirms the DbSet is resolvable from the context
```

---

## Wave 4: Lookup Endpoint Blitz

### Feature: Production Lookup Endpoints

**Parent**: Epic #873 (closed Epic #552 superseded)
**Description**: Expose read-only GET endpoints for production domain reference data. These are used by dropdowns, filters, and forms throughout the Angular UI. Each entity gets a "get by ID" and "get list" endpoint. All queries use `.AsNoTracking()` and return lightweight DTOs. All controllers use `[Authorize]` consistent with the existing lookup controller pattern.

**Entities**: `ProductCategory`, `ProductSubcategory` (include parent category name), `ProductModel`, `UnitMeasure`, `Location`, `ScrapReason`.

**Technical scope**: New controllers under `Controllers/v1/Production/`, new queries under `Application/Features/Production/Queries/`, new DTOs in `Models/Features/Production/`, new AutoMapper profiles. Follows the exact same pattern as existing lookup endpoints (AddressType, ContactType, etc.). All controllers use `[Authorize]` consistent with the existing lookup controller pattern.

**Acceptance Criteria**:
See Stories 4.1-4.6 for detailed acceptance criteria.
Key invariants:

- Each entity has "get all" and "get by ID" endpoints
- All responses include `modifiedDate` for future HTTP caching support
- Non-existent ID returns 404
- All controllers use `[Authorize]` consistent with existing lookup pattern

#### Story 4.1: ProductCategory Lookup Endpoints

**Description**: As an API consumer, I want to retrieve product categories (single and list), so that category dropdowns and filters are populated.

**Acceptance Criteria**:

```gherkin
Scenario: Get all product categories
  When GET /api/v1.0/product-categories is called
  Then a 200 OK is returned with a list containing productCategoryId, name, and modifiedDate for each category

Scenario: Get single product category
  When GET /api/v1.0/product-categories/1 is called
  Then a 200 OK is returned with productCategoryId, name, modifiedDate, and subcategory count

Scenario: Get non-existent product category returns 404
  When GET /api/v1.0/product-categories/999 is called
  Then a 404 Not Found is returned
```

#### Story 4.2: ProductSubcategory Lookup Endpoints

**Description**: As an API consumer, I want to retrieve product subcategories with their parent category name, so that hierarchical category data is available for navigation and filtering.

**Acceptance Criteria**:

```gherkin
Scenario: Get all product subcategories
  When GET /api/v1.0/product-subcategories is called
  Then a 200 OK is returned with a list containing productSubcategoryId, name, productCategoryId, productCategoryName, and modifiedDate

Scenario: Get single product subcategory
  When GET /api/v1.0/product-subcategories/1 is called
  Then a 200 OK is returned including the parent category name via .Include() and modifiedDate

Scenario: Get non-existent product subcategory returns 404
  When the ID does not exist
  Then a 404 Not Found is returned
```

#### Story 4.3: ProductModel Lookup Endpoints

**Description**: As an API consumer, I want to retrieve product models (single and list), so that model dropdowns and filters are populated.

**Acceptance Criteria**:

```gherkin
Scenario: Get all product models
  When GET /api/v1.0/product-models is called
  Then a 200 OK is returned with a list containing productModelId, name, and modifiedDate

Scenario: Get single product model
  When GET /api/v1.0/product-models/1 is called
  Then a 200 OK is returned with productModelId, name, catalogDescription, and modifiedDate

Scenario: Get non-existent product model returns 404
  When the ID does not exist
  Then a 404 Not Found is returned
```

#### Story 4.4: UnitMeasure Lookup Endpoints

**Description**: As an API consumer, I want to retrieve unit measures (single and list), so that measurement type dropdowns are populated.

**Acceptance Criteria**:

```gherkin
Scenario: Get all unit measures
  When GET /api/v1.0/unit-measures is called
  Then a 200 OK is returned with a list containing unitMeasureCode, name, and modifiedDate

Scenario: Get single unit measure
  When GET /api/v1.0/unit-measures/{code} is called with a valid UnitMeasureCode
  Then a 200 OK is returned with unitMeasureCode, name, and modifiedDate

Scenario: Get non-existent unit measure returns 404
  When the code does not exist
  Then a 404 Not Found is returned
```

#### Story 4.5: Location Lookup Endpoints

**Description**: As an API consumer, I want to retrieve manufacturing locations (single and list), so that location dropdowns are populated for inventory and work order screens.

**Acceptance Criteria**:

```gherkin
Scenario: Get all locations
  When GET /api/v1.0/locations is called
  Then a 200 OK is returned with a list containing locationId, name, costRate, availability, and modifiedDate

Scenario: Get single location
  When GET /api/v1.0/locations/1 is called
  Then a 200 OK is returned with locationId, name, costRate, availability, and modifiedDate

Scenario: Get non-existent location returns 404
  When the ID does not exist
  Then a 404 Not Found is returned
```

#### Story 4.6: ScrapReason Lookup Endpoints

**Description**: As an API consumer, I want to retrieve scrap reasons (single and list), so that work order scrap tracking dropdowns are populated.

**Acceptance Criteria**:

```gherkin
Scenario: Get all scrap reasons
  When GET /api/v1.0/scrap-reasons is called
  Then a 200 OK is returned with a list containing scrapReasonId, name, and modifiedDate

Scenario: Get single scrap reason
  When GET /api/v1.0/scrap-reasons/1 is called
  Then a 200 OK is returned with scrapReasonId, name, and modifiedDate

Scenario: Get non-existent scrap reason returns 404
  When the ID does not exist
  Then a 404 Not Found is returned
```

---

### Feature: Sales Lookup Endpoints ✅ Done 2026-05-07

**Parent**: Epic #873 (closed Epic #552 superseded)
**Description**: Expose read-only GET endpoints for sales domain reference data. These support dropdowns, filters, and display labels across sales-related Angular UI screens. `SpecialOffer` includes a computed active/expired status based on `StartDate`/`EndDate` vs. current date. All queries use `.AsNoTracking()`. All controllers use `[Authorize]` consistent with the existing lookup controller pattern.

**Entities**: `SalesReason`, `Currency`, `SpecialOffer` (with active/expired status), `ShipMethod`.

**Technical scope**: New controllers under `Controllers/v1/Sales/`, new queries under `Application/Features/Sales/Queries/`, new DTOs, new AutoMapper profiles. `ShipMethod` lives in the `Purchasing` schema but is used primarily by Sales (SalesOrderHeader.ShipMethodId) -- controller goes under Sales.

**Acceptance Criteria**:
See Stories 4.7-4.10 for detailed acceptance criteria.
Key invariants:

- Each entity has "get all" and "get by ID" endpoints
- All responses include `modifiedDate` for future HTTP caching support
- `SpecialOffer` includes a computed `isActive` boolean (StartDate <= today AND EndDate >= today)
- Non-existent ID returns 404
- All controllers use `[Authorize]` consistent with existing lookup pattern

#### Story 4.7: SalesReason Lookup Endpoints

**Description**: As an API consumer, I want to retrieve sales reasons (single and list), so that order reason dropdowns are populated.

**Acceptance Criteria**:

```gherkin
Scenario: Get all sales reasons
  When GET /api/v1.0/sales-reasons is called
  Then a 200 OK is returned with a list containing salesReasonId, name, reasonType, and modifiedDate

Scenario: Get single sales reason
  When GET /api/v1.0/sales-reasons/1 is called
  Then a 200 OK is returned with salesReasonId, name, reasonType, and modifiedDate

Scenario: Get non-existent sales reason returns 404
  When the ID does not exist
  Then a 404 Not Found is returned
```

#### Story 4.8: Currency Lookup Endpoints

**Description**: As an API consumer, I want to retrieve currencies (single and list), so that currency selection dropdowns are populated.

**Acceptance Criteria**:

```gherkin
Scenario: Get all currencies
  When GET /api/v1.0/currencies is called
  Then a 200 OK is returned with a list containing currencyCode, name, and modifiedDate

Scenario: Get single currency
  When GET /api/v1.0/currencies/{code} is called with a valid CurrencyCode (e.g., "USD")
  Then a 200 OK is returned with currencyCode, name, and modifiedDate

Scenario: Get non-existent currency returns 404
  When the code does not exist
  Then a 404 Not Found is returned
```

#### Story 4.9: SpecialOffer Lookup Endpoints

**Description**: As an API consumer, I want to retrieve special offers with their active/expired status, so that applicable promotions are identifiable.

**Acceptance Criteria**:

```gherkin
Scenario: Get all special offers with status
  When GET /api/v1.0/special-offers is called
  Then a 200 OK is returned with a list containing specialOfferId, description, discountPct, type, category, startDate, endDate, minQty, maxQty, isActive, and modifiedDate
  AND isActive is true when StartDate <= today AND EndDate >= today, otherwise false

Scenario: Get single special offer
  When GET /api/v1.0/special-offers/1 is called
  Then a 200 OK is returned with the full special offer details including isActive and modifiedDate

Scenario: Get non-existent special offer returns 404
  When the ID does not exist
  Then a 404 Not Found is returned
```

#### Story 4.10: ShipMethod Lookup Endpoints

**Description**: As an API consumer, I want to retrieve ship methods (single and list), so that shipping option dropdowns are populated for order management.

**Acceptance Criteria**:

```gherkin
Scenario: Get all ship methods
  When GET /api/v1.0/ship-methods is called
  Then a 200 OK is returned with a list containing shipMethodId, name, shipBase, shipRate, and modifiedDate

Scenario: Get single ship method
  When GET /api/v1.0/ship-methods/1 is called
  Then a 200 OK is returned with shipMethodId, name, shipBase, shipRate, and modifiedDate

Scenario: Get non-existent ship method returns 404
  When the ID does not exist
  Then a 404 Not Found is returned
```

---

## Summary

| Wave                            | Feature                          | ADO  | Stories (created)              | Write Endpoints                   | Read Endpoints                    |
| ------------------------------- | -------------------------------- | ---- | ------------------------------ | --------------------------------- | --------------------------------- |
| 1                               | Store Contact Management         | #874 | 3 (#875-#877)                  | POST, PATCH, DELETE               | (via existing #691)               |
| 1                               | Store Address Management         | #878 | 3 (#879-#881)                  | POST, PATCH, DELETE               | (via existing #690)               |
| 1                               | Store Analytics & Insights       | #882 | 3 (#883-#885)                  | --                                | GET x3                            |
| 1                               | Sales Person Assignment Tracking | #886 | 3 (#887-#889)                  | POST                              | GET + DbUp migration (#887)       |
| 2                               | Employee Department Transfer     | #890 | 1 (#891) [2.2 → #751]          | POST                              | GET via #751                      |
| 2                               | Employee Pay Management          | #892 | 1 (#893) [2.4 → #750]          | POST                              | GET via #750                      |
| 2                               | Department Reporting             | #894 | 3 (#895-#897)                  | --                                | GET x3                            |
| 3                               | Person Email Management          | #898 | 4 (#899-#902)                  | POST, PUT, DELETE                 | GET                               |
| 3                               | Person Phone Management          | #903 | 4 (#904-#907)                  | POST, PUT, DELETE                 | GET                               |
| 3                               | Person Directory & Search        | #908 | 2 (#909-#910)                  | --                                | POST search, GET                  |
| 3                               | PersonCreditCard DbContext Fix   | #911 | 1 (#912)                       | --                                | Bug fix                           |
| 4                               | Production Lookup Endpoints      | #913 | 4 (#914-#917) [4.1+4.2 → #699] | --                                | GET x8 (4 entities × 2 endpoints) |
| 4                               | Sales Lookup Endpoints ✅        | #918 | 4 (#919-#922)                  | --                                | GET x8 — Done 2026-05-07         |
| **Total net-new**               |                                  |      | **13 Features / 36 Stories**   |                                   |                                   |
| **Reparented**                  | #715, #716, #722                 | (3)  | **+10 Stories**                | (DB views/indexes + HR endpoints) |
| **Grand total under Epic #873** |                                  |      | **16 Features / 46 Stories**   |                                   |                                   |

### Dependency Notes

- **Wave 1** Store Contact/Address Management depends on existing #690/#691 for the read endpoints (those stories provide GET, these provide POST/PATCH/DELETE).
- **Wave 1** Sales Person Assignment Tracking requires a DbUp migration (Story 1.10) before the API stories can be implemented.
- **Wave 3** Person Directory depends on Person Email and Person Phone features being complete (the detail endpoint includes emails and phones).
- **Wave 4** has no dependencies and can be parallelized across developers -- each story is fully independent.
- **Wave 4** lookup stories follow the identical pattern as existing lookups (AddressType, ContactType, etc.) and are ideal for junior developer onboarding.
