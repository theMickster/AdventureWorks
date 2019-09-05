-- <Migration ID="8b1dbfb2-5aa2-4f8c-b0c4-4dbdad4368c8" />
GO
/****************************************************************************************************************
** CREATED BY:   Mick Letofsky
** CREATED DATE: 2019.05.08
** CREATED FOR:  PBI 437
** CREATED:      Baseline code for AdventureWorks SQL Change Automation Project.
****************************************************************************************************************/

IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetAccountingEndDate', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Scalar function used in the uSalesOrderHeader trigger to set the starting account date.', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetAccountingEndDate', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetAccountingStartDate', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Scalar function used in the uSalesOrderHeader trigger to set the ending account date.', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetAccountingStartDate', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetContactInformation', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Table value function returning the first name, last name, job title and contact type for a given contact.', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetContactInformation', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetContactInformation', 'PARAMETER', N'@PersonID'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the table value function ufnGetContactInformation. Enter a valid PersonID from the Person.Contact table.', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetContactInformation', 'PARAMETER', N'@PersonID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetDocumentStatusText', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Scalar function returning the text representation of the Status column in the Document table.', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetDocumentStatusText', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetDocumentStatusText', 'PARAMETER', N'@Status'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the scalar function ufnGetDocumentStatusText. Enter a valid integer.', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetDocumentStatusText', 'PARAMETER', N'@Status'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetProductDealerPrice', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Scalar function returning the dealer price for a given product on a particular order date.', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetProductDealerPrice', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetProductDealerPrice', 'PARAMETER', N'@OrderDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the scalar function ufnGetProductDealerPrice. Enter a valid order date.', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetProductDealerPrice', 'PARAMETER', N'@OrderDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetProductDealerPrice', 'PARAMETER', N'@ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the scalar function ufnGetProductDealerPrice. Enter a valid ProductID from the Production.Product table.', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetProductDealerPrice', 'PARAMETER', N'@ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetProductListPrice', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Scalar function returning the list price for a given product on a particular order date.', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetProductListPrice', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetProductListPrice', 'PARAMETER', N'@OrderDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the scalar function ufnGetProductListPrice. Enter a valid order date.', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetProductListPrice', 'PARAMETER', N'@OrderDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetProductListPrice', 'PARAMETER', N'@ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the scalar function ufnGetProductListPrice. Enter a valid ProductID from the Production.Product table.', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetProductListPrice', 'PARAMETER', N'@ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetProductStandardCost', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Scalar function returning the standard cost for a given product on a particular order date.', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetProductStandardCost', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetProductStandardCost', 'PARAMETER', N'@OrderDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the scalar function ufnGetProductStandardCost. Enter a valid order date.', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetProductStandardCost', 'PARAMETER', N'@OrderDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetProductStandardCost', 'PARAMETER', N'@ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the scalar function ufnGetProductStandardCost. Enter a valid ProductID from the Production.Product table.', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetProductStandardCost', 'PARAMETER', N'@ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetPurchaseOrderStatusText', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Scalar function returning the text representation of the Status column in the PurchaseOrderHeader table.', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetPurchaseOrderStatusText', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetPurchaseOrderStatusText', 'PARAMETER', N'@Status'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the scalar function ufnGetPurchaseOrdertStatusText. Enter a valid integer.', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetPurchaseOrderStatusText', 'PARAMETER', N'@Status'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetSalesOrderStatusText', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Scalar function returning the text representation of the Status column in the SalesOrderHeader table.', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetSalesOrderStatusText', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetSalesOrderStatusText', 'PARAMETER', N'@Status'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the scalar function ufnGetSalesOrderStatusText. Enter a valid integer.', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetSalesOrderStatusText', 'PARAMETER', N'@Status'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetStock', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Scalar function returning the quantity of inventory in LocationID 6 (Miscellaneous Storage)for a specified ProductID.', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetStock', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetStock', 'PARAMETER', N'@ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the scalar function ufnGetStock. Enter a valid ProductID from the Production.ProductInventory table.', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetStock', 'PARAMETER', N'@ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnLeadingZeros', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Scalar function used by the Sales.Customer table to help set the account number.', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnLeadingZeros', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnLeadingZeros', 'PARAMETER', N'@Value'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the scalar function ufnLeadingZeros. Enter a valid integer.', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnLeadingZeros', 'PARAMETER', N'@Value'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Department', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Lookup table containing the departments within the Adventure Works Cycles company.', 'SCHEMA', N'HumanResources', 'TABLE', N'Department', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Department', 'COLUMN', N'DepartmentID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for Department records.', 'SCHEMA', N'HumanResources', 'TABLE', N'Department', 'COLUMN', N'DepartmentID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Department', 'COLUMN', N'GroupName'))
EXEC sp_addextendedproperty N'MS_Description', N'Name of the group to which the department belongs.', 'SCHEMA', N'HumanResources', 'TABLE', N'Department', 'COLUMN', N'GroupName'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Department', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'HumanResources', 'TABLE', N'Department', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Department', 'COLUMN', N'Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Name of the department.', 'SCHEMA', N'HumanResources', 'TABLE', N'Department', 'COLUMN', N'Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Department', 'CONSTRAINT', N'DF_Department_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'HumanResources', 'TABLE', N'Department', 'CONSTRAINT', N'DF_Department_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Department', 'CONSTRAINT', N'PK_Department_DepartmentID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'HumanResources', 'TABLE', N'Department', 'CONSTRAINT', N'PK_Department_DepartmentID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Department', 'INDEX', N'AK_Department_Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'HumanResources', 'TABLE', N'Department', 'INDEX', N'AK_Department_Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Department', 'INDEX', N'PK_Department_DepartmentID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'HumanResources', 'TABLE', N'Department', 'INDEX', N'PK_Department_DepartmentID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Employee department transfers.', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'COLUMN', N'BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Employee identification number. Foreign key to Employee.BusinessEntityID.', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'COLUMN', N'BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'COLUMN', N'DepartmentID'))
EXEC sp_addextendedproperty N'MS_Description', N'Department in which the employee worked including currently. Foreign key to Department.DepartmentID.', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'COLUMN', N'DepartmentID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'COLUMN', N'EndDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date the employee left the department. NULL = Current department.', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'COLUMN', N'EndDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'COLUMN', N'ShiftID'))
EXEC sp_addextendedproperty N'MS_Description', N'Identifies which 8-hour shift the employee works. Foreign key to Shift.Shift.ID.', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'COLUMN', N'ShiftID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'COLUMN', N'StartDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date the employee started work in the department.', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'COLUMN', N'StartDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'CONSTRAINT', N'CK_EmployeeDepartmentHistory_EndDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [EndDate] >= [StartDate] OR [EndDate] IS NUL', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'CONSTRAINT', N'CK_EmployeeDepartmentHistory_EndDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'CONSTRAINT', N'DF_EmployeeDepartmentHistory_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'CONSTRAINT', N'DF_EmployeeDepartmentHistory_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'CONSTRAINT', N'FK_EmployeeDepartmentHistory_Department_DepartmentID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Department.DepartmentID.', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'CONSTRAINT', N'FK_EmployeeDepartmentHistory_Department_DepartmentID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'CONSTRAINT', N'FK_EmployeeDepartmentHistory_Employee_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Employee.EmployeeID.', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'CONSTRAINT', N'FK_EmployeeDepartmentHistory_Employee_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'CONSTRAINT', N'FK_EmployeeDepartmentHistory_Shift_ShiftID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Shift.ShiftID', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'CONSTRAINT', N'FK_EmployeeDepartmentHistory_Shift_ShiftID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'CONSTRAINT', N'PK_EmployeeDepartmentHistory_BusinessEntityID_StartDate_DepartmentID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'CONSTRAINT', N'PK_EmployeeDepartmentHistory_BusinessEntityID_StartDate_DepartmentID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'INDEX', N'IX_EmployeeDepartmentHistory_DepartmentID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'INDEX', N'IX_EmployeeDepartmentHistory_DepartmentID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'INDEX', N'IX_EmployeeDepartmentHistory_ShiftID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'INDEX', N'IX_EmployeeDepartmentHistory_ShiftID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'INDEX', N'PK_EmployeeDepartmentHistory_BusinessEntityID_StartDate_DepartmentID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeeDepartmentHistory', 'INDEX', N'PK_EmployeeDepartmentHistory_BusinessEntityID_StartDate_DepartmentID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeePayHistory', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Employee pay history.', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeePayHistory', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeePayHistory', 'COLUMN', N'BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Employee identification number. Foreign key to Employee.BusinessEntityID.', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeePayHistory', 'COLUMN', N'BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeePayHistory', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeePayHistory', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeePayHistory', 'COLUMN', N'PayFrequency'))
EXEC sp_addextendedproperty N'MS_Description', N'1 = Salary received monthly, 2 = Salary received biweekly', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeePayHistory', 'COLUMN', N'PayFrequency'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeePayHistory', 'COLUMN', N'Rate'))
EXEC sp_addextendedproperty N'MS_Description', N'Salary hourly rate.', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeePayHistory', 'COLUMN', N'Rate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeePayHistory', 'COLUMN', N'RateChangeDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date the change in pay is effective', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeePayHistory', 'COLUMN', N'RateChangeDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeePayHistory', 'CONSTRAINT', N'CK_EmployeePayHistory_PayFrequency'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [PayFrequency]=(3) OR [PayFrequency]=(2) OR [PayFrequency]=(1)', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeePayHistory', 'CONSTRAINT', N'CK_EmployeePayHistory_PayFrequency'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeePayHistory', 'CONSTRAINT', N'CK_EmployeePayHistory_Rate'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [Rate] >= (6.50) AND [Rate] <= (200.00)', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeePayHistory', 'CONSTRAINT', N'CK_EmployeePayHistory_Rate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeePayHistory', 'CONSTRAINT', N'DF_EmployeePayHistory_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeePayHistory', 'CONSTRAINT', N'DF_EmployeePayHistory_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeePayHistory', 'CONSTRAINT', N'FK_EmployeePayHistory_Employee_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Employee.EmployeeID.', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeePayHistory', 'CONSTRAINT', N'FK_EmployeePayHistory_Employee_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeePayHistory', 'CONSTRAINT', N'PK_EmployeePayHistory_BusinessEntityID_RateChangeDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeePayHistory', 'CONSTRAINT', N'PK_EmployeePayHistory_BusinessEntityID_RateChangeDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeePayHistory', 'INDEX', N'PK_EmployeePayHistory_BusinessEntityID_RateChangeDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'HumanResources', 'TABLE', N'EmployeePayHistory', 'INDEX', N'PK_EmployeePayHistory_BusinessEntityID_RateChangeDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Employee information such as salary, department, and title.', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'BirthDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date of birth.', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'BirthDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for Employee records.  Foreign key to BusinessEntity.BusinessEntityID.', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'CurrentFlag'))
EXEC sp_addextendedproperty N'MS_Description', N'0 = Inactive, 1 = Active', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'CurrentFlag'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'Gender'))
EXEC sp_addextendedproperty N'MS_Description', N'M = Male, F = Female', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'Gender'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'HireDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Employee hired on this date.', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'HireDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'JobTitle'))
EXEC sp_addextendedproperty N'MS_Description', N'Work title such as Buyer or Sales Representative.', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'JobTitle'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'LoginID'))
EXEC sp_addextendedproperty N'MS_Description', N'Network login.', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'LoginID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'MaritalStatus'))
EXEC sp_addextendedproperty N'MS_Description', N'M = Married, S = Single', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'MaritalStatus'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'NationalIDNumber'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique national identification number such as a social security number.', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'NationalIDNumber'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'OrganizationLevel'))
EXEC sp_addextendedproperty N'MS_Description', N'The depth of the employee in the corporate hierarchy.', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'OrganizationLevel'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'OrganizationNode'))
EXEC sp_addextendedproperty N'MS_Description', N'Where the employee is located in corporate hierarchy.', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'OrganizationNode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'SalariedFlag'))
EXEC sp_addextendedproperty N'MS_Description', N'Job classification. 0 = Hourly, not exempt from collective bargaining. 1 = Salaried, exempt from collective bargaining.', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'SalariedFlag'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'SickLeaveHours'))
EXEC sp_addextendedproperty N'MS_Description', N'Number of available sick leave hours.', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'SickLeaveHours'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'VacationHours'))
EXEC sp_addextendedproperty N'MS_Description', N'Number of available vacation hours.', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'COLUMN', N'VacationHours'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'CONSTRAINT', N'CK_Employee_BirthDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [BirthDate] >= ''1930-01-01'' AND [BirthDate] <= dateadd(year,(-18),GETDATE())', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'CONSTRAINT', N'CK_Employee_BirthDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'CONSTRAINT', N'CK_Employee_Gender'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [Gender]=''f'' OR [Gender]=''m'' OR [Gender]=''F'' OR [Gender]=''M''', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'CONSTRAINT', N'CK_Employee_Gender'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'CONSTRAINT', N'CK_Employee_HireDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [HireDate] >= ''1996-07-01'' AND [HireDate] <= dateadd(day,(1),GETDATE())', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'CONSTRAINT', N'CK_Employee_HireDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'CONSTRAINT', N'CK_Employee_MaritalStatus'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [MaritalStatus]=''s'' OR [MaritalStatus]=''m'' OR [MaritalStatus]=''S'' OR [MaritalStatus]=''M''', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'CONSTRAINT', N'CK_Employee_MaritalStatus'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'CONSTRAINT', N'CK_Employee_SickLeaveHours'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [SickLeaveHours] >= (0) AND [SickLeaveHours] <= (120)', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'CONSTRAINT', N'CK_Employee_SickLeaveHours'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'CONSTRAINT', N'CK_Employee_VacationHours'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [VacationHours] >= (-40) AND [VacationHours] <= (240)', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'CONSTRAINT', N'CK_Employee_VacationHours'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'CONSTRAINT', N'DF_Employee_CurrentFlag'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 1', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'CONSTRAINT', N'DF_Employee_CurrentFlag'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'CONSTRAINT', N'DF_Employee_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'CONSTRAINT', N'DF_Employee_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'CONSTRAINT', N'DF_Employee_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'CONSTRAINT', N'DF_Employee_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'CONSTRAINT', N'DF_Employee_SalariedFlag'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 1 (TRUE)', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'CONSTRAINT', N'DF_Employee_SalariedFlag'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'CONSTRAINT', N'DF_Employee_SickLeaveHours'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'CONSTRAINT', N'DF_Employee_SickLeaveHours'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'CONSTRAINT', N'DF_Employee_VacationHours'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'CONSTRAINT', N'DF_Employee_VacationHours'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'CONSTRAINT', N'FK_Employee_Person_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Person.BusinessEntityID.', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'CONSTRAINT', N'FK_Employee_Person_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'CONSTRAINT', N'PK_Employee_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'CONSTRAINT', N'PK_Employee_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'INDEX', N'AK_Employee_LoginID'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'INDEX', N'AK_Employee_LoginID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'INDEX', N'AK_Employee_NationalIDNumber'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'INDEX', N'AK_Employee_NationalIDNumber'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'INDEX', N'AK_Employee_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index. Used to support replication samples.', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'INDEX', N'AK_Employee_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'INDEX', N'IX_Employee_OrganizationLevel_OrganizationNode'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'INDEX', N'IX_Employee_OrganizationLevel_OrganizationNode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'INDEX', N'IX_Employee_OrganizationNode'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'INDEX', N'IX_Employee_OrganizationNode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'INDEX', N'PK_Employee_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'INDEX', N'PK_Employee_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'TRIGGER', N'dEmployee'))
EXEC sp_addextendedproperty N'MS_Description', N'INSTEAD OF DELETE trigger which keeps Employees from being deleted.', 'SCHEMA', N'HumanResources', 'TABLE', N'Employee', 'TRIGGER', N'dEmployee'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'JobCandidate', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Résumés submitted to Human Resources by job applicants.', 'SCHEMA', N'HumanResources', 'TABLE', N'JobCandidate', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'JobCandidate', 'COLUMN', N'BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Employee identification number if applicant was hired. Foreign key to Employee.BusinessEntityID.', 'SCHEMA', N'HumanResources', 'TABLE', N'JobCandidate', 'COLUMN', N'BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'JobCandidate', 'COLUMN', N'JobCandidateID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for JobCandidate records.', 'SCHEMA', N'HumanResources', 'TABLE', N'JobCandidate', 'COLUMN', N'JobCandidateID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'JobCandidate', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'HumanResources', 'TABLE', N'JobCandidate', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'JobCandidate', 'COLUMN', N'Resume'))
EXEC sp_addextendedproperty N'MS_Description', N'Résumé in XML format.', 'SCHEMA', N'HumanResources', 'TABLE', N'JobCandidate', 'COLUMN', N'Resume'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'JobCandidate', 'CONSTRAINT', N'DF_JobCandidate_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'HumanResources', 'TABLE', N'JobCandidate', 'CONSTRAINT', N'DF_JobCandidate_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'JobCandidate', 'CONSTRAINT', N'FK_JobCandidate_Employee_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Employee.EmployeeID.', 'SCHEMA', N'HumanResources', 'TABLE', N'JobCandidate', 'CONSTRAINT', N'FK_JobCandidate_Employee_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'JobCandidate', 'CONSTRAINT', N'PK_JobCandidate_JobCandidateID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'HumanResources', 'TABLE', N'JobCandidate', 'CONSTRAINT', N'PK_JobCandidate_JobCandidateID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'JobCandidate', 'INDEX', N'IX_JobCandidate_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'HumanResources', 'TABLE', N'JobCandidate', 'INDEX', N'IX_JobCandidate_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'JobCandidate', 'INDEX', N'PK_JobCandidate_JobCandidateID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'HumanResources', 'TABLE', N'JobCandidate', 'INDEX', N'PK_JobCandidate_JobCandidateID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Shift', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Work shift lookup table.', 'SCHEMA', N'HumanResources', 'TABLE', N'Shift', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Shift', 'COLUMN', N'EndTime'))
EXEC sp_addextendedproperty N'MS_Description', N'Shift end time.', 'SCHEMA', N'HumanResources', 'TABLE', N'Shift', 'COLUMN', N'EndTime'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Shift', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'HumanResources', 'TABLE', N'Shift', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Shift', 'COLUMN', N'Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Shift description.', 'SCHEMA', N'HumanResources', 'TABLE', N'Shift', 'COLUMN', N'Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Shift', 'COLUMN', N'ShiftID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for Shift records.', 'SCHEMA', N'HumanResources', 'TABLE', N'Shift', 'COLUMN', N'ShiftID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Shift', 'COLUMN', N'StartTime'))
EXEC sp_addextendedproperty N'MS_Description', N'Shift start time.', 'SCHEMA', N'HumanResources', 'TABLE', N'Shift', 'COLUMN', N'StartTime'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Shift', 'CONSTRAINT', N'DF_Shift_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'HumanResources', 'TABLE', N'Shift', 'CONSTRAINT', N'DF_Shift_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Shift', 'CONSTRAINT', N'PK_Shift_ShiftID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'HumanResources', 'TABLE', N'Shift', 'CONSTRAINT', N'PK_Shift_ShiftID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Shift', 'INDEX', N'AK_Shift_Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'HumanResources', 'TABLE', N'Shift', 'INDEX', N'AK_Shift_Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Shift', 'INDEX', N'AK_Shift_StartTime_EndTime'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'HumanResources', 'TABLE', N'Shift', 'INDEX', N'AK_Shift_StartTime_EndTime'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'TABLE', N'Shift', 'INDEX', N'PK_Shift_ShiftID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'HumanResources', 'TABLE', N'Shift', 'INDEX', N'PK_Shift_ShiftID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'AddressType', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Types of addresses stored in the Address table. ', 'SCHEMA', N'Person', 'TABLE', N'AddressType', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'AddressType', 'COLUMN', N'AddressTypeID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for AddressType records.', 'SCHEMA', N'Person', 'TABLE', N'AddressType', 'COLUMN', N'AddressTypeID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'AddressType', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Person', 'TABLE', N'AddressType', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'AddressType', 'COLUMN', N'Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Address type description. For example, Billing, Home, or Shipping.', 'SCHEMA', N'Person', 'TABLE', N'AddressType', 'COLUMN', N'Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'AddressType', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', 'SCHEMA', N'Person', 'TABLE', N'AddressType', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'AddressType', 'CONSTRAINT', N'DF_AddressType_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Person', 'TABLE', N'AddressType', 'CONSTRAINT', N'DF_AddressType_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'AddressType', 'CONSTRAINT', N'DF_AddressType_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()', 'SCHEMA', N'Person', 'TABLE', N'AddressType', 'CONSTRAINT', N'DF_AddressType_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'AddressType', 'CONSTRAINT', N'PK_AddressType_AddressTypeID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Person', 'TABLE', N'AddressType', 'CONSTRAINT', N'PK_AddressType_AddressTypeID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'AddressType', 'INDEX', N'AK_AddressType_Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'Person', 'TABLE', N'AddressType', 'INDEX', N'AK_AddressType_Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'AddressType', 'INDEX', N'AK_AddressType_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index. Used to support replication samples.', 'SCHEMA', N'Person', 'TABLE', N'AddressType', 'INDEX', N'AK_AddressType_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'AddressType', 'INDEX', N'PK_AddressType_AddressTypeID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Person', 'TABLE', N'AddressType', 'INDEX', N'PK_AddressType_AddressTypeID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Address', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Street address information for customers, employees, and vendors.', 'SCHEMA', N'Person', 'TABLE', N'Address', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Address', 'COLUMN', N'AddressID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for Address records.', 'SCHEMA', N'Person', 'TABLE', N'Address', 'COLUMN', N'AddressID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Address', 'COLUMN', N'AddressLine1'))
EXEC sp_addextendedproperty N'MS_Description', N'First street address line.', 'SCHEMA', N'Person', 'TABLE', N'Address', 'COLUMN', N'AddressLine1'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Address', 'COLUMN', N'AddressLine2'))
EXEC sp_addextendedproperty N'MS_Description', N'Second street address line.', 'SCHEMA', N'Person', 'TABLE', N'Address', 'COLUMN', N'AddressLine2'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Address', 'COLUMN', N'City'))
EXEC sp_addextendedproperty N'MS_Description', N'Name of the city.', 'SCHEMA', N'Person', 'TABLE', N'Address', 'COLUMN', N'City'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Address', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Person', 'TABLE', N'Address', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Address', 'COLUMN', N'PostalCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Postal code for the street address.', 'SCHEMA', N'Person', 'TABLE', N'Address', 'COLUMN', N'PostalCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Address', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', 'SCHEMA', N'Person', 'TABLE', N'Address', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Address', 'COLUMN', N'SpatialLocation'))
EXEC sp_addextendedproperty N'MS_Description', N'Latitude and longitude of this address.', 'SCHEMA', N'Person', 'TABLE', N'Address', 'COLUMN', N'SpatialLocation'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Address', 'COLUMN', N'StateProvinceID'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique identification number for the state or province. Foreign key to StateProvince table.', 'SCHEMA', N'Person', 'TABLE', N'Address', 'COLUMN', N'StateProvinceID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Address', 'CONSTRAINT', N'DF_Address_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Person', 'TABLE', N'Address', 'CONSTRAINT', N'DF_Address_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Address', 'CONSTRAINT', N'DF_Address_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()', 'SCHEMA', N'Person', 'TABLE', N'Address', 'CONSTRAINT', N'DF_Address_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Address', 'CONSTRAINT', N'FK_Address_StateProvince_StateProvinceID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing StateProvince.StateProvinceID.', 'SCHEMA', N'Person', 'TABLE', N'Address', 'CONSTRAINT', N'FK_Address_StateProvince_StateProvinceID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Address', 'CONSTRAINT', N'PK_Address_AddressID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Person', 'TABLE', N'Address', 'CONSTRAINT', N'PK_Address_AddressID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Address', 'INDEX', N'AK_Address_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index. Used to support replication samples.', 'SCHEMA', N'Person', 'TABLE', N'Address', 'INDEX', N'AK_Address_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Address', 'INDEX', N'IX_Address_AddressLine1_AddressLine2_City_StateProvinceID_PostalCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Person', 'TABLE', N'Address', 'INDEX', N'IX_Address_AddressLine1_AddressLine2_City_StateProvinceID_PostalCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Address', 'INDEX', N'IX_Address_StateProvinceID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Person', 'TABLE', N'Address', 'INDEX', N'IX_Address_StateProvinceID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Address', 'INDEX', N'PK_Address_AddressID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Person', 'TABLE', N'Address', 'INDEX', N'PK_Address_AddressID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Cross-reference table mapping customers, vendors, and employees to their addresses.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'COLUMN', N'AddressID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key. Foreign key to Address.AddressID.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'COLUMN', N'AddressID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'COLUMN', N'AddressTypeID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key. Foreign key to AddressType.AddressTypeID.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'COLUMN', N'AddressTypeID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'COLUMN', N'BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key. Foreign key to BusinessEntity.BusinessEntityID.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'COLUMN', N'BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'CONSTRAINT', N'DF_BusinessEntityAddress_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'CONSTRAINT', N'DF_BusinessEntityAddress_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'CONSTRAINT', N'DF_BusinessEntityAddress_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'CONSTRAINT', N'DF_BusinessEntityAddress_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'CONSTRAINT', N'FK_BusinessEntityAddress_Address_AddressID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Address.AddressID.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'CONSTRAINT', N'FK_BusinessEntityAddress_Address_AddressID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'CONSTRAINT', N'FK_BusinessEntityAddress_AddressType_AddressTypeID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing AddressType.AddressTypeID.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'CONSTRAINT', N'FK_BusinessEntityAddress_AddressType_AddressTypeID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'CONSTRAINT', N'FK_BusinessEntityAddress_BusinessEntity_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing BusinessEntity.BusinessEntityID.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'CONSTRAINT', N'FK_BusinessEntityAddress_BusinessEntity_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'CONSTRAINT', N'PK_BusinessEntityAddress_BusinessEntityID_AddressID_AddressTypeID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'CONSTRAINT', N'PK_BusinessEntityAddress_BusinessEntityID_AddressID_AddressTypeID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'INDEX', N'AK_BusinessEntityAddress_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index. Used to support replication samples.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'INDEX', N'AK_BusinessEntityAddress_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'INDEX', N'IX_BusinessEntityAddress_AddressID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'INDEX', N'IX_BusinessEntityAddress_AddressID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'INDEX', N'IX_BusinessEntityAddress_AddressTypeID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'INDEX', N'IX_BusinessEntityAddress_AddressTypeID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'INDEX', N'PK_BusinessEntityAddress_BusinessEntityID_AddressID_AddressTypeID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityAddress', 'INDEX', N'PK_BusinessEntityAddress_BusinessEntityID_AddressID_AddressTypeID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Cross-reference table mapping stores, vendors, and employees to people', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'COLUMN', N'BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key. Foreign key to BusinessEntity.BusinessEntityID.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'COLUMN', N'BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'COLUMN', N'ContactTypeID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key.  Foreign key to ContactType.ContactTypeID.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'COLUMN', N'ContactTypeID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'COLUMN', N'PersonID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key. Foreign key to Person.BusinessEntityID.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'COLUMN', N'PersonID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'CONSTRAINT', N'DF_BusinessEntityContact_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'CONSTRAINT', N'DF_BusinessEntityContact_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'CONSTRAINT', N'DF_BusinessEntityContact_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'CONSTRAINT', N'DF_BusinessEntityContact_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'CONSTRAINT', N'FK_BusinessEntityContact_BusinessEntity_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing BusinessEntity.BusinessEntityID.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'CONSTRAINT', N'FK_BusinessEntityContact_BusinessEntity_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'CONSTRAINT', N'FK_BusinessEntityContact_ContactType_ContactTypeID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing ContactType.ContactTypeID.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'CONSTRAINT', N'FK_BusinessEntityContact_ContactType_ContactTypeID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'CONSTRAINT', N'FK_BusinessEntityContact_Person_PersonID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Person.BusinessEntityID.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'CONSTRAINT', N'FK_BusinessEntityContact_Person_PersonID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'CONSTRAINT', N'PK_BusinessEntityContact_BusinessEntityID_PersonID_ContactTypeID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'CONSTRAINT', N'PK_BusinessEntityContact_BusinessEntityID_PersonID_ContactTypeID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'INDEX', N'AK_BusinessEntityContact_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index. Used to support replication samples.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'INDEX', N'AK_BusinessEntityContact_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'INDEX', N'IX_BusinessEntityContact_ContactTypeID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'INDEX', N'IX_BusinessEntityContact_ContactTypeID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'INDEX', N'IX_BusinessEntityContact_PersonID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'INDEX', N'IX_BusinessEntityContact_PersonID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'INDEX', N'PK_BusinessEntityContact_BusinessEntityID_PersonID_ContactTypeID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntityContact', 'INDEX', N'PK_BusinessEntityContact_BusinessEntityID_PersonID_ContactTypeID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntity', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Source of the ID that connects vendors, customers, and employees with address and contact information.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntity', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntity', 'COLUMN', N'BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for all customers, vendors, and employees.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntity', 'COLUMN', N'BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntity', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntity', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntity', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntity', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntity', 'CONSTRAINT', N'DF_BusinessEntity_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntity', 'CONSTRAINT', N'DF_BusinessEntity_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntity', 'CONSTRAINT', N'DF_BusinessEntity_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntity', 'CONSTRAINT', N'DF_BusinessEntity_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntity', 'CONSTRAINT', N'PK_BusinessEntity_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntity', 'CONSTRAINT', N'PK_BusinessEntity_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntity', 'INDEX', N'AK_BusinessEntity_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index. Used to support replication samples.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntity', 'INDEX', N'AK_BusinessEntity_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntity', 'INDEX', N'PK_BusinessEntity_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Person', 'TABLE', N'BusinessEntity', 'INDEX', N'PK_BusinessEntity_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'ContactType', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Lookup table containing the types of business entity contacts.', 'SCHEMA', N'Person', 'TABLE', N'ContactType', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'ContactType', 'COLUMN', N'ContactTypeID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for ContactType records.', 'SCHEMA', N'Person', 'TABLE', N'ContactType', 'COLUMN', N'ContactTypeID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'ContactType', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Person', 'TABLE', N'ContactType', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'ContactType', 'COLUMN', N'Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Contact type description.', 'SCHEMA', N'Person', 'TABLE', N'ContactType', 'COLUMN', N'Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'ContactType', 'CONSTRAINT', N'DF_ContactType_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Person', 'TABLE', N'ContactType', 'CONSTRAINT', N'DF_ContactType_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'ContactType', 'CONSTRAINT', N'PK_ContactType_ContactTypeID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Person', 'TABLE', N'ContactType', 'CONSTRAINT', N'PK_ContactType_ContactTypeID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'ContactType', 'INDEX', N'AK_ContactType_Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'Person', 'TABLE', N'ContactType', 'INDEX', N'AK_ContactType_Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'ContactType', 'INDEX', N'PK_ContactType_ContactTypeID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Person', 'TABLE', N'ContactType', 'INDEX', N'PK_ContactType_ContactTypeID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'CountryRegion', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Lookup table containing the ISO standard codes for countries and regions.', 'SCHEMA', N'Person', 'TABLE', N'CountryRegion', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'CountryRegion', 'COLUMN', N'CountryRegionCode'))
EXEC sp_addextendedproperty N'MS_Description', N'ISO standard code for countries and regions.', 'SCHEMA', N'Person', 'TABLE', N'CountryRegion', 'COLUMN', N'CountryRegionCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'CountryRegion', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Person', 'TABLE', N'CountryRegion', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'CountryRegion', 'COLUMN', N'Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Country or region name.', 'SCHEMA', N'Person', 'TABLE', N'CountryRegion', 'COLUMN', N'Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'CountryRegion', 'CONSTRAINT', N'DF_CountryRegion_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Person', 'TABLE', N'CountryRegion', 'CONSTRAINT', N'DF_CountryRegion_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'CountryRegion', 'CONSTRAINT', N'PK_CountryRegion_CountryRegionCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Person', 'TABLE', N'CountryRegion', 'CONSTRAINT', N'PK_CountryRegion_CountryRegionCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'CountryRegion', 'INDEX', N'AK_CountryRegion_Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'Person', 'TABLE', N'CountryRegion', 'INDEX', N'AK_CountryRegion_Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'CountryRegion', 'INDEX', N'PK_CountryRegion_CountryRegionCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Person', 'TABLE', N'CountryRegion', 'INDEX', N'PK_CountryRegion_CountryRegionCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'EmailAddress', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Where to send a person email.', 'SCHEMA', N'Person', 'TABLE', N'EmailAddress', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'EmailAddress', 'COLUMN', N'BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key. Person associated with this email address.  Foreign key to Person.BusinessEntityID', 'SCHEMA', N'Person', 'TABLE', N'EmailAddress', 'COLUMN', N'BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'EmailAddress', 'COLUMN', N'EmailAddress'))
EXEC sp_addextendedproperty N'MS_Description', N'E-mail address for the person.', 'SCHEMA', N'Person', 'TABLE', N'EmailAddress', 'COLUMN', N'EmailAddress'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'EmailAddress', 'COLUMN', N'EmailAddressID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key. ID of this email address.', 'SCHEMA', N'Person', 'TABLE', N'EmailAddress', 'COLUMN', N'EmailAddressID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'EmailAddress', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Person', 'TABLE', N'EmailAddress', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'EmailAddress', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', 'SCHEMA', N'Person', 'TABLE', N'EmailAddress', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'EmailAddress', 'CONSTRAINT', N'DF_EmailAddress_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Person', 'TABLE', N'EmailAddress', 'CONSTRAINT', N'DF_EmailAddress_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'EmailAddress', 'CONSTRAINT', N'DF_EmailAddress_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()', 'SCHEMA', N'Person', 'TABLE', N'EmailAddress', 'CONSTRAINT', N'DF_EmailAddress_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'EmailAddress', 'CONSTRAINT', N'FK_EmailAddress_Person_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Person.BusinessEntityID.', 'SCHEMA', N'Person', 'TABLE', N'EmailAddress', 'CONSTRAINT', N'FK_EmailAddress_Person_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'EmailAddress', 'CONSTRAINT', N'PK_EmailAddress_BusinessEntityID_EmailAddressID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Person', 'TABLE', N'EmailAddress', 'CONSTRAINT', N'PK_EmailAddress_BusinessEntityID_EmailAddressID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'EmailAddress', 'INDEX', N'IX_EmailAddress_EmailAddress'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Person', 'TABLE', N'EmailAddress', 'INDEX', N'IX_EmailAddress_EmailAddress'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'EmailAddress', 'INDEX', N'PK_EmailAddress_BusinessEntityID_EmailAddressID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Person', 'TABLE', N'EmailAddress', 'INDEX', N'PK_EmailAddress_BusinessEntityID_EmailAddressID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Password', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'One way hashed authentication information', 'SCHEMA', N'Person', 'TABLE', N'Password', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Password', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Person', 'TABLE', N'Password', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Password', 'COLUMN', N'PasswordHash'))
EXEC sp_addextendedproperty N'MS_Description', N'Password for the e-mail account.', 'SCHEMA', N'Person', 'TABLE', N'Password', 'COLUMN', N'PasswordHash'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Password', 'COLUMN', N'PasswordSalt'))
EXEC sp_addextendedproperty N'MS_Description', N'Random value concatenated with the password string before the password is hashed.', 'SCHEMA', N'Person', 'TABLE', N'Password', 'COLUMN', N'PasswordSalt'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Password', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', 'SCHEMA', N'Person', 'TABLE', N'Password', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Password', 'CONSTRAINT', N'DF_Password_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Person', 'TABLE', N'Password', 'CONSTRAINT', N'DF_Password_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Password', 'CONSTRAINT', N'DF_Password_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()', 'SCHEMA', N'Person', 'TABLE', N'Password', 'CONSTRAINT', N'DF_Password_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Password', 'CONSTRAINT', N'FK_Password_Person_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Person.BusinessEntityID.', 'SCHEMA', N'Person', 'TABLE', N'Password', 'CONSTRAINT', N'FK_Password_Person_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Password', 'CONSTRAINT', N'PK_Password_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Person', 'TABLE', N'Password', 'CONSTRAINT', N'PK_Password_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Password', 'INDEX', N'PK_Password_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Person', 'TABLE', N'Password', 'INDEX', N'PK_Password_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'PersonPhone', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Telephone number and type of a person.', 'SCHEMA', N'Person', 'TABLE', N'PersonPhone', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'PersonPhone', 'COLUMN', N'BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Business entity identification number. Foreign key to Person.BusinessEntityID.', 'SCHEMA', N'Person', 'TABLE', N'PersonPhone', 'COLUMN', N'BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'PersonPhone', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Person', 'TABLE', N'PersonPhone', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'PersonPhone', 'COLUMN', N'PhoneNumber'))
EXEC sp_addextendedproperty N'MS_Description', N'Telephone number identification number.', 'SCHEMA', N'Person', 'TABLE', N'PersonPhone', 'COLUMN', N'PhoneNumber'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'PersonPhone', 'COLUMN', N'PhoneNumberTypeID'))
EXEC sp_addextendedproperty N'MS_Description', N'Kind of phone number. Foreign key to PhoneNumberType.PhoneNumberTypeID.', 'SCHEMA', N'Person', 'TABLE', N'PersonPhone', 'COLUMN', N'PhoneNumberTypeID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'PersonPhone', 'CONSTRAINT', N'PK_PersonPhone_BusinessEntityID_PhoneNumber_PhoneNumberTypeID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Person', 'TABLE', N'PersonPhone', 'CONSTRAINT', N'PK_PersonPhone_BusinessEntityID_PhoneNumber_PhoneNumberTypeID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'PersonPhone', 'INDEX', N'IX_PersonPhone_PhoneNumber'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Person', 'TABLE', N'PersonPhone', 'INDEX', N'IX_PersonPhone_PhoneNumber'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'PersonPhone', 'INDEX', N'PK_PersonPhone_BusinessEntityID_PhoneNumber_PhoneNumberTypeID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Person', 'TABLE', N'PersonPhone', 'INDEX', N'PK_PersonPhone_BusinessEntityID_PhoneNumber_PhoneNumberTypeID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Human beings involved with AdventureWorks: employees, customer contacts, and vendor contacts.', 'SCHEMA', N'Person', 'TABLE', N'Person', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'AdditionalContactInfo'))
EXEC sp_addextendedproperty N'MS_Description', N'Additional contact information about the person stored in xml format. ', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'AdditionalContactInfo'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for Person records.', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'Demographics'))
EXEC sp_addextendedproperty N'MS_Description', N'Personal information such as hobbies, and income collected from online shoppers. Used for sales analysis.', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'Demographics'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'EmailPromotion'))
EXEC sp_addextendedproperty N'MS_Description', N'0 = Contact does not wish to receive e-mail promotions, 1 = Contact does wish to receive e-mail promotions from AdventureWorks, 2 = Contact does wish to receive e-mail promotions from AdventureWorks and selected partners. ', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'EmailPromotion'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'FirstName'))
EXEC sp_addextendedproperty N'MS_Description', N'First name of the person.', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'FirstName'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'LastName'))
EXEC sp_addextendedproperty N'MS_Description', N'Last name of the person.', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'LastName'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'MiddleName'))
EXEC sp_addextendedproperty N'MS_Description', N'Middle name or middle initial of the person.', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'MiddleName'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'NameStyle'))
EXEC sp_addextendedproperty N'MS_Description', N'0 = The data in FirstName and LastName are stored in western style (first name, last name) order.  1 = Eastern style (last name, first name) order.', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'NameStyle'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'PersonType'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary type of person: SC = Store Contact, IN = Individual (retail) customer, SP = Sales person, EM = Employee (non-sales), VC = Vendor contact, GC = General contact', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'PersonType'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'Suffix'))
EXEC sp_addextendedproperty N'MS_Description', N'Surname suffix. For example, Sr. or Jr.', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'Suffix'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'Title'))
EXEC sp_addextendedproperty N'MS_Description', N'A courtesy title. For example, Mr. or Ms.', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'Title'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'CONSTRAINT', N'CK_Person_EmailPromotion'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [EmailPromotion] >= (0) AND [EmailPromotion] <= (2)', 'SCHEMA', N'Person', 'TABLE', N'Person', 'CONSTRAINT', N'CK_Person_EmailPromotion'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'CONSTRAINT', N'CK_Person_PersonType'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [PersonType] is one of SC, VC, IN, EM or SP.', 'SCHEMA', N'Person', 'TABLE', N'Person', 'CONSTRAINT', N'CK_Person_PersonType'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'CONSTRAINT', N'DF_Person_EmailPromotion'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0', 'SCHEMA', N'Person', 'TABLE', N'Person', 'CONSTRAINT', N'DF_Person_EmailPromotion'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'CONSTRAINT', N'DF_Person_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Person', 'TABLE', N'Person', 'CONSTRAINT', N'DF_Person_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'CONSTRAINT', N'DF_Person_NameStyle'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0', 'SCHEMA', N'Person', 'TABLE', N'Person', 'CONSTRAINT', N'DF_Person_NameStyle'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'CONSTRAINT', N'DF_Person_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()', 'SCHEMA', N'Person', 'TABLE', N'Person', 'CONSTRAINT', N'DF_Person_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'CONSTRAINT', N'FK_Person_BusinessEntity_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing BusinessEntity.BusinessEntityID.', 'SCHEMA', N'Person', 'TABLE', N'Person', 'CONSTRAINT', N'FK_Person_BusinessEntity_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'CONSTRAINT', N'PK_Person_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Person', 'TABLE', N'Person', 'CONSTRAINT', N'PK_Person_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'INDEX', N'AK_Person_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index. Used to support replication samples.', 'SCHEMA', N'Person', 'TABLE', N'Person', 'INDEX', N'AK_Person_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'INDEX', N'PK_Person_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Person', 'TABLE', N'Person', 'INDEX', N'PK_Person_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'INDEX', N'PXML_Person_AddContact'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary XML index.', 'SCHEMA', N'Person', 'TABLE', N'Person', 'INDEX', N'PXML_Person_AddContact'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'INDEX', N'PXML_Person_Demographics'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary XML index.', 'SCHEMA', N'Person', 'TABLE', N'Person', 'INDEX', N'PXML_Person_Demographics'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'INDEX', N'XMLPATH_Person_Demographics'))
EXEC sp_addextendedproperty N'MS_Description', N'Secondary XML index for path.', 'SCHEMA', N'Person', 'TABLE', N'Person', 'INDEX', N'XMLPATH_Person_Demographics'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'INDEX', N'XMLPROPERTY_Person_Demographics'))
EXEC sp_addextendedproperty N'MS_Description', N'Secondary XML index for property.', 'SCHEMA', N'Person', 'TABLE', N'Person', 'INDEX', N'XMLPROPERTY_Person_Demographics'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'INDEX', N'XMLVALUE_Person_Demographics'))
EXEC sp_addextendedproperty N'MS_Description', N'Secondary XML index for value.', 'SCHEMA', N'Person', 'TABLE', N'Person', 'INDEX', N'XMLVALUE_Person_Demographics'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'Person', 'TRIGGER', N'iuPerson'))
EXEC sp_addextendedproperty N'MS_Description', N'AFTER INSERT, UPDATE trigger inserting Individual only if the Customer does not exist in the Store table and setting the ModifiedDate column in the Person table to the current date.', 'SCHEMA', N'Person', 'TABLE', N'Person', 'TRIGGER', N'iuPerson'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'PhoneNumberType', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Type of phone number of a person.', 'SCHEMA', N'Person', 'TABLE', N'PhoneNumberType', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'PhoneNumberType', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Person', 'TABLE', N'PhoneNumberType', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'PhoneNumberType', 'COLUMN', N'Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Name of the telephone number type', 'SCHEMA', N'Person', 'TABLE', N'PhoneNumberType', 'COLUMN', N'Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'PhoneNumberType', 'COLUMN', N'PhoneNumberTypeID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for telephone number type records.', 'SCHEMA', N'Person', 'TABLE', N'PhoneNumberType', 'COLUMN', N'PhoneNumberTypeID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'PhoneNumberType', 'CONSTRAINT', N'DF_PhoneNumberType_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Person', 'TABLE', N'PhoneNumberType', 'CONSTRAINT', N'DF_PhoneNumberType_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'PhoneNumberType', 'CONSTRAINT', N'PK_PhoneNumberType_PhoneNumberTypeID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Person', 'TABLE', N'PhoneNumberType', 'CONSTRAINT', N'PK_PhoneNumberType_PhoneNumberTypeID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'State and province lookup table.', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'COLUMN', N'CountryRegionCode'))
EXEC sp_addextendedproperty N'MS_Description', N'ISO standard country or region code. Foreign key to CountryRegion.CountryRegionCode. ', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'COLUMN', N'CountryRegionCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'COLUMN', N'IsOnlyStateProvinceFlag'))
EXEC sp_addextendedproperty N'MS_Description', N'0 = StateProvinceCode exists. 1 = StateProvinceCode unavailable, using CountryRegionCode.', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'COLUMN', N'IsOnlyStateProvinceFlag'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'COLUMN', N'Name'))
EXEC sp_addextendedproperty N'MS_Description', N'State or province description.', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'COLUMN', N'Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'COLUMN', N'StateProvinceCode'))
EXEC sp_addextendedproperty N'MS_Description', N'ISO standard state or province code.', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'COLUMN', N'StateProvinceCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'COLUMN', N'StateProvinceID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for StateProvince records.', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'COLUMN', N'StateProvinceID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'COLUMN', N'TerritoryID'))
EXEC sp_addextendedproperty N'MS_Description', N'ID of the territory in which the state or province is located. Foreign key to SalesTerritory.SalesTerritoryID.', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'COLUMN', N'TerritoryID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'CONSTRAINT', N'DF_StateProvince_IsOnlyStateProvinceFlag'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 1 (TRUE)', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'CONSTRAINT', N'DF_StateProvince_IsOnlyStateProvinceFlag'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'CONSTRAINT', N'DF_StateProvince_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'CONSTRAINT', N'DF_StateProvince_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'CONSTRAINT', N'DF_StateProvince_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'CONSTRAINT', N'DF_StateProvince_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'CONSTRAINT', N'FK_StateProvince_CountryRegion_CountryRegionCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing CountryRegion.CountryRegionCode.', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'CONSTRAINT', N'FK_StateProvince_CountryRegion_CountryRegionCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'CONSTRAINT', N'FK_StateProvince_SalesTerritory_TerritoryID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing SalesTerritory.TerritoryID.', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'CONSTRAINT', N'FK_StateProvince_SalesTerritory_TerritoryID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'CONSTRAINT', N'PK_StateProvince_StateProvinceID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'CONSTRAINT', N'PK_StateProvince_StateProvinceID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'INDEX', N'AK_StateProvince_Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'INDEX', N'AK_StateProvince_Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'INDEX', N'AK_StateProvince_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index. Used to support replication samples.', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'INDEX', N'AK_StateProvince_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'INDEX', N'AK_StateProvince_StateProvinceCode_CountryRegionCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'INDEX', N'AK_StateProvince_StateProvinceCode_CountryRegionCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'INDEX', N'PK_StateProvince_StateProvinceID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Person', 'TABLE', N'StateProvince', 'INDEX', N'PK_StateProvince_StateProvinceID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Items required to make bicycles and bicycle subassemblies. It identifies the heirarchical relationship between a parent product and its components.', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'COLUMN', N'BillOfMaterialsID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for BillOfMaterials records.', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'COLUMN', N'BillOfMaterialsID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'COLUMN', N'BOMLevel'))
EXEC sp_addextendedproperty N'MS_Description', N'Indicates the depth the component is from its parent (AssemblyID).', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'COLUMN', N'BOMLevel'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'COLUMN', N'ComponentID'))
EXEC sp_addextendedproperty N'MS_Description', N'Component identification number. Foreign key to Product.ProductID.', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'COLUMN', N'ComponentID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'COLUMN', N'EndDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date the component stopped being used in the assembly item.', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'COLUMN', N'EndDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'COLUMN', N'PerAssemblyQty'))
EXEC sp_addextendedproperty N'MS_Description', N'Quantity of the component needed to create the assembly.', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'COLUMN', N'PerAssemblyQty'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'COLUMN', N'ProductAssemblyID'))
EXEC sp_addextendedproperty N'MS_Description', N'Parent product identification number. Foreign key to Product.ProductID.', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'COLUMN', N'ProductAssemblyID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'COLUMN', N'StartDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date the component started being used in the assembly item.', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'COLUMN', N'StartDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'COLUMN', N'UnitMeasureCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Standard code identifying the unit of measure for the quantity.', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'COLUMN', N'UnitMeasureCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'CONSTRAINT', N'CK_BillOfMaterials_BOMLevel'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [ProductAssemblyID] IS NULL AND [BOMLevel] = (0) AND [PerAssemblyQty] = (1) OR [ProductAssemblyID] IS NOT NULL AND [BOMLevel] >= (1)', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'CONSTRAINT', N'CK_BillOfMaterials_BOMLevel'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'CONSTRAINT', N'CK_BillOfMaterials_EndDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint EndDate] > [StartDate] OR [EndDate] IS NULL', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'CONSTRAINT', N'CK_BillOfMaterials_EndDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'CONSTRAINT', N'CK_BillOfMaterials_PerAssemblyQty'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [PerAssemblyQty] >= (1.00)', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'CONSTRAINT', N'CK_BillOfMaterials_PerAssemblyQty'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'CONSTRAINT', N'CK_BillOfMaterials_ProductAssemblyID'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [ProductAssemblyID] <> [ComponentID]', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'CONSTRAINT', N'CK_BillOfMaterials_ProductAssemblyID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'CONSTRAINT', N'DF_BillOfMaterials_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'CONSTRAINT', N'DF_BillOfMaterials_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'CONSTRAINT', N'DF_BillOfMaterials_PerAssemblyQty'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 1.0', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'CONSTRAINT', N'DF_BillOfMaterials_PerAssemblyQty'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'CONSTRAINT', N'DF_BillOfMaterials_StartDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'CONSTRAINT', N'DF_BillOfMaterials_StartDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'CONSTRAINT', N'FK_BillOfMaterials_Product_ComponentID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Product.ComponentID.', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'CONSTRAINT', N'FK_BillOfMaterials_Product_ComponentID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'CONSTRAINT', N'FK_BillOfMaterials_Product_ProductAssemblyID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Product.ProductAssemblyID.', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'CONSTRAINT', N'FK_BillOfMaterials_Product_ProductAssemblyID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'CONSTRAINT', N'FK_BillOfMaterials_UnitMeasure_UnitMeasureCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing UnitMeasure.UnitMeasureCode.', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'CONSTRAINT', N'FK_BillOfMaterials_UnitMeasure_UnitMeasureCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'CONSTRAINT', N'PK_BillOfMaterials_BillOfMaterialsID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'CONSTRAINT', N'PK_BillOfMaterials_BillOfMaterialsID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'INDEX', N'AK_BillOfMaterials_ProductAssemblyID_ComponentID_StartDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index.', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'INDEX', N'AK_BillOfMaterials_ProductAssemblyID_ComponentID_StartDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'INDEX', N'IX_BillOfMaterials_UnitMeasureCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'INDEX', N'IX_BillOfMaterials_UnitMeasureCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'INDEX', N'PK_BillOfMaterials_BillOfMaterialsID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index created by a primary key constraint.', 'SCHEMA', N'Production', 'TABLE', N'BillOfMaterials', 'INDEX', N'PK_BillOfMaterials_BillOfMaterialsID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Culture', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Lookup table containing the languages in which some AdventureWorks data is stored.', 'SCHEMA', N'Production', 'TABLE', N'Culture', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Culture', 'COLUMN', N'CultureID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for Culture records.', 'SCHEMA', N'Production', 'TABLE', N'Culture', 'COLUMN', N'CultureID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Culture', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Production', 'TABLE', N'Culture', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Culture', 'COLUMN', N'Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Culture description.', 'SCHEMA', N'Production', 'TABLE', N'Culture', 'COLUMN', N'Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Culture', 'CONSTRAINT', N'DF_Culture_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'Culture', 'CONSTRAINT', N'DF_Culture_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Culture', 'CONSTRAINT', N'PK_Culture_CultureID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Production', 'TABLE', N'Culture', 'CONSTRAINT', N'PK_Culture_CultureID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Culture', 'INDEX', N'AK_Culture_Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'Production', 'TABLE', N'Culture', 'INDEX', N'AK_Culture_Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Culture', 'INDEX', N'PK_Culture_CultureID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Production', 'TABLE', N'Culture', 'INDEX', N'PK_Culture_CultureID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Document', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Product maintenance documents.', 'SCHEMA', N'Production', 'TABLE', N'Document', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Document', 'COLUMN', N'ChangeNumber'))
EXEC sp_addextendedproperty N'MS_Description', N'Engineering change approval number.', 'SCHEMA', N'Production', 'TABLE', N'Document', 'COLUMN', N'ChangeNumber'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Document', 'COLUMN', N'Document'))
EXEC sp_addextendedproperty N'MS_Description', N'Complete document.', 'SCHEMA', N'Production', 'TABLE', N'Document', 'COLUMN', N'Document'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Document', 'COLUMN', N'DocumentLevel'))
EXEC sp_addextendedproperty N'MS_Description', N'Depth in the document hierarchy.', 'SCHEMA', N'Production', 'TABLE', N'Document', 'COLUMN', N'DocumentLevel'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Document', 'COLUMN', N'DocumentNode'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for Document records.', 'SCHEMA', N'Production', 'TABLE', N'Document', 'COLUMN', N'DocumentNode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Document', 'COLUMN', N'DocumentSummary'))
EXEC sp_addextendedproperty N'MS_Description', N'Document abstract.', 'SCHEMA', N'Production', 'TABLE', N'Document', 'COLUMN', N'DocumentSummary'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Document', 'COLUMN', N'FileExtension'))
EXEC sp_addextendedproperty N'MS_Description', N'File extension indicating the document type. For example, .doc or .txt.', 'SCHEMA', N'Production', 'TABLE', N'Document', 'COLUMN', N'FileExtension'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Document', 'COLUMN', N'FileName'))
EXEC sp_addextendedproperty N'MS_Description', N'File name of the document', 'SCHEMA', N'Production', 'TABLE', N'Document', 'COLUMN', N'FileName'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Document', 'COLUMN', N'FolderFlag'))
EXEC sp_addextendedproperty N'MS_Description', N'0 = This is a folder, 1 = This is a document.', 'SCHEMA', N'Production', 'TABLE', N'Document', 'COLUMN', N'FolderFlag'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Document', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Production', 'TABLE', N'Document', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Document', 'COLUMN', N'Owner'))
EXEC sp_addextendedproperty N'MS_Description', N'Employee who controls the document.  Foreign key to Employee.BusinessEntityID', 'SCHEMA', N'Production', 'TABLE', N'Document', 'COLUMN', N'Owner'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Document', 'COLUMN', N'Revision'))
EXEC sp_addextendedproperty N'MS_Description', N'Revision number of the document. ', 'SCHEMA', N'Production', 'TABLE', N'Document', 'COLUMN', N'Revision'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Document', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Required for FileStream.', 'SCHEMA', N'Production', 'TABLE', N'Document', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Document', 'COLUMN', N'Status'))
EXEC sp_addextendedproperty N'MS_Description', N'1 = Pending approval, 2 = Approved, 3 = Obsolete', 'SCHEMA', N'Production', 'TABLE', N'Document', 'COLUMN', N'Status'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Document', 'COLUMN', N'Title'))
EXEC sp_addextendedproperty N'MS_Description', N'Title of the document.', 'SCHEMA', N'Production', 'TABLE', N'Document', 'COLUMN', N'Title'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Document', 'CONSTRAINT', N'CK_Document_Status'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [Status] BETWEEN (1) AND (3)', 'SCHEMA', N'Production', 'TABLE', N'Document', 'CONSTRAINT', N'CK_Document_Status'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Document', 'CONSTRAINT', N'DF_Document_ChangeNumber'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0', 'SCHEMA', N'Production', 'TABLE', N'Document', 'CONSTRAINT', N'DF_Document_ChangeNumber'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Document', 'CONSTRAINT', N'DF_Document_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'Document', 'CONSTRAINT', N'DF_Document_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Document', 'CONSTRAINT', N'DF_Document_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()', 'SCHEMA', N'Production', 'TABLE', N'Document', 'CONSTRAINT', N'DF_Document_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Document', 'CONSTRAINT', N'FK_Document_Employee_Owner'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Employee.BusinessEntityID.', 'SCHEMA', N'Production', 'TABLE', N'Document', 'CONSTRAINT', N'FK_Document_Employee_Owner'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Document', 'CONSTRAINT', N'PK_Document_DocumentNode'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Production', 'TABLE', N'Document', 'CONSTRAINT', N'PK_Document_DocumentNode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Document', 'INDEX', N'AK_Document_DocumentLevel_DocumentNode'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'Production', 'TABLE', N'Document', 'INDEX', N'AK_Document_DocumentLevel_DocumentNode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Document', 'INDEX', N'AK_Document_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index. Used to support FileStream.', 'SCHEMA', N'Production', 'TABLE', N'Document', 'INDEX', N'AK_Document_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Document', 'INDEX', N'IX_Document_FileName_Revision'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'Production', 'TABLE', N'Document', 'INDEX', N'IX_Document_FileName_Revision'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Document', 'INDEX', N'PK_Document_DocumentNode'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Production', 'TABLE', N'Document', 'INDEX', N'PK_Document_DocumentNode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Illustration', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Bicycle assembly diagrams.', 'SCHEMA', N'Production', 'TABLE', N'Illustration', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Illustration', 'COLUMN', N'Diagram'))
EXEC sp_addextendedproperty N'MS_Description', N'Illustrations used in manufacturing instructions. Stored as XML.', 'SCHEMA', N'Production', 'TABLE', N'Illustration', 'COLUMN', N'Diagram'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Illustration', 'COLUMN', N'IllustrationID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for Illustration records.', 'SCHEMA', N'Production', 'TABLE', N'Illustration', 'COLUMN', N'IllustrationID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Illustration', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Production', 'TABLE', N'Illustration', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Illustration', 'CONSTRAINT', N'DF_Illustration_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'Illustration', 'CONSTRAINT', N'DF_Illustration_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Illustration', 'CONSTRAINT', N'PK_Illustration_IllustrationID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Production', 'TABLE', N'Illustration', 'CONSTRAINT', N'PK_Illustration_IllustrationID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Illustration', 'INDEX', N'PK_Illustration_IllustrationID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Production', 'TABLE', N'Illustration', 'INDEX', N'PK_Illustration_IllustrationID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Location', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Product inventory and manufacturing locations.', 'SCHEMA', N'Production', 'TABLE', N'Location', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Location', 'COLUMN', N'Availability'))
EXEC sp_addextendedproperty N'MS_Description', N'Work capacity (in hours) of the manufacturing location.', 'SCHEMA', N'Production', 'TABLE', N'Location', 'COLUMN', N'Availability'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Location', 'COLUMN', N'CostRate'))
EXEC sp_addextendedproperty N'MS_Description', N'Standard hourly cost of the manufacturing location.', 'SCHEMA', N'Production', 'TABLE', N'Location', 'COLUMN', N'CostRate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Location', 'COLUMN', N'LocationID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for Location records.', 'SCHEMA', N'Production', 'TABLE', N'Location', 'COLUMN', N'LocationID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Location', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Production', 'TABLE', N'Location', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Location', 'COLUMN', N'Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Location description.', 'SCHEMA', N'Production', 'TABLE', N'Location', 'COLUMN', N'Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Location', 'CONSTRAINT', N'CK_Location_Availability'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [Availability] >= (0.00)', 'SCHEMA', N'Production', 'TABLE', N'Location', 'CONSTRAINT', N'CK_Location_Availability'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Location', 'CONSTRAINT', N'CK_Location_CostRate'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [CostRate] >= (0.00)', 'SCHEMA', N'Production', 'TABLE', N'Location', 'CONSTRAINT', N'CK_Location_CostRate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Location', 'CONSTRAINT', N'DF_Location_Availability'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0.00', 'SCHEMA', N'Production', 'TABLE', N'Location', 'CONSTRAINT', N'DF_Location_Availability'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Location', 'CONSTRAINT', N'DF_Location_CostRate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0.0', 'SCHEMA', N'Production', 'TABLE', N'Location', 'CONSTRAINT', N'DF_Location_CostRate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Location', 'CONSTRAINT', N'DF_Location_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'Location', 'CONSTRAINT', N'DF_Location_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Location', 'CONSTRAINT', N'PK_Location_LocationID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Production', 'TABLE', N'Location', 'CONSTRAINT', N'PK_Location_LocationID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Location', 'INDEX', N'AK_Location_Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'Production', 'TABLE', N'Location', 'INDEX', N'AK_Location_Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Location', 'INDEX', N'PK_Location_LocationID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Production', 'TABLE', N'Location', 'INDEX', N'PK_Location_LocationID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductCategory', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'High-level product categorization.', 'SCHEMA', N'Production', 'TABLE', N'ProductCategory', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductCategory', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Production', 'TABLE', N'ProductCategory', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductCategory', 'COLUMN', N'Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Category description.', 'SCHEMA', N'Production', 'TABLE', N'ProductCategory', 'COLUMN', N'Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductCategory', 'COLUMN', N'ProductCategoryID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for ProductCategory records.', 'SCHEMA', N'Production', 'TABLE', N'ProductCategory', 'COLUMN', N'ProductCategoryID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductCategory', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', 'SCHEMA', N'Production', 'TABLE', N'ProductCategory', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductCategory', 'CONSTRAINT', N'DF_ProductCategory_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'ProductCategory', 'CONSTRAINT', N'DF_ProductCategory_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductCategory', 'CONSTRAINT', N'DF_ProductCategory_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()()', 'SCHEMA', N'Production', 'TABLE', N'ProductCategory', 'CONSTRAINT', N'DF_ProductCategory_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductCategory', 'CONSTRAINT', N'PK_ProductCategory_ProductCategoryID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Production', 'TABLE', N'ProductCategory', 'CONSTRAINT', N'PK_ProductCategory_ProductCategoryID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductCategory', 'INDEX', N'AK_ProductCategory_Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'Production', 'TABLE', N'ProductCategory', 'INDEX', N'AK_ProductCategory_Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductCategory', 'INDEX', N'AK_ProductCategory_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index. Used to support replication samples.', 'SCHEMA', N'Production', 'TABLE', N'ProductCategory', 'INDEX', N'AK_ProductCategory_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductCategory', 'INDEX', N'PK_ProductCategory_ProductCategoryID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Production', 'TABLE', N'ProductCategory', 'INDEX', N'PK_ProductCategory_ProductCategoryID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductCostHistory', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Changes in the cost of a product over time.', 'SCHEMA', N'Production', 'TABLE', N'ProductCostHistory', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductCostHistory', 'COLUMN', N'EndDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Product cost end date.', 'SCHEMA', N'Production', 'TABLE', N'ProductCostHistory', 'COLUMN', N'EndDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductCostHistory', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Production', 'TABLE', N'ProductCostHistory', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductCostHistory', 'COLUMN', N'ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Product identification number. Foreign key to Product.ProductID', 'SCHEMA', N'Production', 'TABLE', N'ProductCostHistory', 'COLUMN', N'ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductCostHistory', 'COLUMN', N'StandardCost'))
EXEC sp_addextendedproperty N'MS_Description', N'Standard cost of the product.', 'SCHEMA', N'Production', 'TABLE', N'ProductCostHistory', 'COLUMN', N'StandardCost'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductCostHistory', 'COLUMN', N'StartDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Product cost start date.', 'SCHEMA', N'Production', 'TABLE', N'ProductCostHistory', 'COLUMN', N'StartDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductCostHistory', 'CONSTRAINT', N'CK_ProductCostHistory_EndDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [EndDate] >= [StartDate] OR [EndDate] IS NULL', 'SCHEMA', N'Production', 'TABLE', N'ProductCostHistory', 'CONSTRAINT', N'CK_ProductCostHistory_EndDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductCostHistory', 'CONSTRAINT', N'CK_ProductCostHistory_StandardCost'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [StandardCost] >= (0.00)', 'SCHEMA', N'Production', 'TABLE', N'ProductCostHistory', 'CONSTRAINT', N'CK_ProductCostHistory_StandardCost'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductCostHistory', 'CONSTRAINT', N'DF_ProductCostHistory_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'ProductCostHistory', 'CONSTRAINT', N'DF_ProductCostHistory_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductCostHistory', 'CONSTRAINT', N'FK_ProductCostHistory_Product_ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Product.ProductID.', 'SCHEMA', N'Production', 'TABLE', N'ProductCostHistory', 'CONSTRAINT', N'FK_ProductCostHistory_Product_ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductCostHistory', 'CONSTRAINT', N'PK_ProductCostHistory_ProductID_StartDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Production', 'TABLE', N'ProductCostHistory', 'CONSTRAINT', N'PK_ProductCostHistory_ProductID_StartDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductCostHistory', 'INDEX', N'PK_ProductCostHistory_ProductID_StartDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Production', 'TABLE', N'ProductCostHistory', 'INDEX', N'PK_ProductCostHistory_ProductID_StartDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductDescription', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Product descriptions in several languages.', 'SCHEMA', N'Production', 'TABLE', N'ProductDescription', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductDescription', 'COLUMN', N'Description'))
EXEC sp_addextendedproperty N'MS_Description', N'Description of the product.', 'SCHEMA', N'Production', 'TABLE', N'ProductDescription', 'COLUMN', N'Description'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductDescription', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Production', 'TABLE', N'ProductDescription', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductDescription', 'COLUMN', N'ProductDescriptionID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for ProductDescription records.', 'SCHEMA', N'Production', 'TABLE', N'ProductDescription', 'COLUMN', N'ProductDescriptionID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductDescription', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', 'SCHEMA', N'Production', 'TABLE', N'ProductDescription', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductDescription', 'CONSTRAINT', N'DF_ProductDescription_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'ProductDescription', 'CONSTRAINT', N'DF_ProductDescription_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductDescription', 'CONSTRAINT', N'DF_ProductDescription_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()', 'SCHEMA', N'Production', 'TABLE', N'ProductDescription', 'CONSTRAINT', N'DF_ProductDescription_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductDescription', 'CONSTRAINT', N'PK_ProductDescription_ProductDescriptionID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Production', 'TABLE', N'ProductDescription', 'CONSTRAINT', N'PK_ProductDescription_ProductDescriptionID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductDescription', 'INDEX', N'AK_ProductDescription_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index. Used to support replication samples.', 'SCHEMA', N'Production', 'TABLE', N'ProductDescription', 'INDEX', N'AK_ProductDescription_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductDescription', 'INDEX', N'PK_ProductDescription_ProductDescriptionID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Production', 'TABLE', N'ProductDescription', 'INDEX', N'PK_ProductDescription_ProductDescriptionID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductDocument', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Cross-reference table mapping products to related product documents.', 'SCHEMA', N'Production', 'TABLE', N'ProductDocument', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductDocument', 'COLUMN', N'DocumentNode'))
EXEC sp_addextendedproperty N'MS_Description', N'Document identification number. Foreign key to Document.DocumentNode.', 'SCHEMA', N'Production', 'TABLE', N'ProductDocument', 'COLUMN', N'DocumentNode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductDocument', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Production', 'TABLE', N'ProductDocument', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductDocument', 'COLUMN', N'ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Product identification number. Foreign key to Product.ProductID.', 'SCHEMA', N'Production', 'TABLE', N'ProductDocument', 'COLUMN', N'ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductDocument', 'CONSTRAINT', N'DF_ProductDocument_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'ProductDocument', 'CONSTRAINT', N'DF_ProductDocument_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductDocument', 'CONSTRAINT', N'FK_ProductDocument_Document_DocumentNode'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Document.DocumentNode.', 'SCHEMA', N'Production', 'TABLE', N'ProductDocument', 'CONSTRAINT', N'FK_ProductDocument_Document_DocumentNode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductDocument', 'CONSTRAINT', N'FK_ProductDocument_Product_ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Product.ProductID.', 'SCHEMA', N'Production', 'TABLE', N'ProductDocument', 'CONSTRAINT', N'FK_ProductDocument_Product_ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductDocument', 'CONSTRAINT', N'PK_ProductDocument_ProductID_DocumentNode'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Production', 'TABLE', N'ProductDocument', 'CONSTRAINT', N'PK_ProductDocument_ProductID_DocumentNode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductDocument', 'INDEX', N'PK_ProductDocument_ProductID_DocumentNode'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Production', 'TABLE', N'ProductDocument', 'INDEX', N'PK_ProductDocument_ProductID_DocumentNode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Product inventory information.', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'COLUMN', N'Bin'))
EXEC sp_addextendedproperty N'MS_Description', N'Storage container on a shelf in an inventory location.', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'COLUMN', N'Bin'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'COLUMN', N'LocationID'))
EXEC sp_addextendedproperty N'MS_Description', N'Inventory location identification number. Foreign key to Location.LocationID. ', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'COLUMN', N'LocationID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'COLUMN', N'ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Product identification number. Foreign key to Product.ProductID.', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'COLUMN', N'ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'COLUMN', N'Quantity'))
EXEC sp_addextendedproperty N'MS_Description', N'Quantity of products in the inventory location.', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'COLUMN', N'Quantity'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'COLUMN', N'Shelf'))
EXEC sp_addextendedproperty N'MS_Description', N'Storage compartment within an inventory location.', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'COLUMN', N'Shelf'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'CONSTRAINT', N'CK_ProductInventory_Bin'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [Bin] BETWEEN (0) AND (100)', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'CONSTRAINT', N'CK_ProductInventory_Bin'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'CONSTRAINT', N'CK_ProductInventory_Shelf'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [Shelf] like ''[A-Za-z]'' OR [Shelf]=''N/A''', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'CONSTRAINT', N'CK_ProductInventory_Shelf'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'CONSTRAINT', N'DF_ProductInventory_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'CONSTRAINT', N'DF_ProductInventory_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'CONSTRAINT', N'DF_ProductInventory_Quantity'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'CONSTRAINT', N'DF_ProductInventory_Quantity'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'CONSTRAINT', N'DF_ProductInventory_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'CONSTRAINT', N'DF_ProductInventory_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'CONSTRAINT', N'FK_ProductInventory_Location_LocationID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Location.LocationID.', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'CONSTRAINT', N'FK_ProductInventory_Location_LocationID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'CONSTRAINT', N'FK_ProductInventory_Product_ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Product.ProductID.', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'CONSTRAINT', N'FK_ProductInventory_Product_ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'CONSTRAINT', N'PK_ProductInventory_ProductID_LocationID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'CONSTRAINT', N'PK_ProductInventory_ProductID_LocationID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'INDEX', N'PK_ProductInventory_ProductID_LocationID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Production', 'TABLE', N'ProductInventory', 'INDEX', N'PK_ProductInventory_ProductID_LocationID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductListPriceHistory', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Changes in the list price of a product over time.', 'SCHEMA', N'Production', 'TABLE', N'ProductListPriceHistory', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductListPriceHistory', 'COLUMN', N'EndDate'))
EXEC sp_addextendedproperty N'MS_Description', N'List price end date', 'SCHEMA', N'Production', 'TABLE', N'ProductListPriceHistory', 'COLUMN', N'EndDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductListPriceHistory', 'COLUMN', N'ListPrice'))
EXEC sp_addextendedproperty N'MS_Description', N'Product list price.', 'SCHEMA', N'Production', 'TABLE', N'ProductListPriceHistory', 'COLUMN', N'ListPrice'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductListPriceHistory', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Production', 'TABLE', N'ProductListPriceHistory', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductListPriceHistory', 'COLUMN', N'ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Product identification number. Foreign key to Product.ProductID', 'SCHEMA', N'Production', 'TABLE', N'ProductListPriceHistory', 'COLUMN', N'ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductListPriceHistory', 'COLUMN', N'StartDate'))
EXEC sp_addextendedproperty N'MS_Description', N'List price start date.', 'SCHEMA', N'Production', 'TABLE', N'ProductListPriceHistory', 'COLUMN', N'StartDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductListPriceHistory', 'CONSTRAINT', N'CK_ProductListPriceHistory_EndDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [EndDate] >= [StartDate] OR [EndDate] IS NULL', 'SCHEMA', N'Production', 'TABLE', N'ProductListPriceHistory', 'CONSTRAINT', N'CK_ProductListPriceHistory_EndDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductListPriceHistory', 'CONSTRAINT', N'CK_ProductListPriceHistory_ListPrice'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [ListPrice] > (0.00)', 'SCHEMA', N'Production', 'TABLE', N'ProductListPriceHistory', 'CONSTRAINT', N'CK_ProductListPriceHistory_ListPrice'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductListPriceHistory', 'CONSTRAINT', N'DF_ProductListPriceHistory_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'ProductListPriceHistory', 'CONSTRAINT', N'DF_ProductListPriceHistory_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductListPriceHistory', 'CONSTRAINT', N'FK_ProductListPriceHistory_Product_ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Product.ProductID.', 'SCHEMA', N'Production', 'TABLE', N'ProductListPriceHistory', 'CONSTRAINT', N'FK_ProductListPriceHistory_Product_ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductListPriceHistory', 'CONSTRAINT', N'PK_ProductListPriceHistory_ProductID_StartDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Production', 'TABLE', N'ProductListPriceHistory', 'CONSTRAINT', N'PK_ProductListPriceHistory_ProductID_StartDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductListPriceHistory', 'INDEX', N'PK_ProductListPriceHistory_ProductID_StartDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Production', 'TABLE', N'ProductListPriceHistory', 'INDEX', N'PK_ProductListPriceHistory_ProductID_StartDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModelIllustration', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Cross-reference table mapping product models and illustrations.', 'SCHEMA', N'Production', 'TABLE', N'ProductModelIllustration', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModelIllustration', 'COLUMN', N'IllustrationID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key. Foreign key to Illustration.IllustrationID.', 'SCHEMA', N'Production', 'TABLE', N'ProductModelIllustration', 'COLUMN', N'IllustrationID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModelIllustration', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Production', 'TABLE', N'ProductModelIllustration', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModelIllustration', 'COLUMN', N'ProductModelID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key. Foreign key to ProductModel.ProductModelID.', 'SCHEMA', N'Production', 'TABLE', N'ProductModelIllustration', 'COLUMN', N'ProductModelID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModelIllustration', 'CONSTRAINT', N'DF_ProductModelIllustration_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'ProductModelIllustration', 'CONSTRAINT', N'DF_ProductModelIllustration_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModelIllustration', 'CONSTRAINT', N'FK_ProductModelIllustration_Illustration_IllustrationID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Illustration.IllustrationID.', 'SCHEMA', N'Production', 'TABLE', N'ProductModelIllustration', 'CONSTRAINT', N'FK_ProductModelIllustration_Illustration_IllustrationID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModelIllustration', 'CONSTRAINT', N'FK_ProductModelIllustration_ProductModel_ProductModelID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing ProductModel.ProductModelID.', 'SCHEMA', N'Production', 'TABLE', N'ProductModelIllustration', 'CONSTRAINT', N'FK_ProductModelIllustration_ProductModel_ProductModelID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModelIllustration', 'CONSTRAINT', N'PK_ProductModelIllustration_ProductModelID_IllustrationID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Production', 'TABLE', N'ProductModelIllustration', 'CONSTRAINT', N'PK_ProductModelIllustration_ProductModelID_IllustrationID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModelIllustration', 'INDEX', N'PK_ProductModelIllustration_ProductModelID_IllustrationID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Production', 'TABLE', N'ProductModelIllustration', 'INDEX', N'PK_ProductModelIllustration_ProductModelID_IllustrationID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModelProductDescriptionCulture', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Cross-reference table mapping product descriptions and the language the description is written in.', 'SCHEMA', N'Production', 'TABLE', N'ProductModelProductDescriptionCulture', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModelProductDescriptionCulture', 'COLUMN', N'CultureID'))
EXEC sp_addextendedproperty N'MS_Description', N'Culture identification number. Foreign key to Culture.CultureID.', 'SCHEMA', N'Production', 'TABLE', N'ProductModelProductDescriptionCulture', 'COLUMN', N'CultureID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModelProductDescriptionCulture', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Production', 'TABLE', N'ProductModelProductDescriptionCulture', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModelProductDescriptionCulture', 'COLUMN', N'ProductDescriptionID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key. Foreign key to ProductDescription.ProductDescriptionID.', 'SCHEMA', N'Production', 'TABLE', N'ProductModelProductDescriptionCulture', 'COLUMN', N'ProductDescriptionID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModelProductDescriptionCulture', 'COLUMN', N'ProductModelID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key. Foreign key to ProductModel.ProductModelID.', 'SCHEMA', N'Production', 'TABLE', N'ProductModelProductDescriptionCulture', 'COLUMN', N'ProductModelID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModelProductDescriptionCulture', 'CONSTRAINT', N'DF_ProductModelProductDescriptionCulture_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'ProductModelProductDescriptionCulture', 'CONSTRAINT', N'DF_ProductModelProductDescriptionCulture_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModelProductDescriptionCulture', 'CONSTRAINT', N'FK_ProductModelProductDescriptionCulture_Culture_CultureID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Culture.CultureID.', 'SCHEMA', N'Production', 'TABLE', N'ProductModelProductDescriptionCulture', 'CONSTRAINT', N'FK_ProductModelProductDescriptionCulture_Culture_CultureID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModelProductDescriptionCulture', 'CONSTRAINT', N'FK_ProductModelProductDescriptionCulture_ProductDescription_ProductDescriptionID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing ProductDescription.ProductDescriptionID.', 'SCHEMA', N'Production', 'TABLE', N'ProductModelProductDescriptionCulture', 'CONSTRAINT', N'FK_ProductModelProductDescriptionCulture_ProductDescription_ProductDescriptionID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModelProductDescriptionCulture', 'CONSTRAINT', N'FK_ProductModelProductDescriptionCulture_ProductModel_ProductModelID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing ProductModel.ProductModelID.', 'SCHEMA', N'Production', 'TABLE', N'ProductModelProductDescriptionCulture', 'CONSTRAINT', N'FK_ProductModelProductDescriptionCulture_ProductModel_ProductModelID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModelProductDescriptionCulture', 'CONSTRAINT', N'PK_ProductModelProductDescriptionCulture_ProductModelID_ProductDescriptionID_CultureID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Production', 'TABLE', N'ProductModelProductDescriptionCulture', 'CONSTRAINT', N'PK_ProductModelProductDescriptionCulture_ProductModelID_ProductDescriptionID_CultureID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModelProductDescriptionCulture', 'INDEX', N'PK_ProductModelProductDescriptionCulture_ProductModelID_ProductDescriptionID_CultureID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Production', 'TABLE', N'ProductModelProductDescriptionCulture', 'INDEX', N'PK_ProductModelProductDescriptionCulture_ProductModelID_ProductDescriptionID_CultureID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Product model classification.', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', 'COLUMN', N'CatalogDescription'))
EXEC sp_addextendedproperty N'MS_Description', N'Detailed product catalog information in xml format.', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', 'COLUMN', N'CatalogDescription'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', 'COLUMN', N'Instructions'))
EXEC sp_addextendedproperty N'MS_Description', N'Manufacturing instructions in xml format.', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', 'COLUMN', N'Instructions'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', 'COLUMN', N'Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Product model description.', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', 'COLUMN', N'Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', 'COLUMN', N'ProductModelID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for ProductModel records.', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', 'COLUMN', N'ProductModelID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', 'CONSTRAINT', N'DF_ProductModel_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', 'CONSTRAINT', N'DF_ProductModel_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', 'CONSTRAINT', N'DF_ProductModel_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', 'CONSTRAINT', N'DF_ProductModel_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', 'CONSTRAINT', N'PK_ProductModel_ProductModelID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', 'CONSTRAINT', N'PK_ProductModel_ProductModelID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', 'INDEX', N'AK_ProductModel_Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', 'INDEX', N'AK_ProductModel_Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', 'INDEX', N'AK_ProductModel_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index. Used to support replication samples.', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', 'INDEX', N'AK_ProductModel_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', 'INDEX', N'PK_ProductModel_ProductModelID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', 'INDEX', N'PK_ProductModel_ProductModelID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', 'INDEX', N'PXML_ProductModel_CatalogDescription'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary XML index.', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', 'INDEX', N'PXML_ProductModel_CatalogDescription'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', 'INDEX', N'PXML_ProductModel_Instructions'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary XML index.', 'SCHEMA', N'Production', 'TABLE', N'ProductModel', 'INDEX', N'PXML_ProductModel_Instructions'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductPhoto', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Product images.', 'SCHEMA', N'Production', 'TABLE', N'ProductPhoto', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductPhoto', 'COLUMN', N'LargePhoto'))
EXEC sp_addextendedproperty N'MS_Description', N'Large image of the product.', 'SCHEMA', N'Production', 'TABLE', N'ProductPhoto', 'COLUMN', N'LargePhoto'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductPhoto', 'COLUMN', N'LargePhotoFileName'))
EXEC sp_addextendedproperty N'MS_Description', N'Large image file name.', 'SCHEMA', N'Production', 'TABLE', N'ProductPhoto', 'COLUMN', N'LargePhotoFileName'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductPhoto', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Production', 'TABLE', N'ProductPhoto', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductPhoto', 'COLUMN', N'ProductPhotoID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for ProductPhoto records.', 'SCHEMA', N'Production', 'TABLE', N'ProductPhoto', 'COLUMN', N'ProductPhotoID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductPhoto', 'COLUMN', N'ThumbNailPhoto'))
EXEC sp_addextendedproperty N'MS_Description', N'Small image of the product.', 'SCHEMA', N'Production', 'TABLE', N'ProductPhoto', 'COLUMN', N'ThumbNailPhoto'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductPhoto', 'COLUMN', N'ThumbnailPhotoFileName'))
EXEC sp_addextendedproperty N'MS_Description', N'Small image file name.', 'SCHEMA', N'Production', 'TABLE', N'ProductPhoto', 'COLUMN', N'ThumbnailPhotoFileName'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductPhoto', 'CONSTRAINT', N'DF_ProductPhoto_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'ProductPhoto', 'CONSTRAINT', N'DF_ProductPhoto_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductPhoto', 'CONSTRAINT', N'PK_ProductPhoto_ProductPhotoID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Production', 'TABLE', N'ProductPhoto', 'CONSTRAINT', N'PK_ProductPhoto_ProductPhotoID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductPhoto', 'INDEX', N'PK_ProductPhoto_ProductPhotoID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Production', 'TABLE', N'ProductPhoto', 'INDEX', N'PK_ProductPhoto_ProductPhotoID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductProductPhoto', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Cross-reference table mapping products and product photos.', 'SCHEMA', N'Production', 'TABLE', N'ProductProductPhoto', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductProductPhoto', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Production', 'TABLE', N'ProductProductPhoto', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductProductPhoto', 'COLUMN', N'Primary'))
EXEC sp_addextendedproperty N'MS_Description', N'0 = Photo is not the principal image. 1 = Photo is the principal image.', 'SCHEMA', N'Production', 'TABLE', N'ProductProductPhoto', 'COLUMN', N'Primary'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductProductPhoto', 'COLUMN', N'ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Product identification number. Foreign key to Product.ProductID.', 'SCHEMA', N'Production', 'TABLE', N'ProductProductPhoto', 'COLUMN', N'ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductProductPhoto', 'COLUMN', N'ProductPhotoID'))
EXEC sp_addextendedproperty N'MS_Description', N'Product photo identification number. Foreign key to ProductPhoto.ProductPhotoID.', 'SCHEMA', N'Production', 'TABLE', N'ProductProductPhoto', 'COLUMN', N'ProductPhotoID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductProductPhoto', 'CONSTRAINT', N'DF_ProductProductPhoto_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'ProductProductPhoto', 'CONSTRAINT', N'DF_ProductProductPhoto_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductProductPhoto', 'CONSTRAINT', N'DF_ProductProductPhoto_Primary'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0 (FALSE)', 'SCHEMA', N'Production', 'TABLE', N'ProductProductPhoto', 'CONSTRAINT', N'DF_ProductProductPhoto_Primary'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductProductPhoto', 'CONSTRAINT', N'FK_ProductProductPhoto_Product_ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Product.ProductID.', 'SCHEMA', N'Production', 'TABLE', N'ProductProductPhoto', 'CONSTRAINT', N'FK_ProductProductPhoto_Product_ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductProductPhoto', 'CONSTRAINT', N'FK_ProductProductPhoto_ProductPhoto_ProductPhotoID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing ProductPhoto.ProductPhotoID.', 'SCHEMA', N'Production', 'TABLE', N'ProductProductPhoto', 'CONSTRAINT', N'FK_ProductProductPhoto_ProductPhoto_ProductPhotoID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductProductPhoto', 'CONSTRAINT', N'PK_ProductProductPhoto_ProductID_ProductPhotoID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Production', 'TABLE', N'ProductProductPhoto', 'CONSTRAINT', N'PK_ProductProductPhoto_ProductID_ProductPhotoID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductProductPhoto', 'INDEX', N'PK_ProductProductPhoto_ProductID_ProductPhotoID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index created by a primary key constraint.', 'SCHEMA', N'Production', 'TABLE', N'ProductProductPhoto', 'INDEX', N'PK_ProductProductPhoto_ProductID_ProductPhotoID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Customer reviews of products they have purchased.', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'COLUMN', N'Comments'))
EXEC sp_addextendedproperty N'MS_Description', N'Reviewer''s comments', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'COLUMN', N'Comments'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'COLUMN', N'EmailAddress'))
EXEC sp_addextendedproperty N'MS_Description', N'Reviewer''s e-mail address.', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'COLUMN', N'EmailAddress'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'COLUMN', N'ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Product identification number. Foreign key to Product.ProductID.', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'COLUMN', N'ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'COLUMN', N'ProductReviewID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for ProductReview records.', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'COLUMN', N'ProductReviewID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'COLUMN', N'Rating'))
EXEC sp_addextendedproperty N'MS_Description', N'Product rating given by the reviewer. Scale is 1 to 5 with 5 as the highest rating.', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'COLUMN', N'Rating'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'COLUMN', N'ReviewDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date review was submitted.', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'COLUMN', N'ReviewDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'COLUMN', N'ReviewerName'))
EXEC sp_addextendedproperty N'MS_Description', N'Name of the reviewer.', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'COLUMN', N'ReviewerName'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'CONSTRAINT', N'CK_ProductReview_Rating'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [Rating] BETWEEN (1) AND (5)', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'CONSTRAINT', N'CK_ProductReview_Rating'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'CONSTRAINT', N'DF_ProductReview_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'CONSTRAINT', N'DF_ProductReview_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'CONSTRAINT', N'DF_ProductReview_ReviewDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'CONSTRAINT', N'DF_ProductReview_ReviewDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'CONSTRAINT', N'FK_ProductReview_Product_ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Product.ProductID.', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'CONSTRAINT', N'FK_ProductReview_Product_ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'CONSTRAINT', N'PK_ProductReview_ProductReviewID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'CONSTRAINT', N'PK_ProductReview_ProductReviewID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'INDEX', N'IX_ProductReview_ProductID_Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'INDEX', N'IX_ProductReview_ProductID_Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'INDEX', N'PK_ProductReview_ProductReviewID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Production', 'TABLE', N'ProductReview', 'INDEX', N'PK_ProductReview_ProductReviewID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductSubcategory', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Product subcategories. See ProductCategory table.', 'SCHEMA', N'Production', 'TABLE', N'ProductSubcategory', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductSubcategory', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Production', 'TABLE', N'ProductSubcategory', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductSubcategory', 'COLUMN', N'Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Subcategory description.', 'SCHEMA', N'Production', 'TABLE', N'ProductSubcategory', 'COLUMN', N'Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductSubcategory', 'COLUMN', N'ProductCategoryID'))
EXEC sp_addextendedproperty N'MS_Description', N'Product category identification number. Foreign key to ProductCategory.ProductCategoryID.', 'SCHEMA', N'Production', 'TABLE', N'ProductSubcategory', 'COLUMN', N'ProductCategoryID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductSubcategory', 'COLUMN', N'ProductSubcategoryID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for ProductSubcategory records.', 'SCHEMA', N'Production', 'TABLE', N'ProductSubcategory', 'COLUMN', N'ProductSubcategoryID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductSubcategory', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', 'SCHEMA', N'Production', 'TABLE', N'ProductSubcategory', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductSubcategory', 'CONSTRAINT', N'DF_ProductSubcategory_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'ProductSubcategory', 'CONSTRAINT', N'DF_ProductSubcategory_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductSubcategory', 'CONSTRAINT', N'DF_ProductSubcategory_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()', 'SCHEMA', N'Production', 'TABLE', N'ProductSubcategory', 'CONSTRAINT', N'DF_ProductSubcategory_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductSubcategory', 'CONSTRAINT', N'FK_ProductSubcategory_ProductCategory_ProductCategoryID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing ProductCategory.ProductCategoryID.', 'SCHEMA', N'Production', 'TABLE', N'ProductSubcategory', 'CONSTRAINT', N'FK_ProductSubcategory_ProductCategory_ProductCategoryID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductSubcategory', 'CONSTRAINT', N'PK_ProductSubcategory_ProductSubcategoryID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Production', 'TABLE', N'ProductSubcategory', 'CONSTRAINT', N'PK_ProductSubcategory_ProductSubcategoryID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductSubcategory', 'INDEX', N'AK_ProductSubcategory_Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'Production', 'TABLE', N'ProductSubcategory', 'INDEX', N'AK_ProductSubcategory_Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductSubcategory', 'INDEX', N'AK_ProductSubcategory_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index. Used to support replication samples.', 'SCHEMA', N'Production', 'TABLE', N'ProductSubcategory', 'INDEX', N'AK_ProductSubcategory_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ProductSubcategory', 'INDEX', N'PK_ProductSubcategory_ProductSubcategoryID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Production', 'TABLE', N'ProductSubcategory', 'INDEX', N'PK_ProductSubcategory_ProductSubcategoryID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Products sold or used in the manfacturing of sold products.', 'SCHEMA', N'Production', 'TABLE', N'Product', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'Class'))
EXEC sp_addextendedproperty N'MS_Description', N'H = High, M = Medium, L = Low', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'Class'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'Color'))
EXEC sp_addextendedproperty N'MS_Description', N'Product color.', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'Color'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'DaysToManufacture'))
EXEC sp_addextendedproperty N'MS_Description', N'Number of days required to manufacture the product.', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'DaysToManufacture'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'DiscontinuedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date the product was discontinued.', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'DiscontinuedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'FinishedGoodsFlag'))
EXEC sp_addextendedproperty N'MS_Description', N'0 = Product is not a salable item. 1 = Product is salable.', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'FinishedGoodsFlag'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'ListPrice'))
EXEC sp_addextendedproperty N'MS_Description', N'Selling price.', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'ListPrice'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'MakeFlag'))
EXEC sp_addextendedproperty N'MS_Description', N'0 = Product is purchased, 1 = Product is manufactured in-house.', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'MakeFlag'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Name of the product.', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for Product records.', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'ProductLine'))
EXEC sp_addextendedproperty N'MS_Description', N'R = Road, M = Mountain, T = Touring, S = Standard', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'ProductLine'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'ProductModelID'))
EXEC sp_addextendedproperty N'MS_Description', N'Product is a member of this product model. Foreign key to ProductModel.ProductModelID.', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'ProductModelID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'ProductNumber'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique product identification number.', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'ProductNumber'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'ProductSubcategoryID'))
EXEC sp_addextendedproperty N'MS_Description', N'Product is a member of this product subcategory. Foreign key to ProductSubCategory.ProductSubCategoryID. ', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'ProductSubcategoryID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'ReorderPoint'))
EXEC sp_addextendedproperty N'MS_Description', N'Inventory level that triggers a purchase order or work order. ', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'ReorderPoint'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'SafetyStockLevel'))
EXEC sp_addextendedproperty N'MS_Description', N'Minimum inventory quantity. ', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'SafetyStockLevel'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'SellEndDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date the product was no longer available for sale.', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'SellEndDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'SellStartDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date the product was available for sale.', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'SellStartDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'Size'))
EXEC sp_addextendedproperty N'MS_Description', N'Product size.', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'Size'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'SizeUnitMeasureCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Unit of measure for Size column.', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'SizeUnitMeasureCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'StandardCost'))
EXEC sp_addextendedproperty N'MS_Description', N'Standard cost of the product.', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'StandardCost'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'Style'))
EXEC sp_addextendedproperty N'MS_Description', N'W = Womens, M = Mens, U = Universal', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'Style'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'Weight'))
EXEC sp_addextendedproperty N'MS_Description', N'Product weight.', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'Weight'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'WeightUnitMeasureCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Unit of measure for Weight column.', 'SCHEMA', N'Production', 'TABLE', N'Product', 'COLUMN', N'WeightUnitMeasureCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'CK_Product_Class'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [Class]=''h'' OR [Class]=''m'' OR [Class]=''l'' OR [Class]=''H'' OR [Class]=''M'' OR [Class]=''L'' OR [Class] IS NULL', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'CK_Product_Class'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'CK_Product_DaysToManufacture'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [DaysToManufacture] >= (0)', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'CK_Product_DaysToManufacture'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'CK_Product_ListPrice'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [ListPrice] >= (0.00)', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'CK_Product_ListPrice'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'CK_Product_ProductLine'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [ProductLine]=''r'' OR [ProductLine]=''m'' OR [ProductLine]=''t'' OR [ProductLine]=''s'' OR [ProductLine]=''R'' OR [ProductLine]=''M'' OR [ProductLine]=''T'' OR [ProductLine]=''S'' OR [ProductLine] IS NULL', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'CK_Product_ProductLine'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'CK_Product_ReorderPoint'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [ReorderPoint] > (0)', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'CK_Product_ReorderPoint'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'CK_Product_SafetyStockLevel'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [SafetyStockLevel] > (0)', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'CK_Product_SafetyStockLevel'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'CK_Product_SellEndDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [SellEndDate] >= [SellStartDate] OR [SellEndDate] IS NULL', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'CK_Product_SellEndDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'CK_Product_StandardCost'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [SafetyStockLevel] > (0)', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'CK_Product_StandardCost'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'CK_Product_Style'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [Style]=''u'' OR [Style]=''m'' OR [Style]=''w'' OR [Style]=''U'' OR [Style]=''M'' OR [Style]=''W'' OR [Style] IS NULL', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'CK_Product_Style'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'CK_Product_Weight'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [Weight] > (0.00)', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'CK_Product_Weight'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'DF_Product_FinishedGoodsFlag'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of  1', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'DF_Product_FinishedGoodsFlag'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'DF_Product_MakeFlag'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of  1', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'DF_Product_MakeFlag'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'DF_Product_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'DF_Product_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'DF_Product_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'DF_Product_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'FK_Product_ProductModel_ProductModelID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing ProductModel.ProductModelID.', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'FK_Product_ProductModel_ProductModelID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'FK_Product_ProductSubcategory_ProductSubcategoryID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing ProductSubcategory.ProductSubcategoryID.', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'FK_Product_ProductSubcategory_ProductSubcategoryID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'FK_Product_UnitMeasure_SizeUnitMeasureCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing UnitMeasure.UnitMeasureCode.', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'FK_Product_UnitMeasure_SizeUnitMeasureCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'FK_Product_UnitMeasure_WeightUnitMeasureCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing UnitMeasure.UnitMeasureCode.', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'FK_Product_UnitMeasure_WeightUnitMeasureCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'PK_Product_ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Production', 'TABLE', N'Product', 'CONSTRAINT', N'PK_Product_ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'INDEX', N'AK_Product_Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'Production', 'TABLE', N'Product', 'INDEX', N'AK_Product_Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'INDEX', N'AK_Product_ProductNumber'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'Production', 'TABLE', N'Product', 'INDEX', N'AK_Product_ProductNumber'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'INDEX', N'AK_Product_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index. Used to support replication samples.', 'SCHEMA', N'Production', 'TABLE', N'Product', 'INDEX', N'AK_Product_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'Product', 'INDEX', N'PK_Product_ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Production', 'TABLE', N'Product', 'INDEX', N'PK_Product_ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ScrapReason', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Manufacturing failure reasons lookup table.', 'SCHEMA', N'Production', 'TABLE', N'ScrapReason', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ScrapReason', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Production', 'TABLE', N'ScrapReason', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ScrapReason', 'COLUMN', N'Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Failure description.', 'SCHEMA', N'Production', 'TABLE', N'ScrapReason', 'COLUMN', N'Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ScrapReason', 'COLUMN', N'ScrapReasonID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for ScrapReason records.', 'SCHEMA', N'Production', 'TABLE', N'ScrapReason', 'COLUMN', N'ScrapReasonID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ScrapReason', 'CONSTRAINT', N'DF_ScrapReason_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'ScrapReason', 'CONSTRAINT', N'DF_ScrapReason_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ScrapReason', 'CONSTRAINT', N'PK_ScrapReason_ScrapReasonID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Production', 'TABLE', N'ScrapReason', 'CONSTRAINT', N'PK_ScrapReason_ScrapReasonID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ScrapReason', 'INDEX', N'AK_ScrapReason_Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'Production', 'TABLE', N'ScrapReason', 'INDEX', N'AK_ScrapReason_Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'ScrapReason', 'INDEX', N'PK_ScrapReason_ScrapReasonID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Production', 'TABLE', N'ScrapReason', 'INDEX', N'PK_ScrapReason_ScrapReasonID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Transactions for previous years.', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'COLUMN', N'ActualCost'))
EXEC sp_addextendedproperty N'MS_Description', N'Product cost.', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'COLUMN', N'ActualCost'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'COLUMN', N'ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Product identification number. Foreign key to Product.ProductID.', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'COLUMN', N'ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'COLUMN', N'Quantity'))
EXEC sp_addextendedproperty N'MS_Description', N'Product quantity.', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'COLUMN', N'Quantity'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'COLUMN', N'ReferenceOrderID'))
EXEC sp_addextendedproperty N'MS_Description', N'Purchase order, sales order, or work order identification number.', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'COLUMN', N'ReferenceOrderID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'COLUMN', N'ReferenceOrderLineID'))
EXEC sp_addextendedproperty N'MS_Description', N'Line number associated with the purchase order, sales order, or work order.', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'COLUMN', N'ReferenceOrderLineID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'COLUMN', N'TransactionDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time of the transaction.', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'COLUMN', N'TransactionDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'COLUMN', N'TransactionID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for TransactionHistoryArchive records.', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'COLUMN', N'TransactionID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'COLUMN', N'TransactionType'))
EXEC sp_addextendedproperty N'MS_Description', N'W = Work Order, S = Sales Order, P = Purchase Order', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'COLUMN', N'TransactionType'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'CONSTRAINT', N'CK_TransactionHistoryArchive_TransactionType'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [TransactionType]=''p'' OR [TransactionType]=''s'' OR [TransactionType]=''w'' OR [TransactionType]=''P'' OR [TransactionType]=''S'' OR [TransactionType]=''W''', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'CONSTRAINT', N'CK_TransactionHistoryArchive_TransactionType'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'CONSTRAINT', N'DF_TransactionHistoryArchive_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'CONSTRAINT', N'DF_TransactionHistoryArchive_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'CONSTRAINT', N'DF_TransactionHistoryArchive_ReferenceOrderLineID'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'CONSTRAINT', N'DF_TransactionHistoryArchive_ReferenceOrderLineID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'CONSTRAINT', N'DF_TransactionHistoryArchive_TransactionDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'CONSTRAINT', N'DF_TransactionHistoryArchive_TransactionDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'CONSTRAINT', N'PK_TransactionHistoryArchive_TransactionID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'CONSTRAINT', N'PK_TransactionHistoryArchive_TransactionID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'INDEX', N'IX_TransactionHistoryArchive_ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'INDEX', N'IX_TransactionHistoryArchive_ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'INDEX', N'IX_TransactionHistoryArchive_ReferenceOrderID_ReferenceOrderLineID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'INDEX', N'IX_TransactionHistoryArchive_ReferenceOrderID_ReferenceOrderLineID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'INDEX', N'PK_TransactionHistoryArchive_TransactionID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistoryArchive', 'INDEX', N'PK_TransactionHistoryArchive_TransactionID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Record of each purchase order, sales order, or work order transaction year to date.', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'COLUMN', N'ActualCost'))
EXEC sp_addextendedproperty N'MS_Description', N'Product cost.', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'COLUMN', N'ActualCost'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'COLUMN', N'ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Product identification number. Foreign key to Product.ProductID.', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'COLUMN', N'ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'COLUMN', N'Quantity'))
EXEC sp_addextendedproperty N'MS_Description', N'Product quantity.', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'COLUMN', N'Quantity'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'COLUMN', N'ReferenceOrderID'))
EXEC sp_addextendedproperty N'MS_Description', N'Purchase order, sales order, or work order identification number.', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'COLUMN', N'ReferenceOrderID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'COLUMN', N'ReferenceOrderLineID'))
EXEC sp_addextendedproperty N'MS_Description', N'Line number associated with the purchase order, sales order, or work order.', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'COLUMN', N'ReferenceOrderLineID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'COLUMN', N'TransactionDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time of the transaction.', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'COLUMN', N'TransactionDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'COLUMN', N'TransactionID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for TransactionHistory records.', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'COLUMN', N'TransactionID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'COLUMN', N'TransactionType'))
EXEC sp_addextendedproperty N'MS_Description', N'W = WorkOrder, S = SalesOrder, P = PurchaseOrder', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'COLUMN', N'TransactionType'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'CONSTRAINT', N'CK_TransactionHistory_TransactionType'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [TransactionType]=''p'' OR [TransactionType]=''s'' OR [TransactionType]=''w'' OR [TransactionType]=''P'' OR [TransactionType]=''S'' OR [TransactionType]=''W'')', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'CONSTRAINT', N'CK_TransactionHistory_TransactionType'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'CONSTRAINT', N'DF_TransactionHistory_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'CONSTRAINT', N'DF_TransactionHistory_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'CONSTRAINT', N'DF_TransactionHistory_ReferenceOrderLineID'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'CONSTRAINT', N'DF_TransactionHistory_ReferenceOrderLineID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'CONSTRAINT', N'DF_TransactionHistory_TransactionDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'CONSTRAINT', N'DF_TransactionHistory_TransactionDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'CONSTRAINT', N'FK_TransactionHistory_Product_ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Product.ProductID.', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'CONSTRAINT', N'FK_TransactionHistory_Product_ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'CONSTRAINT', N'PK_TransactionHistory_TransactionID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'CONSTRAINT', N'PK_TransactionHistory_TransactionID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'INDEX', N'IX_TransactionHistory_ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'INDEX', N'IX_TransactionHistory_ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'INDEX', N'IX_TransactionHistory_ReferenceOrderID_ReferenceOrderLineID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'INDEX', N'IX_TransactionHistory_ReferenceOrderID_ReferenceOrderLineID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'INDEX', N'PK_TransactionHistory_TransactionID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Production', 'TABLE', N'TransactionHistory', 'INDEX', N'PK_TransactionHistory_TransactionID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'UnitMeasure', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Unit of measure lookup table.', 'SCHEMA', N'Production', 'TABLE', N'UnitMeasure', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'UnitMeasure', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Production', 'TABLE', N'UnitMeasure', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'UnitMeasure', 'COLUMN', N'Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Unit of measure description.', 'SCHEMA', N'Production', 'TABLE', N'UnitMeasure', 'COLUMN', N'Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'UnitMeasure', 'COLUMN', N'UnitMeasureCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key.', 'SCHEMA', N'Production', 'TABLE', N'UnitMeasure', 'COLUMN', N'UnitMeasureCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'UnitMeasure', 'CONSTRAINT', N'DF_UnitMeasure_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'UnitMeasure', 'CONSTRAINT', N'DF_UnitMeasure_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'UnitMeasure', 'CONSTRAINT', N'PK_UnitMeasure_UnitMeasureCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Production', 'TABLE', N'UnitMeasure', 'CONSTRAINT', N'PK_UnitMeasure_UnitMeasureCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'UnitMeasure', 'INDEX', N'AK_UnitMeasure_Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'Production', 'TABLE', N'UnitMeasure', 'INDEX', N'AK_UnitMeasure_Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'UnitMeasure', 'INDEX', N'PK_UnitMeasure_UnitMeasureCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Production', 'TABLE', N'UnitMeasure', 'INDEX', N'PK_UnitMeasure_UnitMeasureCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Work order details.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'COLUMN', N'ActualCost'))
EXEC sp_addextendedproperty N'MS_Description', N'Actual manufacturing cost.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'COLUMN', N'ActualCost'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'COLUMN', N'ActualEndDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Actual end date.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'COLUMN', N'ActualEndDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'COLUMN', N'ActualResourceHrs'))
EXEC sp_addextendedproperty N'MS_Description', N'Number of manufacturing hours used.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'COLUMN', N'ActualResourceHrs'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'COLUMN', N'ActualStartDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Actual start date.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'COLUMN', N'ActualStartDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'COLUMN', N'LocationID'))
EXEC sp_addextendedproperty N'MS_Description', N'Manufacturing location where the part is processed. Foreign key to Location.LocationID.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'COLUMN', N'LocationID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'COLUMN', N'OperationSequence'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key. Indicates the manufacturing process sequence.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'COLUMN', N'OperationSequence'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'COLUMN', N'PlannedCost'))
EXEC sp_addextendedproperty N'MS_Description', N'Estimated manufacturing cost.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'COLUMN', N'PlannedCost'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'COLUMN', N'ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key. Foreign key to Product.ProductID.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'COLUMN', N'ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'COLUMN', N'ScheduledEndDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Planned manufacturing end date.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'COLUMN', N'ScheduledEndDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'COLUMN', N'ScheduledStartDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Planned manufacturing start date.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'COLUMN', N'ScheduledStartDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'COLUMN', N'WorkOrderID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key. Foreign key to WorkOrder.WorkOrderID.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'COLUMN', N'WorkOrderID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'CONSTRAINT', N'CK_WorkOrderRouting_ActualCost'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [ActualCost] > (0.00)', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'CONSTRAINT', N'CK_WorkOrderRouting_ActualCost'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'CONSTRAINT', N'CK_WorkOrderRouting_ActualEndDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [ActualEndDate] >= [ActualStartDate] OR [ActualEndDate] IS NULL OR [ActualStartDate] IS NULL', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'CONSTRAINT', N'CK_WorkOrderRouting_ActualEndDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'CONSTRAINT', N'CK_WorkOrderRouting_ActualResourceHrs'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [ActualResourceHrs] >= (0.0000)', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'CONSTRAINT', N'CK_WorkOrderRouting_ActualResourceHrs'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'CONSTRAINT', N'CK_WorkOrderRouting_PlannedCost'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [PlannedCost] > (0.00)', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'CONSTRAINT', N'CK_WorkOrderRouting_PlannedCost'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'CONSTRAINT', N'CK_WorkOrderRouting_ScheduledEndDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [ScheduledEndDate] >= [ScheduledStartDate]', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'CONSTRAINT', N'CK_WorkOrderRouting_ScheduledEndDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'CONSTRAINT', N'DF_WorkOrderRouting_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'CONSTRAINT', N'DF_WorkOrderRouting_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'CONSTRAINT', N'FK_WorkOrderRouting_Location_LocationID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Location.LocationID.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'CONSTRAINT', N'FK_WorkOrderRouting_Location_LocationID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'CONSTRAINT', N'FK_WorkOrderRouting_WorkOrder_WorkOrderID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing WorkOrder.WorkOrderID.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'CONSTRAINT', N'FK_WorkOrderRouting_WorkOrder_WorkOrderID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'CONSTRAINT', N'PK_WorkOrderRouting_WorkOrderID_ProductID_OperationSequence'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'CONSTRAINT', N'PK_WorkOrderRouting_WorkOrderID_ProductID_OperationSequence'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'INDEX', N'IX_WorkOrderRouting_ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'INDEX', N'IX_WorkOrderRouting_ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'INDEX', N'PK_WorkOrderRouting_WorkOrderID_ProductID_OperationSequence'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrderRouting', 'INDEX', N'PK_WorkOrderRouting_WorkOrderID_ProductID_OperationSequence'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Manufacturing work orders.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'COLUMN', N'DueDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Work order due date.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'COLUMN', N'DueDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'COLUMN', N'EndDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Work order end date.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'COLUMN', N'EndDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'COLUMN', N'OrderQty'))
EXEC sp_addextendedproperty N'MS_Description', N'Product quantity to build.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'COLUMN', N'OrderQty'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'COLUMN', N'ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Product identification number. Foreign key to Product.ProductID.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'COLUMN', N'ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'COLUMN', N'ScrappedQty'))
EXEC sp_addextendedproperty N'MS_Description', N'Quantity that failed inspection.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'COLUMN', N'ScrappedQty'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'COLUMN', N'ScrapReasonID'))
EXEC sp_addextendedproperty N'MS_Description', N'Reason for inspection failure.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'COLUMN', N'ScrapReasonID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'COLUMN', N'StartDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Work order start date.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'COLUMN', N'StartDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'COLUMN', N'StockedQty'))
EXEC sp_addextendedproperty N'MS_Description', N'Quantity built and put in inventory.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'COLUMN', N'StockedQty'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'COLUMN', N'WorkOrderID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for WorkOrder records.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'COLUMN', N'WorkOrderID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'CONSTRAINT', N'CK_WorkOrder_EndDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [EndDate] >= [StartDate] OR [EndDate] IS NULL', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'CONSTRAINT', N'CK_WorkOrder_EndDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'CONSTRAINT', N'CK_WorkOrder_OrderQty'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [OrderQty] > (0)', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'CONSTRAINT', N'CK_WorkOrder_OrderQty'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'CONSTRAINT', N'CK_WorkOrder_ScrappedQty'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [ScrappedQty] >= (0)', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'CONSTRAINT', N'CK_WorkOrder_ScrappedQty'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'CONSTRAINT', N'DF_WorkOrder_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'CONSTRAINT', N'DF_WorkOrder_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'CONSTRAINT', N'FK_WorkOrder_Product_ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Product.ProductID.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'CONSTRAINT', N'FK_WorkOrder_Product_ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'CONSTRAINT', N'FK_WorkOrder_ScrapReason_ScrapReasonID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing ScrapReason.ScrapReasonID.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'CONSTRAINT', N'FK_WorkOrder_ScrapReason_ScrapReasonID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'CONSTRAINT', N'PK_WorkOrder_WorkOrderID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'CONSTRAINT', N'PK_WorkOrder_WorkOrderID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'INDEX', N'IX_WorkOrder_ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'INDEX', N'IX_WorkOrder_ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'INDEX', N'IX_WorkOrder_ScrapReasonID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'INDEX', N'IX_WorkOrder_ScrapReasonID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'INDEX', N'PK_WorkOrder_WorkOrderID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'INDEX', N'PK_WorkOrder_WorkOrderID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'TRIGGER', N'iWorkOrder'))
EXEC sp_addextendedproperty N'MS_Description', N'AFTER INSERT trigger that inserts a row in the TransactionHistory table.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'TRIGGER', N'iWorkOrder'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'TRIGGER', N'uWorkOrder'))
EXEC sp_addextendedproperty N'MS_Description', N'AFTER UPDATE trigger that inserts a row in the TransactionHistory table, updates ModifiedDate in the WorkOrder table.', 'SCHEMA', N'Production', 'TABLE', N'WorkOrder', 'TRIGGER', N'uWorkOrder'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Cross-reference table mapping vendors with the products they supply.', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'COLUMN', N'AverageLeadTime'))
EXEC sp_addextendedproperty N'MS_Description', N'The average span of time (in days) between placing an order with the vendor and receiving the purchased product.', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'COLUMN', N'AverageLeadTime'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'COLUMN', N'BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key. Foreign key to Vendor.BusinessEntityID.', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'COLUMN', N'BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'COLUMN', N'LastReceiptCost'))
EXEC sp_addextendedproperty N'MS_Description', N'The selling price when last purchased.', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'COLUMN', N'LastReceiptCost'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'COLUMN', N'LastReceiptDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date the product was last received by the vendor.', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'COLUMN', N'LastReceiptDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'COLUMN', N'MaxOrderQty'))
EXEC sp_addextendedproperty N'MS_Description', N'The minimum quantity that should be ordered.', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'COLUMN', N'MaxOrderQty'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'COLUMN', N'MinOrderQty'))
EXEC sp_addextendedproperty N'MS_Description', N'The maximum quantity that should be ordered.', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'COLUMN', N'MinOrderQty'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'COLUMN', N'OnOrderQty'))
EXEC sp_addextendedproperty N'MS_Description', N'The quantity currently on order.', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'COLUMN', N'OnOrderQty'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'COLUMN', N'ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key. Foreign key to Product.ProductID.', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'COLUMN', N'ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'COLUMN', N'StandardPrice'))
EXEC sp_addextendedproperty N'MS_Description', N'The vendor''s usual selling price.', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'COLUMN', N'StandardPrice'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'COLUMN', N'UnitMeasureCode'))
EXEC sp_addextendedproperty N'MS_Description', N'The product''s unit of measure.', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'COLUMN', N'UnitMeasureCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'CONSTRAINT', N'CK_ProductVendor_AverageLeadTime'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [AverageLeadTime] >= (1)', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'CONSTRAINT', N'CK_ProductVendor_AverageLeadTime'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'CONSTRAINT', N'CK_ProductVendor_LastReceiptCost'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [LastReceiptCost] > (0.00)', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'CONSTRAINT', N'CK_ProductVendor_LastReceiptCost'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'CONSTRAINT', N'CK_ProductVendor_MaxOrderQty'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [MaxOrderQty] >= (1)', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'CONSTRAINT', N'CK_ProductVendor_MaxOrderQty'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'CONSTRAINT', N'CK_ProductVendor_MinOrderQty'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [MinOrderQty] >= (1)', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'CONSTRAINT', N'CK_ProductVendor_MinOrderQty'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'CONSTRAINT', N'CK_ProductVendor_OnOrderQty'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [OnOrderQty] >= (0)', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'CONSTRAINT', N'CK_ProductVendor_OnOrderQty'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'CONSTRAINT', N'CK_ProductVendor_StandardPrice'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [StandardPrice] > (0.00)', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'CONSTRAINT', N'CK_ProductVendor_StandardPrice'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'CONSTRAINT', N'DF_ProductVendor_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'CONSTRAINT', N'DF_ProductVendor_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'CONSTRAINT', N'FK_ProductVendor_Product_ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Product.ProductID.', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'CONSTRAINT', N'FK_ProductVendor_Product_ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'CONSTRAINT', N'FK_ProductVendor_UnitMeasure_UnitMeasureCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing UnitMeasure.UnitMeasureCode.', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'CONSTRAINT', N'FK_ProductVendor_UnitMeasure_UnitMeasureCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'CONSTRAINT', N'FK_ProductVendor_Vendor_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Vendor.BusinessEntityID.', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'CONSTRAINT', N'FK_ProductVendor_Vendor_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'CONSTRAINT', N'PK_ProductVendor_ProductID_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'CONSTRAINT', N'PK_ProductVendor_ProductID_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'INDEX', N'IX_ProductVendor_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'INDEX', N'IX_ProductVendor_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'INDEX', N'IX_ProductVendor_UnitMeasureCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'INDEX', N'IX_ProductVendor_UnitMeasureCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'INDEX', N'PK_ProductVendor_ProductID_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Purchasing', 'TABLE', N'ProductVendor', 'INDEX', N'PK_ProductVendor_ProductID_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Individual products associated with a specific purchase order. See PurchaseOrderHeader.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'COLUMN', N'DueDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date the product is expected to be received.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'COLUMN', N'DueDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'COLUMN', N'LineTotal'))
EXEC sp_addextendedproperty N'MS_Description', N'Per product subtotal. Computed as OrderQty * UnitPrice.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'COLUMN', N'LineTotal'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'COLUMN', N'OrderQty'))
EXEC sp_addextendedproperty N'MS_Description', N'Quantity ordered.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'COLUMN', N'OrderQty'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'COLUMN', N'ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Product identification number. Foreign key to Product.ProductID.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'COLUMN', N'ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'COLUMN', N'PurchaseOrderDetailID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key. One line number per purchased product.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'COLUMN', N'PurchaseOrderDetailID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'COLUMN', N'PurchaseOrderID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key. Foreign key to PurchaseOrderHeader.PurchaseOrderID.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'COLUMN', N'PurchaseOrderID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'COLUMN', N'ReceivedQty'))
EXEC sp_addextendedproperty N'MS_Description', N'Quantity actually received from the vendor.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'COLUMN', N'ReceivedQty'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'COLUMN', N'RejectedQty'))
EXEC sp_addextendedproperty N'MS_Description', N'Quantity rejected during inspection.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'COLUMN', N'RejectedQty'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'COLUMN', N'StockedQty'))
EXEC sp_addextendedproperty N'MS_Description', N'Quantity accepted into inventory. Computed as ReceivedQty - RejectedQty.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'COLUMN', N'StockedQty'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'COLUMN', N'UnitPrice'))
EXEC sp_addextendedproperty N'MS_Description', N'Vendor''s selling price of a single product.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'COLUMN', N'UnitPrice'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'CONSTRAINT', N'CK_PurchaseOrderDetail_OrderQty'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [OrderQty] > (0)', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'CONSTRAINT', N'CK_PurchaseOrderDetail_OrderQty'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'CONSTRAINT', N'CK_PurchaseOrderDetail_ReceivedQty'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [ReceivedQty] >= (0.00)', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'CONSTRAINT', N'CK_PurchaseOrderDetail_ReceivedQty'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'CONSTRAINT', N'CK_PurchaseOrderDetail_RejectedQty'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [RejectedQty] >= (0.00)', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'CONSTRAINT', N'CK_PurchaseOrderDetail_RejectedQty'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'CONSTRAINT', N'CK_PurchaseOrderDetail_UnitPrice'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [UnitPrice] >= (0.00)', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'CONSTRAINT', N'CK_PurchaseOrderDetail_UnitPrice'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'CONSTRAINT', N'DF_PurchaseOrderDetail_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'CONSTRAINT', N'DF_PurchaseOrderDetail_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'CONSTRAINT', N'FK_PurchaseOrderDetail_Product_ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Product.ProductID.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'CONSTRAINT', N'FK_PurchaseOrderDetail_Product_ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'CONSTRAINT', N'FK_PurchaseOrderDetail_PurchaseOrderHeader_PurchaseOrderID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing PurchaseOrderHeader.PurchaseOrderID.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'CONSTRAINT', N'FK_PurchaseOrderDetail_PurchaseOrderHeader_PurchaseOrderID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'CONSTRAINT', N'PK_PurchaseOrderDetail_PurchaseOrderID_PurchaseOrderDetailID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'CONSTRAINT', N'PK_PurchaseOrderDetail_PurchaseOrderID_PurchaseOrderDetailID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'INDEX', N'IX_PurchaseOrderDetail_ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'INDEX', N'IX_PurchaseOrderDetail_ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'INDEX', N'PK_PurchaseOrderDetail_PurchaseOrderID_PurchaseOrderDetailID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'INDEX', N'PK_PurchaseOrderDetail_PurchaseOrderID_PurchaseOrderDetailID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'TRIGGER', N'iPurchaseOrderDetail'))
EXEC sp_addextendedproperty N'MS_Description', N'AFTER INSERT trigger that inserts a row in the TransactionHistory table and updates the PurchaseOrderHeader.SubTotal column.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'TRIGGER', N'iPurchaseOrderDetail'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'TRIGGER', N'uPurchaseOrderDetail'))
EXEC sp_addextendedproperty N'MS_Description', N'AFTER UPDATE trigger that inserts a row in the TransactionHistory table, updates ModifiedDate in PurchaseOrderDetail and updates the PurchaseOrderHeader.SubTotal column.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderDetail', 'TRIGGER', N'uPurchaseOrderDetail'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'General purchase order information. See PurchaseOrderDetail.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'COLUMN', N'EmployeeID'))
EXEC sp_addextendedproperty N'MS_Description', N'Employee who created the purchase order. Foreign key to Employee.BusinessEntityID.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'COLUMN', N'EmployeeID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'COLUMN', N'Freight'))
EXEC sp_addextendedproperty N'MS_Description', N'Shipping cost.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'COLUMN', N'Freight'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'COLUMN', N'OrderDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Purchase order creation date.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'COLUMN', N'OrderDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'COLUMN', N'PurchaseOrderID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'COLUMN', N'PurchaseOrderID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'COLUMN', N'RevisionNumber'))
EXEC sp_addextendedproperty N'MS_Description', N'Incremental number to track changes to the purchase order over time.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'COLUMN', N'RevisionNumber'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'COLUMN', N'ShipDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Estimated shipment date from the vendor.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'COLUMN', N'ShipDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'COLUMN', N'ShipMethodID'))
EXEC sp_addextendedproperty N'MS_Description', N'Shipping method. Foreign key to ShipMethod.ShipMethodID.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'COLUMN', N'ShipMethodID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'COLUMN', N'Status'))
EXEC sp_addextendedproperty N'MS_Description', N'Order current status. 1 = Pending; 2 = Approved; 3 = Rejected; 4 = Complete', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'COLUMN', N'Status'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'COLUMN', N'SubTotal'))
EXEC sp_addextendedproperty N'MS_Description', N'Purchase order subtotal. Computed as SUM(PurchaseOrderDetail.LineTotal)for the appropriate PurchaseOrderID.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'COLUMN', N'SubTotal'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'COLUMN', N'TaxAmt'))
EXEC sp_addextendedproperty N'MS_Description', N'Tax amount.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'COLUMN', N'TaxAmt'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'COLUMN', N'TotalDue'))
EXEC sp_addextendedproperty N'MS_Description', N'Total due to vendor. Computed as Subtotal + TaxAmt + Freight.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'COLUMN', N'TotalDue'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'COLUMN', N'VendorID'))
EXEC sp_addextendedproperty N'MS_Description', N'Vendor with whom the purchase order is placed. Foreign key to Vendor.BusinessEntityID.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'COLUMN', N'VendorID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'CK_PurchaseOrderHeader_Freight'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [Freight] >= (0.00)', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'CK_PurchaseOrderHeader_Freight'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'CK_PurchaseOrderHeader_ShipDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [ShipDate] >= [OrderDate] OR [ShipDate] IS NULL', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'CK_PurchaseOrderHeader_ShipDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'CK_PurchaseOrderHeader_Status'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [Status] BETWEEN (1) AND (4)', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'CK_PurchaseOrderHeader_Status'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'CK_PurchaseOrderHeader_SubTotal'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [SubTotal] >= (0.00)', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'CK_PurchaseOrderHeader_SubTotal'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'CK_PurchaseOrderHeader_TaxAmt'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [TaxAmt] >= (0.00)', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'CK_PurchaseOrderHeader_TaxAmt'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'DF_PurchaseOrderHeader_Freight'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0.0', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'DF_PurchaseOrderHeader_Freight'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'DF_PurchaseOrderHeader_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'DF_PurchaseOrderHeader_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'DF_PurchaseOrderHeader_OrderDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'DF_PurchaseOrderHeader_OrderDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'DF_PurchaseOrderHeader_RevisionNumber'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'DF_PurchaseOrderHeader_RevisionNumber'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'DF_PurchaseOrderHeader_Status'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 1', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'DF_PurchaseOrderHeader_Status'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'DF_PurchaseOrderHeader_SubTotal'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0.0', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'DF_PurchaseOrderHeader_SubTotal'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'DF_PurchaseOrderHeader_TaxAmt'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0.0', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'DF_PurchaseOrderHeader_TaxAmt'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'FK_PurchaseOrderHeader_Employee_EmployeeID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Employee.EmployeeID.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'FK_PurchaseOrderHeader_Employee_EmployeeID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'FK_PurchaseOrderHeader_ShipMethod_ShipMethodID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing ShipMethod.ShipMethodID.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'FK_PurchaseOrderHeader_ShipMethod_ShipMethodID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'FK_PurchaseOrderHeader_Vendor_VendorID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Vendor.VendorID.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'FK_PurchaseOrderHeader_Vendor_VendorID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'PK_PurchaseOrderHeader_PurchaseOrderID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'CONSTRAINT', N'PK_PurchaseOrderHeader_PurchaseOrderID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'INDEX', N'IX_PurchaseOrderHeader_EmployeeID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'INDEX', N'IX_PurchaseOrderHeader_EmployeeID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'INDEX', N'IX_PurchaseOrderHeader_VendorID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'INDEX', N'IX_PurchaseOrderHeader_VendorID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'INDEX', N'PK_PurchaseOrderHeader_PurchaseOrderID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'INDEX', N'PK_PurchaseOrderHeader_PurchaseOrderID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'TRIGGER', N'uPurchaseOrderHeader'))
EXEC sp_addextendedproperty N'MS_Description', N'AFTER UPDATE trigger that updates the RevisionNumber and ModifiedDate columns in the PurchaseOrderHeader table.', 'SCHEMA', N'Purchasing', 'TABLE', N'PurchaseOrderHeader', 'TRIGGER', N'uPurchaseOrderHeader'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Shipping company lookup table.', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'COLUMN', N'Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Shipping company name.', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'COLUMN', N'Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'COLUMN', N'ShipBase'))
EXEC sp_addextendedproperty N'MS_Description', N'Minimum shipping charge.', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'COLUMN', N'ShipBase'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'COLUMN', N'ShipMethodID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for ShipMethod records.', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'COLUMN', N'ShipMethodID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'COLUMN', N'ShipRate'))
EXEC sp_addextendedproperty N'MS_Description', N'Shipping charge per pound.', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'COLUMN', N'ShipRate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'CONSTRAINT', N'CK_ShipMethod_ShipBase'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [ShipBase] > (0.00)', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'CONSTRAINT', N'CK_ShipMethod_ShipBase'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'CONSTRAINT', N'CK_ShipMethod_ShipRate'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [ShipRate] > (0.00)', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'CONSTRAINT', N'CK_ShipMethod_ShipRate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'CONSTRAINT', N'DF_ShipMethod_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'CONSTRAINT', N'DF_ShipMethod_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'CONSTRAINT', N'DF_ShipMethod_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'CONSTRAINT', N'DF_ShipMethod_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'CONSTRAINT', N'DF_ShipMethod_ShipBase'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0.0', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'CONSTRAINT', N'DF_ShipMethod_ShipBase'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'CONSTRAINT', N'DF_ShipMethod_ShipRate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0.0', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'CONSTRAINT', N'DF_ShipMethod_ShipRate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'CONSTRAINT', N'PK_ShipMethod_ShipMethodID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'CONSTRAINT', N'PK_ShipMethod_ShipMethodID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'INDEX', N'AK_ShipMethod_Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'INDEX', N'AK_ShipMethod_Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'INDEX', N'AK_ShipMethod_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index. Used to support replication samples.', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'INDEX', N'AK_ShipMethod_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'INDEX', N'PK_ShipMethod_ShipMethodID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Purchasing', 'TABLE', N'ShipMethod', 'INDEX', N'PK_ShipMethod_ShipMethodID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Companies from whom Adventure Works Cycles purchases parts or other goods.', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'COLUMN', N'AccountNumber'))
EXEC sp_addextendedproperty N'MS_Description', N'Vendor account (identification) number.', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'COLUMN', N'AccountNumber'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'COLUMN', N'ActiveFlag'))
EXEC sp_addextendedproperty N'MS_Description', N'0 = Vendor no longer used. 1 = Vendor is actively used.', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'COLUMN', N'ActiveFlag'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'COLUMN', N'BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for Vendor records.  Foreign key to BusinessEntity.BusinessEntityID', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'COLUMN', N'BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'COLUMN', N'CreditRating'))
EXEC sp_addextendedproperty N'MS_Description', N'1 = Superior, 2 = Excellent, 3 = Above average, 4 = Average, 5 = Below average', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'COLUMN', N'CreditRating'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'COLUMN', N'Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Company name.', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'COLUMN', N'Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'COLUMN', N'PreferredVendorStatus'))
EXEC sp_addextendedproperty N'MS_Description', N'0 = Do not use if another vendor is available. 1 = Preferred over other vendors supplying the same product.', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'COLUMN', N'PreferredVendorStatus'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'COLUMN', N'PurchasingWebServiceURL'))
EXEC sp_addextendedproperty N'MS_Description', N'Vendor URL.', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'COLUMN', N'PurchasingWebServiceURL'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'CONSTRAINT', N'CK_Vendor_CreditRating'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [CreditRating] BETWEEN (1) AND (5)', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'CONSTRAINT', N'CK_Vendor_CreditRating'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'CONSTRAINT', N'DF_Vendor_ActiveFlag'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 1 (TRUE)', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'CONSTRAINT', N'DF_Vendor_ActiveFlag'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'CONSTRAINT', N'DF_Vendor_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'CONSTRAINT', N'DF_Vendor_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'CONSTRAINT', N'DF_Vendor_PreferredVendorStatus'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 1 (TRUE)', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'CONSTRAINT', N'DF_Vendor_PreferredVendorStatus'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'CONSTRAINT', N'FK_Vendor_BusinessEntity_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing BusinessEntity.BusinessEntityID', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'CONSTRAINT', N'FK_Vendor_BusinessEntity_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'CONSTRAINT', N'PK_Vendor_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'CONSTRAINT', N'PK_Vendor_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'INDEX', N'AK_Vendor_AccountNumber'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'INDEX', N'AK_Vendor_AccountNumber'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'INDEX', N'PK_Vendor_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'INDEX', N'PK_Vendor_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'TRIGGER', N'dVendor'))
EXEC sp_addextendedproperty N'MS_Description', N'INSTEAD OF DELETE trigger which keeps Vendors from being deleted.', 'SCHEMA', N'Purchasing', 'TABLE', N'Vendor', 'TRIGGER', N'dVendor'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CountryRegionCurrency', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Cross-reference table mapping ISO currency codes to a country or region.', 'SCHEMA', N'Sales', 'TABLE', N'CountryRegionCurrency', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CountryRegionCurrency', 'COLUMN', N'CountryRegionCode'))
EXEC sp_addextendedproperty N'MS_Description', N'ISO code for countries and regions. Foreign key to CountryRegion.CountryRegionCode.', 'SCHEMA', N'Sales', 'TABLE', N'CountryRegionCurrency', 'COLUMN', N'CountryRegionCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CountryRegionCurrency', 'COLUMN', N'CurrencyCode'))
EXEC sp_addextendedproperty N'MS_Description', N'ISO standard currency code. Foreign key to Currency.CurrencyCode.', 'SCHEMA', N'Sales', 'TABLE', N'CountryRegionCurrency', 'COLUMN', N'CurrencyCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CountryRegionCurrency', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Sales', 'TABLE', N'CountryRegionCurrency', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CountryRegionCurrency', 'CONSTRAINT', N'DF_CountryRegionCurrency_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Sales', 'TABLE', N'CountryRegionCurrency', 'CONSTRAINT', N'DF_CountryRegionCurrency_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CountryRegionCurrency', 'CONSTRAINT', N'FK_CountryRegionCurrency_CountryRegion_CountryRegionCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing CountryRegion.CountryRegionCode.', 'SCHEMA', N'Sales', 'TABLE', N'CountryRegionCurrency', 'CONSTRAINT', N'FK_CountryRegionCurrency_CountryRegion_CountryRegionCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CountryRegionCurrency', 'CONSTRAINT', N'FK_CountryRegionCurrency_Currency_CurrencyCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Currency.CurrencyCode.', 'SCHEMA', N'Sales', 'TABLE', N'CountryRegionCurrency', 'CONSTRAINT', N'FK_CountryRegionCurrency_Currency_CurrencyCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CountryRegionCurrency', 'CONSTRAINT', N'PK_CountryRegionCurrency_CountryRegionCode_CurrencyCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Sales', 'TABLE', N'CountryRegionCurrency', 'CONSTRAINT', N'PK_CountryRegionCurrency_CountryRegionCode_CurrencyCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CountryRegionCurrency', 'INDEX', N'IX_CountryRegionCurrency_CurrencyCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Sales', 'TABLE', N'CountryRegionCurrency', 'INDEX', N'IX_CountryRegionCurrency_CurrencyCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CountryRegionCurrency', 'INDEX', N'PK_CountryRegionCurrency_CountryRegionCode_CurrencyCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Sales', 'TABLE', N'CountryRegionCurrency', 'INDEX', N'PK_CountryRegionCurrency_CountryRegionCode_CurrencyCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CreditCard', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Customer credit card information.', 'SCHEMA', N'Sales', 'TABLE', N'CreditCard', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CreditCard', 'COLUMN', N'CardNumber'))
EXEC sp_addextendedproperty N'MS_Description', N'Credit card number.', 'SCHEMA', N'Sales', 'TABLE', N'CreditCard', 'COLUMN', N'CardNumber'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CreditCard', 'COLUMN', N'CardType'))
EXEC sp_addextendedproperty N'MS_Description', N'Credit card name.', 'SCHEMA', N'Sales', 'TABLE', N'CreditCard', 'COLUMN', N'CardType'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CreditCard', 'COLUMN', N'CreditCardID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for CreditCard records.', 'SCHEMA', N'Sales', 'TABLE', N'CreditCard', 'COLUMN', N'CreditCardID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CreditCard', 'COLUMN', N'ExpMonth'))
EXEC sp_addextendedproperty N'MS_Description', N'Credit card expiration month.', 'SCHEMA', N'Sales', 'TABLE', N'CreditCard', 'COLUMN', N'ExpMonth'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CreditCard', 'COLUMN', N'ExpYear'))
EXEC sp_addextendedproperty N'MS_Description', N'Credit card expiration year.', 'SCHEMA', N'Sales', 'TABLE', N'CreditCard', 'COLUMN', N'ExpYear'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CreditCard', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Sales', 'TABLE', N'CreditCard', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CreditCard', 'CONSTRAINT', N'DF_CreditCard_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Sales', 'TABLE', N'CreditCard', 'CONSTRAINT', N'DF_CreditCard_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CreditCard', 'CONSTRAINT', N'PK_CreditCard_CreditCardID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Sales', 'TABLE', N'CreditCard', 'CONSTRAINT', N'PK_CreditCard_CreditCardID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CreditCard', 'INDEX', N'AK_CreditCard_CardNumber'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'Sales', 'TABLE', N'CreditCard', 'INDEX', N'AK_CreditCard_CardNumber'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CreditCard', 'INDEX', N'PK_CreditCard_CreditCardID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Sales', 'TABLE', N'CreditCard', 'INDEX', N'PK_CreditCard_CreditCardID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CurrencyRate', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Currency exchange rates.', 'SCHEMA', N'Sales', 'TABLE', N'CurrencyRate', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CurrencyRate', 'COLUMN', N'AverageRate'))
EXEC sp_addextendedproperty N'MS_Description', N'Average exchange rate for the day.', 'SCHEMA', N'Sales', 'TABLE', N'CurrencyRate', 'COLUMN', N'AverageRate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CurrencyRate', 'COLUMN', N'CurrencyRateDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the exchange rate was obtained.', 'SCHEMA', N'Sales', 'TABLE', N'CurrencyRate', 'COLUMN', N'CurrencyRateDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CurrencyRate', 'COLUMN', N'CurrencyRateID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for CurrencyRate records.', 'SCHEMA', N'Sales', 'TABLE', N'CurrencyRate', 'COLUMN', N'CurrencyRateID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CurrencyRate', 'COLUMN', N'EndOfDayRate'))
EXEC sp_addextendedproperty N'MS_Description', N'Final exchange rate for the day.', 'SCHEMA', N'Sales', 'TABLE', N'CurrencyRate', 'COLUMN', N'EndOfDayRate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CurrencyRate', 'COLUMN', N'FromCurrencyCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Exchange rate was converted from this currency code.', 'SCHEMA', N'Sales', 'TABLE', N'CurrencyRate', 'COLUMN', N'FromCurrencyCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CurrencyRate', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Sales', 'TABLE', N'CurrencyRate', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CurrencyRate', 'COLUMN', N'ToCurrencyCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Exchange rate was converted to this currency code.', 'SCHEMA', N'Sales', 'TABLE', N'CurrencyRate', 'COLUMN', N'ToCurrencyCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CurrencyRate', 'CONSTRAINT', N'DF_CurrencyRate_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Sales', 'TABLE', N'CurrencyRate', 'CONSTRAINT', N'DF_CurrencyRate_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CurrencyRate', 'CONSTRAINT', N'FK_CurrencyRate_Currency_FromCurrencyCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Currency.FromCurrencyCode.', 'SCHEMA', N'Sales', 'TABLE', N'CurrencyRate', 'CONSTRAINT', N'FK_CurrencyRate_Currency_FromCurrencyCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CurrencyRate', 'CONSTRAINT', N'FK_CurrencyRate_Currency_ToCurrencyCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Currency.ToCurrencyCode.', 'SCHEMA', N'Sales', 'TABLE', N'CurrencyRate', 'CONSTRAINT', N'FK_CurrencyRate_Currency_ToCurrencyCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CurrencyRate', 'CONSTRAINT', N'PK_CurrencyRate_CurrencyRateID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Sales', 'TABLE', N'CurrencyRate', 'CONSTRAINT', N'PK_CurrencyRate_CurrencyRateID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CurrencyRate', 'INDEX', N'AK_CurrencyRate_CurrencyRateDate_FromCurrencyCode_ToCurrencyCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'Sales', 'TABLE', N'CurrencyRate', 'INDEX', N'AK_CurrencyRate_CurrencyRateDate_FromCurrencyCode_ToCurrencyCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'CurrencyRate', 'INDEX', N'PK_CurrencyRate_CurrencyRateID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Sales', 'TABLE', N'CurrencyRate', 'INDEX', N'PK_CurrencyRate_CurrencyRateID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Currency', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Lookup table containing standard ISO currencies.', 'SCHEMA', N'Sales', 'TABLE', N'Currency', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Currency', 'COLUMN', N'CurrencyCode'))
EXEC sp_addextendedproperty N'MS_Description', N'The ISO code for the Currency.', 'SCHEMA', N'Sales', 'TABLE', N'Currency', 'COLUMN', N'CurrencyCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Currency', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Sales', 'TABLE', N'Currency', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Currency', 'COLUMN', N'Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Currency name.', 'SCHEMA', N'Sales', 'TABLE', N'Currency', 'COLUMN', N'Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Currency', 'CONSTRAINT', N'DF_Currency_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Sales', 'TABLE', N'Currency', 'CONSTRAINT', N'DF_Currency_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Currency', 'CONSTRAINT', N'PK_Currency_CurrencyCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Sales', 'TABLE', N'Currency', 'CONSTRAINT', N'PK_Currency_CurrencyCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Currency', 'INDEX', N'AK_Currency_Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'Sales', 'TABLE', N'Currency', 'INDEX', N'AK_Currency_Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Currency', 'INDEX', N'PK_Currency_CurrencyCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Sales', 'TABLE', N'Currency', 'INDEX', N'PK_Currency_CurrencyCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Customer', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Current customer information. Also see the Person and Store tables.', 'SCHEMA', N'Sales', 'TABLE', N'Customer', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'COLUMN', N'AccountNumber'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique number identifying the customer assigned by the accounting system.', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'COLUMN', N'AccountNumber'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'COLUMN', N'CustomerID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key.', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'COLUMN', N'CustomerID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'COLUMN', N'PersonID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key to Person.BusinessEntityID', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'COLUMN', N'PersonID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'COLUMN', N'StoreID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key to Store.BusinessEntityID', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'COLUMN', N'StoreID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'COLUMN', N'TerritoryID'))
EXEC sp_addextendedproperty N'MS_Description', N'ID of the territory in which the customer is located. Foreign key to SalesTerritory.SalesTerritoryID.', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'COLUMN', N'TerritoryID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'CONSTRAINT', N'DF_Customer_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'CONSTRAINT', N'DF_Customer_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'CONSTRAINT', N'DF_Customer_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'CONSTRAINT', N'DF_Customer_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'CONSTRAINT', N'FK_Customer_Person_PersonID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Person.BusinessEntityID.', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'CONSTRAINT', N'FK_Customer_Person_PersonID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'CONSTRAINT', N'FK_Customer_SalesTerritory_TerritoryID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing SalesTerritory.TerritoryID.', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'CONSTRAINT', N'FK_Customer_SalesTerritory_TerritoryID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'CONSTRAINT', N'FK_Customer_Store_StoreID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Store.BusinessEntityID.', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'CONSTRAINT', N'FK_Customer_Store_StoreID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'CONSTRAINT', N'PK_Customer_CustomerID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'CONSTRAINT', N'PK_Customer_CustomerID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'INDEX', N'AK_Customer_AccountNumber'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'INDEX', N'AK_Customer_AccountNumber'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'INDEX', N'AK_Customer_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index. Used to support replication samples.', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'INDEX', N'AK_Customer_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'INDEX', N'IX_Customer_TerritoryID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'INDEX', N'IX_Customer_TerritoryID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'INDEX', N'PK_Customer_CustomerID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Sales', 'TABLE', N'Customer', 'INDEX', N'PK_Customer_CustomerID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'OrderTracking', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Individual tracking events associated with a specific sales order.', 'SCHEMA', N'Sales', 'TABLE', N'OrderTracking', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'OrderTracking', 'COLUMN', N'CarrierTrackingNumber'))
EXEC sp_addextendedproperty N'MS_Description', N'Shipment tracking number supplied by the shipper.', 'SCHEMA', N'Sales', 'TABLE', N'OrderTracking', 'COLUMN', N'CarrierTrackingNumber'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'OrderTracking', 'COLUMN', N'EventDateTime'))
EXEC sp_addextendedproperty N'MS_Description', N'The date and time that a tracking event has occurred.', 'SCHEMA', N'Sales', 'TABLE', N'OrderTracking', 'COLUMN', N'EventDateTime'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'OrderTracking', 'COLUMN', N'EventDetails'))
EXEC sp_addextendedproperty N'MS_Description', N'Details for a delivery tracking event.', 'SCHEMA', N'Sales', 'TABLE', N'OrderTracking', 'COLUMN', N'EventDetails'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'OrderTracking', 'COLUMN', N'OrderTrackingID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key.', 'SCHEMA', N'Sales', 'TABLE', N'OrderTracking', 'COLUMN', N'OrderTrackingID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'OrderTracking', 'COLUMN', N'SalesOrderID'))
EXEC sp_addextendedproperty N'MS_Description', N'Sales order identification number.  Foreign key to SalesOrderHeader.SalesOrderID.', 'SCHEMA', N'Sales', 'TABLE', N'OrderTracking', 'COLUMN', N'SalesOrderID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'OrderTracking', 'COLUMN', N'TrackingEventID'))
EXEC sp_addextendedproperty N'MS_Description', N'Tracking delivery event for Order shipped to customer. Foreign key to TrackingEvent.TrackingEventID.', 'SCHEMA', N'Sales', 'TABLE', N'OrderTracking', 'COLUMN', N'TrackingEventID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'OrderTracking', 'CONSTRAINT', N'PK_OrderTracking'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Sales', 'TABLE', N'OrderTracking', 'CONSTRAINT', N'PK_OrderTracking'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'PersonCreditCard', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Cross-reference table mapping people to their credit card information in the CreditCard table. ', 'SCHEMA', N'Sales', 'TABLE', N'PersonCreditCard', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'PersonCreditCard', 'COLUMN', N'BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Business entity identification number. Foreign key to Person.BusinessEntityID.', 'SCHEMA', N'Sales', 'TABLE', N'PersonCreditCard', 'COLUMN', N'BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'PersonCreditCard', 'COLUMN', N'CreditCardID'))
EXEC sp_addextendedproperty N'MS_Description', N'Credit card identification number. Foreign key to CreditCard.CreditCardID.', 'SCHEMA', N'Sales', 'TABLE', N'PersonCreditCard', 'COLUMN', N'CreditCardID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'PersonCreditCard', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Sales', 'TABLE', N'PersonCreditCard', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'PersonCreditCard', 'CONSTRAINT', N'DF_PersonCreditCard_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Sales', 'TABLE', N'PersonCreditCard', 'CONSTRAINT', N'DF_PersonCreditCard_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'PersonCreditCard', 'CONSTRAINT', N'FK_PersonCreditCard_CreditCard_CreditCardID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing CreditCard.CreditCardID.', 'SCHEMA', N'Sales', 'TABLE', N'PersonCreditCard', 'CONSTRAINT', N'FK_PersonCreditCard_CreditCard_CreditCardID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'PersonCreditCard', 'CONSTRAINT', N'FK_PersonCreditCard_Person_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Person.BusinessEntityID.', 'SCHEMA', N'Sales', 'TABLE', N'PersonCreditCard', 'CONSTRAINT', N'FK_PersonCreditCard_Person_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'PersonCreditCard', 'CONSTRAINT', N'PK_PersonCreditCard_BusinessEntityID_CreditCardID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Sales', 'TABLE', N'PersonCreditCard', 'CONSTRAINT', N'PK_PersonCreditCard_BusinessEntityID_CreditCardID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'PersonCreditCard', 'INDEX', N'PK_PersonCreditCard_BusinessEntityID_CreditCardID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Sales', 'TABLE', N'PersonCreditCard', 'INDEX', N'PK_PersonCreditCard_BusinessEntityID_CreditCardID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Individual products associated with a specific sales order. See SalesOrderHeader.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'COLUMN', N'CarrierTrackingNumber'))
EXEC sp_addextendedproperty N'MS_Description', N'Shipment tracking number supplied by the shipper.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'COLUMN', N'CarrierTrackingNumber'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'COLUMN', N'LineTotal'))
EXEC sp_addextendedproperty N'MS_Description', N'Per product subtotal. Computed as UnitPrice * (1 - UnitPriceDiscount) * OrderQty.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'COLUMN', N'LineTotal'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'COLUMN', N'OrderQty'))
EXEC sp_addextendedproperty N'MS_Description', N'Quantity ordered per product.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'COLUMN', N'OrderQty'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'COLUMN', N'ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Product sold to customer. Foreign key to Product.ProductID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'COLUMN', N'ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'COLUMN', N'SalesOrderDetailID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key. One incremental unique number per product sold.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'COLUMN', N'SalesOrderDetailID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'COLUMN', N'SalesOrderID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key. Foreign key to SalesOrderHeader.SalesOrderID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'COLUMN', N'SalesOrderID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'COLUMN', N'SpecialOfferID'))
EXEC sp_addextendedproperty N'MS_Description', N'Promotional code. Foreign key to SpecialOffer.SpecialOfferID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'COLUMN', N'SpecialOfferID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'COLUMN', N'UnitPrice'))
EXEC sp_addextendedproperty N'MS_Description', N'Selling price of a single product.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'COLUMN', N'UnitPrice'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'COLUMN', N'UnitPriceDiscount'))
EXEC sp_addextendedproperty N'MS_Description', N'Discount amount.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'COLUMN', N'UnitPriceDiscount'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'CONSTRAINT', N'CK_SalesOrderDetail_OrderQty'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [OrderQty] > (0)', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'CONSTRAINT', N'CK_SalesOrderDetail_OrderQty'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'CONSTRAINT', N'CK_SalesOrderDetail_UnitPrice'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [UnitPrice] >= (0.00)', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'CONSTRAINT', N'CK_SalesOrderDetail_UnitPrice'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'CONSTRAINT', N'CK_SalesOrderDetail_UnitPriceDiscount'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [UnitPriceDiscount] >= (0.00)', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'CONSTRAINT', N'CK_SalesOrderDetail_UnitPriceDiscount'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'CONSTRAINT', N'DF_SalesOrderDetail_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'CONSTRAINT', N'DF_SalesOrderDetail_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'CONSTRAINT', N'DF_SalesOrderDetail_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'CONSTRAINT', N'DF_SalesOrderDetail_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'CONSTRAINT', N'DF_SalesOrderDetail_UnitPriceDiscount'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0.0', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'CONSTRAINT', N'DF_SalesOrderDetail_UnitPriceDiscount'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'CONSTRAINT', N'FK_SalesOrderDetail_SalesOrderHeader_SalesOrderID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing SalesOrderHeader.PurchaseOrderID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'CONSTRAINT', N'FK_SalesOrderDetail_SalesOrderHeader_SalesOrderID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'CONSTRAINT', N'FK_SalesOrderDetail_SpecialOfferProduct_SpecialOfferIDProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing SpecialOfferProduct.SpecialOfferIDProductID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'CONSTRAINT', N'FK_SalesOrderDetail_SpecialOfferProduct_SpecialOfferIDProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'CONSTRAINT', N'PK_SalesOrderDetail_SalesOrderID_SalesOrderDetailID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'CONSTRAINT', N'PK_SalesOrderDetail_SalesOrderID_SalesOrderDetailID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'INDEX', N'AK_SalesOrderDetail_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index. Used to support replication samples.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'INDEX', N'AK_SalesOrderDetail_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'INDEX', N'IX_SalesOrderDetail_ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'INDEX', N'IX_SalesOrderDetail_ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'INDEX', N'PK_SalesOrderDetail_SalesOrderID_SalesOrderDetailID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'INDEX', N'PK_SalesOrderDetail_SalesOrderID_SalesOrderDetailID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'TRIGGER', N'iduSalesOrderDetail'))
EXEC sp_addextendedproperty N'MS_Description', N'AFTER INSERT, DELETE, UPDATE trigger that inserts a row in the TransactionHistory table, updates ModifiedDate in SalesOrderDetail and updates the SalesOrderHeader.SubTotal column.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderDetail', 'TRIGGER', N'iduSalesOrderDetail'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeaderSalesReason', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Cross-reference table mapping sales orders to sales reason codes.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeaderSalesReason', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeaderSalesReason', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeaderSalesReason', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeaderSalesReason', 'COLUMN', N'SalesOrderID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key. Foreign key to SalesOrderHeader.SalesOrderID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeaderSalesReason', 'COLUMN', N'SalesOrderID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeaderSalesReason', 'COLUMN', N'SalesReasonID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key. Foreign key to SalesReason.SalesReasonID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeaderSalesReason', 'COLUMN', N'SalesReasonID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeaderSalesReason', 'CONSTRAINT', N'DF_SalesOrderHeaderSalesReason_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeaderSalesReason', 'CONSTRAINT', N'DF_SalesOrderHeaderSalesReason_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeaderSalesReason', 'CONSTRAINT', N'FK_SalesOrderHeaderSalesReason_SalesOrderHeader_SalesOrderID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing SalesOrderHeader.SalesOrderID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeaderSalesReason', 'CONSTRAINT', N'FK_SalesOrderHeaderSalesReason_SalesOrderHeader_SalesOrderID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeaderSalesReason', 'CONSTRAINT', N'FK_SalesOrderHeaderSalesReason_SalesReason_SalesReasonID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing SalesReason.SalesReasonID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeaderSalesReason', 'CONSTRAINT', N'FK_SalesOrderHeaderSalesReason_SalesReason_SalesReasonID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeaderSalesReason', 'CONSTRAINT', N'PK_SalesOrderHeaderSalesReason_SalesOrderID_SalesReasonID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeaderSalesReason', 'CONSTRAINT', N'PK_SalesOrderHeaderSalesReason_SalesOrderID_SalesReasonID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeaderSalesReason', 'INDEX', N'PK_SalesOrderHeaderSalesReason_SalesOrderID_SalesReasonID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeaderSalesReason', 'INDEX', N'PK_SalesOrderHeaderSalesReason_SalesOrderID_SalesReasonID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'General sales order information.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'AccountNumber'))
EXEC sp_addextendedproperty N'MS_Description', N'Financial accounting number reference.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'AccountNumber'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'BillToAddressID'))
EXEC sp_addextendedproperty N'MS_Description', N'Customer billing address. Foreign key to Address.AddressID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'BillToAddressID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'Comment'))
EXEC sp_addextendedproperty N'MS_Description', N'Sales representative comments.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'Comment'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'CreditCardApprovalCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Approval code provided by the credit card company.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'CreditCardApprovalCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'CreditCardID'))
EXEC sp_addextendedproperty N'MS_Description', N'Credit card identification number. Foreign key to CreditCard.CreditCardID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'CreditCardID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'CurrencyRateID'))
EXEC sp_addextendedproperty N'MS_Description', N'Currency exchange rate used. Foreign key to CurrencyRate.CurrencyRateID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'CurrencyRateID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'CustomerID'))
EXEC sp_addextendedproperty N'MS_Description', N'Customer identification number. Foreign key to Customer.BusinessEntityID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'CustomerID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'DueDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date the order is due to the customer.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'DueDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'Freight'))
EXEC sp_addextendedproperty N'MS_Description', N'Shipping cost.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'Freight'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'OnlineOrderFlag'))
EXEC sp_addextendedproperty N'MS_Description', N'0 = Order placed by sales person. 1 = Order placed online by customer.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'OnlineOrderFlag'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'OrderDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Dates the sales order was created.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'OrderDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'PurchaseOrderNumber'))
EXEC sp_addextendedproperty N'MS_Description', N'Customer purchase order number reference. ', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'PurchaseOrderNumber'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'RevisionNumber'))
EXEC sp_addextendedproperty N'MS_Description', N'Incremental number to track changes to the sales order over time.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'RevisionNumber'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'SalesOrderID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'SalesOrderID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'SalesOrderNumber'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique sales order identification number.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'SalesOrderNumber'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'SalesPersonID'))
EXEC sp_addextendedproperty N'MS_Description', N'Sales person who created the sales order. Foreign key to SalesPerson.BusinessEntityID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'SalesPersonID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'ShipDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date the order was shipped to the customer.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'ShipDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'ShipMethodID'))
EXEC sp_addextendedproperty N'MS_Description', N'Shipping method. Foreign key to ShipMethod.ShipMethodID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'ShipMethodID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'ShipToAddressID'))
EXEC sp_addextendedproperty N'MS_Description', N'Customer shipping address. Foreign key to Address.AddressID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'ShipToAddressID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'Status'))
EXEC sp_addextendedproperty N'MS_Description', N'Order current status. 1 = In process; 2 = Approved; 3 = Backordered; 4 = Rejected; 5 = Shipped; 6 = Cancelled', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'Status'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'SubTotal'))
EXEC sp_addextendedproperty N'MS_Description', N'Sales subtotal. Computed as SUM(SalesOrderDetail.LineTotal)for the appropriate SalesOrderID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'SubTotal'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'TaxAmt'))
EXEC sp_addextendedproperty N'MS_Description', N'Tax amount.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'TaxAmt'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'TerritoryID'))
EXEC sp_addextendedproperty N'MS_Description', N'Territory in which the sale was made. Foreign key to SalesTerritory.SalesTerritoryID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'TerritoryID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'TotalDue'))
EXEC sp_addextendedproperty N'MS_Description', N'Total due from customer. Computed as Subtotal + TaxAmt + Freight.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'COLUMN', N'TotalDue'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'CK_SalesOrderHeader_DueDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [DueDate] >= [OrderDate]', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'CK_SalesOrderHeader_DueDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'CK_SalesOrderHeader_Freight'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [Freight] >= (0.00)', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'CK_SalesOrderHeader_Freight'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'CK_SalesOrderHeader_ShipDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [ShipDate] >= [OrderDate] OR [ShipDate] IS NULL', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'CK_SalesOrderHeader_ShipDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'CK_SalesOrderHeader_Status'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [Status] BETWEEN (0) AND (8)', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'CK_SalesOrderHeader_Status'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'CK_SalesOrderHeader_SubTotal'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [SubTotal] >= (0.00)', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'CK_SalesOrderHeader_SubTotal'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'CK_SalesOrderHeader_TaxAmt'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [TaxAmt] >= (0.00)', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'CK_SalesOrderHeader_TaxAmt'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'DF_SalesOrderHeader_Freight'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0.0', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'DF_SalesOrderHeader_Freight'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'DF_SalesOrderHeader_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'DF_SalesOrderHeader_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'DF_SalesOrderHeader_OnlineOrderFlag'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 1 (TRUE)', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'DF_SalesOrderHeader_OnlineOrderFlag'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'DF_SalesOrderHeader_OrderDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'DF_SalesOrderHeader_OrderDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'DF_SalesOrderHeader_RevisionNumber'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'DF_SalesOrderHeader_RevisionNumber'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'DF_SalesOrderHeader_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'DF_SalesOrderHeader_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'DF_SalesOrderHeader_Status'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 1', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'DF_SalesOrderHeader_Status'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'DF_SalesOrderHeader_SubTotal'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0.0', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'DF_SalesOrderHeader_SubTotal'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'DF_SalesOrderHeader_TaxAmt'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0.0', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'DF_SalesOrderHeader_TaxAmt'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'FK_SalesOrderHeader_Address_BillToAddressID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Address.AddressID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'FK_SalesOrderHeader_Address_BillToAddressID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'FK_SalesOrderHeader_Address_ShipToAddressID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Address.AddressID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'FK_SalesOrderHeader_Address_ShipToAddressID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'FK_SalesOrderHeader_CreditCard_CreditCardID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing CreditCard.CreditCardID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'FK_SalesOrderHeader_CreditCard_CreditCardID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'FK_SalesOrderHeader_CurrencyRate_CurrencyRateID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing CurrencyRate.CurrencyRateID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'FK_SalesOrderHeader_CurrencyRate_CurrencyRateID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'FK_SalesOrderHeader_Customer_CustomerID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Customer.CustomerID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'FK_SalesOrderHeader_Customer_CustomerID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'FK_SalesOrderHeader_SalesPerson_SalesPersonID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing SalesPerson.SalesPersonID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'FK_SalesOrderHeader_SalesPerson_SalesPersonID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'FK_SalesOrderHeader_SalesTerritory_TerritoryID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing SalesTerritory.TerritoryID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'FK_SalesOrderHeader_SalesTerritory_TerritoryID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'FK_SalesOrderHeader_ShipMethod_ShipMethodID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing ShipMethod.ShipMethodID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'FK_SalesOrderHeader_ShipMethod_ShipMethodID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'PK_SalesOrderHeader_SalesOrderID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'CONSTRAINT', N'PK_SalesOrderHeader_SalesOrderID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'INDEX', N'AK_SalesOrderHeader_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index. Used to support replication samples.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'INDEX', N'AK_SalesOrderHeader_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'INDEX', N'AK_SalesOrderHeader_SalesOrderNumber'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'INDEX', N'AK_SalesOrderHeader_SalesOrderNumber'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'INDEX', N'IX_SalesOrderHeader_CustomerID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'INDEX', N'IX_SalesOrderHeader_CustomerID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'INDEX', N'IX_SalesOrderHeader_SalesPersonID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'INDEX', N'IX_SalesOrderHeader_SalesPersonID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'INDEX', N'PK_SalesOrderHeader_SalesOrderID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'INDEX', N'PK_SalesOrderHeader_SalesOrderID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'TRIGGER', N'uSalesOrderHeader'))
EXEC sp_addextendedproperty N'MS_Description', N'AFTER UPDATE trigger that updates the RevisionNumber and ModifiedDate columns in the SalesOrderHeader table.Updates the SalesYTD column in the SalesPerson and SalesTerritory tables.', 'SCHEMA', N'Sales', 'TABLE', N'SalesOrderHeader', 'TRIGGER', N'uSalesOrderHeader'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPersonQuotaHistory', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Sales performance tracking.', 'SCHEMA', N'Sales', 'TABLE', N'SalesPersonQuotaHistory', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPersonQuotaHistory', 'COLUMN', N'BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Sales person identification number. Foreign key to SalesPerson.BusinessEntityID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesPersonQuotaHistory', 'COLUMN', N'BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPersonQuotaHistory', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Sales', 'TABLE', N'SalesPersonQuotaHistory', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPersonQuotaHistory', 'COLUMN', N'QuotaDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Sales quota date.', 'SCHEMA', N'Sales', 'TABLE', N'SalesPersonQuotaHistory', 'COLUMN', N'QuotaDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPersonQuotaHistory', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', 'SCHEMA', N'Sales', 'TABLE', N'SalesPersonQuotaHistory', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPersonQuotaHistory', 'COLUMN', N'SalesQuota'))
EXEC sp_addextendedproperty N'MS_Description', N'Sales quota amount.', 'SCHEMA', N'Sales', 'TABLE', N'SalesPersonQuotaHistory', 'COLUMN', N'SalesQuota'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPersonQuotaHistory', 'CONSTRAINT', N'CK_SalesPersonQuotaHistory_SalesQuota'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [SalesQuota] > (0.00)', 'SCHEMA', N'Sales', 'TABLE', N'SalesPersonQuotaHistory', 'CONSTRAINT', N'CK_SalesPersonQuotaHistory_SalesQuota'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPersonQuotaHistory', 'CONSTRAINT', N'DF_SalesPersonQuotaHistory_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Sales', 'TABLE', N'SalesPersonQuotaHistory', 'CONSTRAINT', N'DF_SalesPersonQuotaHistory_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPersonQuotaHistory', 'CONSTRAINT', N'DF_SalesPersonQuotaHistory_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()', 'SCHEMA', N'Sales', 'TABLE', N'SalesPersonQuotaHistory', 'CONSTRAINT', N'DF_SalesPersonQuotaHistory_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPersonQuotaHistory', 'CONSTRAINT', N'FK_SalesPersonQuotaHistory_SalesPerson_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing SalesPerson.SalesPersonID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesPersonQuotaHistory', 'CONSTRAINT', N'FK_SalesPersonQuotaHistory_SalesPerson_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPersonQuotaHistory', 'CONSTRAINT', N'PK_SalesPersonQuotaHistory_BusinessEntityID_QuotaDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Sales', 'TABLE', N'SalesPersonQuotaHistory', 'CONSTRAINT', N'PK_SalesPersonQuotaHistory_BusinessEntityID_QuotaDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPersonQuotaHistory', 'INDEX', N'AK_SalesPersonQuotaHistory_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index. Used to support replication samples.', 'SCHEMA', N'Sales', 'TABLE', N'SalesPersonQuotaHistory', 'INDEX', N'AK_SalesPersonQuotaHistory_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPersonQuotaHistory', 'INDEX', N'PK_SalesPersonQuotaHistory_BusinessEntityID_QuotaDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Sales', 'TABLE', N'SalesPersonQuotaHistory', 'INDEX', N'PK_SalesPersonQuotaHistory_BusinessEntityID_QuotaDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Sales representative current information.', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'COLUMN', N'Bonus'))
EXEC sp_addextendedproperty N'MS_Description', N'Bonus due if quota is met.', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'COLUMN', N'Bonus'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'COLUMN', N'BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for SalesPerson records. Foreign key to Employee.BusinessEntityID', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'COLUMN', N'BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'COLUMN', N'CommissionPct'))
EXEC sp_addextendedproperty N'MS_Description', N'Commision percent received per sale.', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'COLUMN', N'CommissionPct'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'COLUMN', N'SalesLastYear'))
EXEC sp_addextendedproperty N'MS_Description', N'Sales total of previous year.', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'COLUMN', N'SalesLastYear'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'COLUMN', N'SalesQuota'))
EXEC sp_addextendedproperty N'MS_Description', N'Projected yearly sales.', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'COLUMN', N'SalesQuota'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'COLUMN', N'SalesYTD'))
EXEC sp_addextendedproperty N'MS_Description', N'Sales total year to date.', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'COLUMN', N'SalesYTD'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'COLUMN', N'TerritoryID'))
EXEC sp_addextendedproperty N'MS_Description', N'Territory currently assigned to. Foreign key to SalesTerritory.SalesTerritoryID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'COLUMN', N'TerritoryID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'CONSTRAINT', N'CK_SalesPerson_Bonus'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [Bonus] >= (0.00)', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'CONSTRAINT', N'CK_SalesPerson_Bonus'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'CONSTRAINT', N'CK_SalesPerson_CommissionPct'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [CommissionPct] >= (0.00)', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'CONSTRAINT', N'CK_SalesPerson_CommissionPct'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'CONSTRAINT', N'CK_SalesPerson_SalesLastYear'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [SalesLastYear] >= (0.00)', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'CONSTRAINT', N'CK_SalesPerson_SalesLastYear'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'CONSTRAINT', N'CK_SalesPerson_SalesQuota'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [SalesQuota] > (0.00)', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'CONSTRAINT', N'CK_SalesPerson_SalesQuota'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'CONSTRAINT', N'CK_SalesPerson_SalesYTD'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [SalesYTD] >= (0.00)', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'CONSTRAINT', N'CK_SalesPerson_SalesYTD'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'CONSTRAINT', N'DF_SalesPerson_Bonus'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0.0', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'CONSTRAINT', N'DF_SalesPerson_Bonus'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'CONSTRAINT', N'DF_SalesPerson_CommissionPct'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0.0', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'CONSTRAINT', N'DF_SalesPerson_CommissionPct'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'CONSTRAINT', N'DF_SalesPerson_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'CONSTRAINT', N'DF_SalesPerson_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'CONSTRAINT', N'DF_SalesPerson_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'CONSTRAINT', N'DF_SalesPerson_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'CONSTRAINT', N'DF_SalesPerson_SalesLastYear'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0.0', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'CONSTRAINT', N'DF_SalesPerson_SalesLastYear'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'CONSTRAINT', N'DF_SalesPerson_SalesYTD'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0.0', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'CONSTRAINT', N'DF_SalesPerson_SalesYTD'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'CONSTRAINT', N'FK_SalesPerson_Employee_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Employee.EmployeeID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'CONSTRAINT', N'FK_SalesPerson_Employee_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'CONSTRAINT', N'FK_SalesPerson_SalesTerritory_TerritoryID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing SalesTerritory.TerritoryID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'CONSTRAINT', N'FK_SalesPerson_SalesTerritory_TerritoryID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'CONSTRAINT', N'PK_SalesPerson_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'CONSTRAINT', N'PK_SalesPerson_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'INDEX', N'AK_SalesPerson_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index. Used to support replication samples.', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'INDEX', N'AK_SalesPerson_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'INDEX', N'PK_SalesPerson_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Sales', 'TABLE', N'SalesPerson', 'INDEX', N'PK_SalesPerson_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesReason', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Lookup table of customer purchase reasons.', 'SCHEMA', N'Sales', 'TABLE', N'SalesReason', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesReason', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Sales', 'TABLE', N'SalesReason', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesReason', 'COLUMN', N'Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Sales reason description.', 'SCHEMA', N'Sales', 'TABLE', N'SalesReason', 'COLUMN', N'Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesReason', 'COLUMN', N'ReasonType'))
EXEC sp_addextendedproperty N'MS_Description', N'Category the sales reason belongs to.', 'SCHEMA', N'Sales', 'TABLE', N'SalesReason', 'COLUMN', N'ReasonType'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesReason', 'COLUMN', N'SalesReasonID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for SalesReason records.', 'SCHEMA', N'Sales', 'TABLE', N'SalesReason', 'COLUMN', N'SalesReasonID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesReason', 'CONSTRAINT', N'DF_SalesReason_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Sales', 'TABLE', N'SalesReason', 'CONSTRAINT', N'DF_SalesReason_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesReason', 'CONSTRAINT', N'PK_SalesReason_SalesReasonID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Sales', 'TABLE', N'SalesReason', 'CONSTRAINT', N'PK_SalesReason_SalesReasonID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesReason', 'INDEX', N'PK_SalesReason_SalesReasonID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Sales', 'TABLE', N'SalesReason', 'INDEX', N'PK_SalesReason_SalesReasonID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Tax rate lookup table.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'COLUMN', N'Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Tax rate description.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'COLUMN', N'Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'COLUMN', N'SalesTaxRateID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for SalesTaxRate records.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'COLUMN', N'SalesTaxRateID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'COLUMN', N'StateProvinceID'))
EXEC sp_addextendedproperty N'MS_Description', N'State, province, or country/region the sales tax applies to.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'COLUMN', N'StateProvinceID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'COLUMN', N'TaxRate'))
EXEC sp_addextendedproperty N'MS_Description', N'Tax rate amount.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'COLUMN', N'TaxRate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'COLUMN', N'TaxType'))
EXEC sp_addextendedproperty N'MS_Description', N'1 = Tax applied to retail transactions, 2 = Tax applied to wholesale transactions, 3 = Tax applied to all sales (retail and wholesale) transactions.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'COLUMN', N'TaxType'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'CONSTRAINT', N'CK_SalesTaxRate_TaxType'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [TaxType] BETWEEN (1) AND (3)', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'CONSTRAINT', N'CK_SalesTaxRate_TaxType'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'CONSTRAINT', N'DF_SalesTaxRate_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'CONSTRAINT', N'DF_SalesTaxRate_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'CONSTRAINT', N'DF_SalesTaxRate_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'CONSTRAINT', N'DF_SalesTaxRate_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'CONSTRAINT', N'DF_SalesTaxRate_TaxRate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0.0', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'CONSTRAINT', N'DF_SalesTaxRate_TaxRate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'CONSTRAINT', N'FK_SalesTaxRate_StateProvince_StateProvinceID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing StateProvince.StateProvinceID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'CONSTRAINT', N'FK_SalesTaxRate_StateProvince_StateProvinceID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'CONSTRAINT', N'PK_SalesTaxRate_SalesTaxRateID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'CONSTRAINT', N'PK_SalesTaxRate_SalesTaxRateID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'INDEX', N'AK_SalesTaxRate_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index. Used to support replication samples.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'INDEX', N'AK_SalesTaxRate_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'INDEX', N'AK_SalesTaxRate_StateProvinceID_TaxType'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'INDEX', N'AK_SalesTaxRate_StateProvinceID_TaxType'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'INDEX', N'PK_SalesTaxRate_SalesTaxRateID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTaxRate', 'INDEX', N'PK_SalesTaxRate_SalesTaxRateID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Sales representative transfers to other sales territories.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', 'COLUMN', N'BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key. The sales rep.  Foreign key to SalesPerson.BusinessEntityID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', 'COLUMN', N'BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', 'COLUMN', N'EndDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date the sales representative left work in the territory.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', 'COLUMN', N'EndDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', 'COLUMN', N'StartDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key. Date the sales representive started work in the territory.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', 'COLUMN', N'StartDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', 'COLUMN', N'TerritoryID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key. Territory identification number. Foreign key to SalesTerritory.SalesTerritoryID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', 'COLUMN', N'TerritoryID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', 'CONSTRAINT', N'CK_SalesTerritoryHistory_EndDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [EndDate] >= [StartDate] OR [EndDate] IS NULL', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', 'CONSTRAINT', N'CK_SalesTerritoryHistory_EndDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', 'CONSTRAINT', N'DF_SalesTerritoryHistory_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', 'CONSTRAINT', N'DF_SalesTerritoryHistory_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', 'CONSTRAINT', N'DF_SalesTerritoryHistory_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', 'CONSTRAINT', N'DF_SalesTerritoryHistory_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', 'CONSTRAINT', N'FK_SalesTerritoryHistory_SalesPerson_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing SalesPerson.SalesPersonID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', 'CONSTRAINT', N'FK_SalesTerritoryHistory_SalesPerson_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', 'CONSTRAINT', N'FK_SalesTerritoryHistory_SalesTerritory_TerritoryID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing SalesTerritory.TerritoryID.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', 'CONSTRAINT', N'FK_SalesTerritoryHistory_SalesTerritory_TerritoryID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', 'CONSTRAINT', N'PK_SalesTerritoryHistory_BusinessEntityID_StartDate_TerritoryID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', 'CONSTRAINT', N'PK_SalesTerritoryHistory_BusinessEntityID_StartDate_TerritoryID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', 'INDEX', N'AK_SalesTerritoryHistory_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index. Used to support replication samples.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', 'INDEX', N'AK_SalesTerritoryHistory_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', 'INDEX', N'PK_SalesTerritoryHistory_BusinessEntityID_StartDate_TerritoryID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritoryHistory', 'INDEX', N'PK_SalesTerritoryHistory_BusinessEntityID_StartDate_TerritoryID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Sales territory lookup table.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'COLUMN', N'CostLastYear'))
EXEC sp_addextendedproperty N'MS_Description', N'Business costs in the territory the previous year.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'COLUMN', N'CostLastYear'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'COLUMN', N'CostYTD'))
EXEC sp_addextendedproperty N'MS_Description', N'Business costs in the territory year to date.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'COLUMN', N'CostYTD'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'COLUMN', N'CountryRegionCode'))
EXEC sp_addextendedproperty N'MS_Description', N'ISO standard country or region code. Foreign key to CountryRegion.CountryRegionCode. ', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'COLUMN', N'CountryRegionCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'COLUMN', N'Group'))
EXEC sp_addextendedproperty N'MS_Description', N'Geographic area to which the sales territory belong.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'COLUMN', N'Group'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'COLUMN', N'Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Sales territory description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'COLUMN', N'Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'COLUMN', N'SalesLastYear'))
EXEC sp_addextendedproperty N'MS_Description', N'Sales in the territory the previous year.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'COLUMN', N'SalesLastYear'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'COLUMN', N'SalesYTD'))
EXEC sp_addextendedproperty N'MS_Description', N'Sales in the territory year to date.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'COLUMN', N'SalesYTD'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'COLUMN', N'TerritoryID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for SalesTerritory records.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'COLUMN', N'TerritoryID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'CONSTRAINT', N'CK_SalesTerritory_CostLastYear'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [CostLastYear] >= (0.00)', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'CONSTRAINT', N'CK_SalesTerritory_CostLastYear'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'CONSTRAINT', N'CK_SalesTerritory_CostYTD'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [CostYTD] >= (0.00)', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'CONSTRAINT', N'CK_SalesTerritory_CostYTD'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'CONSTRAINT', N'CK_SalesTerritory_SalesLastYear'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [SalesLastYear] >= (0.00)', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'CONSTRAINT', N'CK_SalesTerritory_SalesLastYear'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'CONSTRAINT', N'CK_SalesTerritory_SalesYTD'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [SalesYTD] >= (0.00)', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'CONSTRAINT', N'CK_SalesTerritory_SalesYTD'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'CONSTRAINT', N'DF_SalesTerritory_CostLastYear'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0.0', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'CONSTRAINT', N'DF_SalesTerritory_CostLastYear'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'CONSTRAINT', N'DF_SalesTerritory_CostYTD'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0.0', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'CONSTRAINT', N'DF_SalesTerritory_CostYTD'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'CONSTRAINT', N'DF_SalesTerritory_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'CONSTRAINT', N'DF_SalesTerritory_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'CONSTRAINT', N'DF_SalesTerritory_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'CONSTRAINT', N'DF_SalesTerritory_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'CONSTRAINT', N'DF_SalesTerritory_SalesLastYear'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0.0', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'CONSTRAINT', N'DF_SalesTerritory_SalesLastYear'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'CONSTRAINT', N'DF_SalesTerritory_SalesYTD'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0.0', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'CONSTRAINT', N'DF_SalesTerritory_SalesYTD'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'CONSTRAINT', N'FK_SalesTerritory_CountryRegion_CountryRegionCode'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing CountryRegion.CountryRegionCode.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'CONSTRAINT', N'FK_SalesTerritory_CountryRegion_CountryRegionCode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'CONSTRAINT', N'PK_SalesTerritory_TerritoryID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'CONSTRAINT', N'PK_SalesTerritory_TerritoryID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'INDEX', N'AK_SalesTerritory_Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'INDEX', N'AK_SalesTerritory_Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'INDEX', N'AK_SalesTerritory_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index. Used to support replication samples.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'INDEX', N'AK_SalesTerritory_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'INDEX', N'PK_SalesTerritory_TerritoryID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Sales', 'TABLE', N'SalesTerritory', 'INDEX', N'PK_SalesTerritory_TerritoryID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Contains online customer orders until the order is submitted or cancelled.', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', 'COLUMN', N'DateCreated'))
EXEC sp_addextendedproperty N'MS_Description', N'Date the time the record was created.', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', 'COLUMN', N'DateCreated'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', 'COLUMN', N'ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Product ordered. Foreign key to Product.ProductID.', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', 'COLUMN', N'ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', 'COLUMN', N'Quantity'))
EXEC sp_addextendedproperty N'MS_Description', N'Product quantity ordered.', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', 'COLUMN', N'Quantity'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', 'COLUMN', N'ShoppingCartID'))
EXEC sp_addextendedproperty N'MS_Description', N'Shopping cart identification number.', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', 'COLUMN', N'ShoppingCartID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', 'COLUMN', N'ShoppingCartItemID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for ShoppingCartItem records.', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', 'COLUMN', N'ShoppingCartItemID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', 'CONSTRAINT', N'CK_ShoppingCartItem_Quantity'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [Quantity] >= (1)', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', 'CONSTRAINT', N'CK_ShoppingCartItem_Quantity'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', 'CONSTRAINT', N'DF_ShoppingCartItem_DateCreated'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', 'CONSTRAINT', N'DF_ShoppingCartItem_DateCreated'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', 'CONSTRAINT', N'DF_ShoppingCartItem_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', 'CONSTRAINT', N'DF_ShoppingCartItem_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', 'CONSTRAINT', N'DF_ShoppingCartItem_Quantity'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 1', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', 'CONSTRAINT', N'DF_ShoppingCartItem_Quantity'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', 'CONSTRAINT', N'FK_ShoppingCartItem_Product_ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Product.ProductID.', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', 'CONSTRAINT', N'FK_ShoppingCartItem_Product_ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', 'CONSTRAINT', N'PK_ShoppingCartItem_ShoppingCartItemID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', 'CONSTRAINT', N'PK_ShoppingCartItem_ShoppingCartItemID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', 'INDEX', N'IX_ShoppingCartItem_ShoppingCartID_ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', 'INDEX', N'IX_ShoppingCartItem_ShoppingCartID_ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', 'INDEX', N'PK_ShoppingCartItem_ShoppingCartItemID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Sales', 'TABLE', N'ShoppingCartItem', 'INDEX', N'PK_ShoppingCartItem_ShoppingCartItemID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOfferProduct', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Cross-reference table mapping products to special offer discounts.', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOfferProduct', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOfferProduct', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOfferProduct', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOfferProduct', 'COLUMN', N'ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Product identification number. Foreign key to Product.ProductID.', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOfferProduct', 'COLUMN', N'ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOfferProduct', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOfferProduct', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOfferProduct', 'COLUMN', N'SpecialOfferID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for SpecialOfferProduct records.', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOfferProduct', 'COLUMN', N'SpecialOfferID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOfferProduct', 'CONSTRAINT', N'DF_SpecialOfferProduct_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOfferProduct', 'CONSTRAINT', N'DF_SpecialOfferProduct_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOfferProduct', 'CONSTRAINT', N'DF_SpecialOfferProduct_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOfferProduct', 'CONSTRAINT', N'DF_SpecialOfferProduct_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOfferProduct', 'CONSTRAINT', N'FK_SpecialOfferProduct_Product_ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing Product.ProductID.', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOfferProduct', 'CONSTRAINT', N'FK_SpecialOfferProduct_Product_ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOfferProduct', 'CONSTRAINT', N'FK_SpecialOfferProduct_SpecialOffer_SpecialOfferID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing SpecialOffer.SpecialOfferID.', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOfferProduct', 'CONSTRAINT', N'FK_SpecialOfferProduct_SpecialOffer_SpecialOfferID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOfferProduct', 'CONSTRAINT', N'PK_SpecialOfferProduct_SpecialOfferID_ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOfferProduct', 'CONSTRAINT', N'PK_SpecialOfferProduct_SpecialOfferID_ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOfferProduct', 'INDEX', N'AK_SpecialOfferProduct_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index. Used to support replication samples.', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOfferProduct', 'INDEX', N'AK_SpecialOfferProduct_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOfferProduct', 'INDEX', N'IX_SpecialOfferProduct_ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOfferProduct', 'INDEX', N'IX_SpecialOfferProduct_ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOfferProduct', 'INDEX', N'PK_SpecialOfferProduct_SpecialOfferID_ProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOfferProduct', 'INDEX', N'PK_SpecialOfferProduct_SpecialOfferID_ProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Sale discounts lookup table.', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'COLUMN', N'Category'))
EXEC sp_addextendedproperty N'MS_Description', N'Group the discount applies to such as Reseller or Customer.', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'COLUMN', N'Category'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'COLUMN', N'Description'))
EXEC sp_addextendedproperty N'MS_Description', N'Discount description.', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'COLUMN', N'Description'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'COLUMN', N'DiscountPct'))
EXEC sp_addextendedproperty N'MS_Description', N'Discount precentage.', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'COLUMN', N'DiscountPct'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'COLUMN', N'EndDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Discount end date.', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'COLUMN', N'EndDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'COLUMN', N'MaxQty'))
EXEC sp_addextendedproperty N'MS_Description', N'Maximum discount percent allowed.', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'COLUMN', N'MaxQty'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'COLUMN', N'MinQty'))
EXEC sp_addextendedproperty N'MS_Description', N'Minimum discount percent allowed.', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'COLUMN', N'MinQty'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'COLUMN', N'SpecialOfferID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for SpecialOffer records.', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'COLUMN', N'SpecialOfferID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'COLUMN', N'StartDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Discount start date.', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'COLUMN', N'StartDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'COLUMN', N'Type'))
EXEC sp_addextendedproperty N'MS_Description', N'Discount type category.', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'COLUMN', N'Type'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'CONSTRAINT', N'CK_SpecialOffer_DiscountPct'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [DiscountPct] >= (0.00)', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'CONSTRAINT', N'CK_SpecialOffer_DiscountPct'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'CONSTRAINT', N'CK_SpecialOffer_EndDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [EndDate] >= [StartDate]', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'CONSTRAINT', N'CK_SpecialOffer_EndDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'CONSTRAINT', N'CK_SpecialOffer_MaxQty'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [MaxQty] >= (0)', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'CONSTRAINT', N'CK_SpecialOffer_MaxQty'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'CONSTRAINT', N'CK_SpecialOffer_MinQty'))
EXEC sp_addextendedproperty N'MS_Description', N'Check constraint [MinQty] >= (0)', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'CONSTRAINT', N'CK_SpecialOffer_MinQty'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'CONSTRAINT', N'DF_SpecialOffer_DiscountPct'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0.0', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'CONSTRAINT', N'DF_SpecialOffer_DiscountPct'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'CONSTRAINT', N'DF_SpecialOffer_MinQty'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of 0.0', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'CONSTRAINT', N'DF_SpecialOffer_MinQty'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'CONSTRAINT', N'DF_SpecialOffer_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'CONSTRAINT', N'DF_SpecialOffer_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'CONSTRAINT', N'DF_SpecialOffer_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'CONSTRAINT', N'DF_SpecialOffer_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'CONSTRAINT', N'PK_SpecialOffer_SpecialOfferID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'CONSTRAINT', N'PK_SpecialOffer_SpecialOfferID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'INDEX', N'AK_SpecialOffer_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index. Used to support replication samples.', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'INDEX', N'AK_SpecialOffer_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'INDEX', N'PK_SpecialOffer_SpecialOfferID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Sales', 'TABLE', N'SpecialOffer', 'INDEX', N'PK_SpecialOffer_SpecialOfferID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Store', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Customers (resellers) of Adventure Works products.', 'SCHEMA', N'Sales', 'TABLE', N'Store', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'COLUMN', N'BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key. Foreign key to Customer.BusinessEntityID.', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'COLUMN', N'BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'COLUMN', N'Demographics'))
EXEC sp_addextendedproperty N'MS_Description', N'Demographic informationg about the store such as the number of employees, annual sales and store type.', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'COLUMN', N'Demographics'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'COLUMN', N'Name'))
EXEC sp_addextendedproperty N'MS_Description', N'Name of the store.', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'COLUMN', N'Name'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'COLUMN', N'rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'COLUMN', N'rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'COLUMN', N'SalesPersonID'))
EXEC sp_addextendedproperty N'MS_Description', N'ID of the sales person assigned to the customer. Foreign key to SalesPerson.BusinessEntityID.', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'COLUMN', N'SalesPersonID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'CONSTRAINT', N'DF_Store_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'CONSTRAINT', N'DF_Store_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'CONSTRAINT', N'DF_Store_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of NEWID()', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'CONSTRAINT', N'DF_Store_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'CONSTRAINT', N'FK_Store_BusinessEntity_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing BusinessEntity.BusinessEntityID', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'CONSTRAINT', N'FK_Store_BusinessEntity_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'CONSTRAINT', N'FK_Store_SalesPerson_SalesPersonID'))
EXEC sp_addextendedproperty N'MS_Description', N'Foreign key constraint referencing SalesPerson.SalesPersonID', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'CONSTRAINT', N'FK_Store_SalesPerson_SalesPersonID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'CONSTRAINT', N'PK_Store_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'CONSTRAINT', N'PK_Store_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'INDEX', N'AK_Store_rowguid'))
EXEC sp_addextendedproperty N'MS_Description', N'Unique nonclustered index. Used to support replication samples.', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'INDEX', N'AK_Store_rowguid'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'INDEX', N'IX_Store_SalesPersonID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index.', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'INDEX', N'IX_Store_SalesPersonID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'INDEX', N'PK_Store_BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'INDEX', N'PK_Store_BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'INDEX', N'PXML_Store_Demographics'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary XML index.', 'SCHEMA', N'Sales', 'TABLE', N'Store', 'INDEX', N'PXML_Store_Demographics'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'TrackingEvent', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Tracking event lookup table.', 'SCHEMA', N'Sales', 'TABLE', N'TrackingEvent', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'TrackingEvent', 'COLUMN', N'EventName'))
EXEC sp_addextendedproperty N'MS_Description', N'Tracking event name.', 'SCHEMA', N'Sales', 'TABLE', N'TrackingEvent', 'COLUMN', N'EventName'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'TrackingEvent', 'COLUMN', N'TrackingEventID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key.', 'SCHEMA', N'Sales', 'TABLE', N'TrackingEvent', 'COLUMN', N'TrackingEventID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'TABLE', N'TrackingEvent', 'CONSTRAINT', N'PK_TrackingEvent_TrackingEventID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'Sales', 'TABLE', N'TrackingEvent', 'CONSTRAINT', N'PK_TrackingEvent_TrackingEventID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'AWBuildVersion', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Current version number of the AdventureWorks 2014 sample database. ', 'SCHEMA', N'dbo', 'TABLE', N'AWBuildVersion', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'AWBuildVersion', 'COLUMN', N'Database Version'))
EXEC sp_addextendedproperty N'MS_Description', N'Version number of the database in 9.yy.mm.dd.00 format.', 'SCHEMA', N'dbo', 'TABLE', N'AWBuildVersion', 'COLUMN', N'Database Version'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'AWBuildVersion', 'COLUMN', N'ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'dbo', 'TABLE', N'AWBuildVersion', 'COLUMN', N'ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'AWBuildVersion', 'COLUMN', N'SystemInformationID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for AWBuildVersion records.', 'SCHEMA', N'dbo', 'TABLE', N'AWBuildVersion', 'COLUMN', N'SystemInformationID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'AWBuildVersion', 'COLUMN', N'VersionDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Date and time the record was last updated.', 'SCHEMA', N'dbo', 'TABLE', N'AWBuildVersion', 'COLUMN', N'VersionDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'AWBuildVersion', 'CONSTRAINT', N'DF_AWBuildVersion_ModifiedDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'dbo', 'TABLE', N'AWBuildVersion', 'CONSTRAINT', N'DF_AWBuildVersion_ModifiedDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'AWBuildVersion', 'CONSTRAINT', N'PK_AWBuildVersion_SystemInformationID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'dbo', 'TABLE', N'AWBuildVersion', 'CONSTRAINT', N'PK_AWBuildVersion_SystemInformationID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'AWBuildVersion', 'INDEX', N'PK_AWBuildVersion_SystemInformationID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'dbo', 'TABLE', N'AWBuildVersion', 'INDEX', N'PK_AWBuildVersion_SystemInformationID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Audit table tracking all DDL changes made to the AdventureWorks database. Data is captured by the database trigger ddlDatabaseTriggerLog.', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'COLUMN', N'DatabaseLogID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for DatabaseLog records.', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'COLUMN', N'DatabaseLogID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'COLUMN', N'DatabaseUser'))
EXEC sp_addextendedproperty N'MS_Description', N'The user who implemented the DDL change.', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'COLUMN', N'DatabaseUser'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'COLUMN', N'Event'))
EXEC sp_addextendedproperty N'MS_Description', N'The type of DDL statement that was executed.', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'COLUMN', N'Event'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'COLUMN', N'Object'))
EXEC sp_addextendedproperty N'MS_Description', N'The object that was changed by the DDL statment.', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'COLUMN', N'Object'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'COLUMN', N'PostTime'))
EXEC sp_addextendedproperty N'MS_Description', N'The date and time the DDL change occurred.', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'COLUMN', N'PostTime'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'COLUMN', N'Schema'))
EXEC sp_addextendedproperty N'MS_Description', N'The schema to which the changed object belongs.', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'COLUMN', N'Schema'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'COLUMN', N'TSQL'))
EXEC sp_addextendedproperty N'MS_Description', N'The exact Transact-SQL statement that was executed.', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'COLUMN', N'TSQL'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'COLUMN', N'XmlEvent'))
EXEC sp_addextendedproperty N'MS_Description', N'The raw XML data generated by database trigger.', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'COLUMN', N'XmlEvent'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'CONSTRAINT', N'PK_DatabaseLog_DatabaseLogID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (nonclustered) constraint', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'CONSTRAINT', N'PK_DatabaseLog_DatabaseLogID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'INDEX', N'PK_DatabaseLog_DatabaseLogID'))
EXEC sp_addextendedproperty N'MS_Description', N'Nonclustered index created by a primary key constraint.', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'INDEX', N'PK_DatabaseLog_DatabaseLogID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'ErrorLog', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Audit table tracking errors in the the AdventureWorks database that are caught by the CATCH block of a TRY...CATCH construct. Data is inserted by stored procedure dbo.uspLogError when it is executed from inside the CATCH block of a TRY...CATCH construct.', 'SCHEMA', N'dbo', 'TABLE', N'ErrorLog', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'ErrorLog', 'COLUMN', N'ErrorLine'))
EXEC sp_addextendedproperty N'MS_Description', N'The line number at which the error occurred.', 'SCHEMA', N'dbo', 'TABLE', N'ErrorLog', 'COLUMN', N'ErrorLine'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'ErrorLog', 'COLUMN', N'ErrorLogID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key for ErrorLog records.', 'SCHEMA', N'dbo', 'TABLE', N'ErrorLog', 'COLUMN', N'ErrorLogID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'ErrorLog', 'COLUMN', N'ErrorMessage'))
EXEC sp_addextendedproperty N'MS_Description', N'The message text of the error that occurred.', 'SCHEMA', N'dbo', 'TABLE', N'ErrorLog', 'COLUMN', N'ErrorMessage'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'ErrorLog', 'COLUMN', N'ErrorNumber'))
EXEC sp_addextendedproperty N'MS_Description', N'The error number of the error that occurred.', 'SCHEMA', N'dbo', 'TABLE', N'ErrorLog', 'COLUMN', N'ErrorNumber'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'ErrorLog', 'COLUMN', N'ErrorProcedure'))
EXEC sp_addextendedproperty N'MS_Description', N'The name of the stored procedure or trigger where the error occurred.', 'SCHEMA', N'dbo', 'TABLE', N'ErrorLog', 'COLUMN', N'ErrorProcedure'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'ErrorLog', 'COLUMN', N'ErrorSeverity'))
EXEC sp_addextendedproperty N'MS_Description', N'The severity of the error that occurred.', 'SCHEMA', N'dbo', 'TABLE', N'ErrorLog', 'COLUMN', N'ErrorSeverity'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'ErrorLog', 'COLUMN', N'ErrorState'))
EXEC sp_addextendedproperty N'MS_Description', N'The state number of the error that occurred.', 'SCHEMA', N'dbo', 'TABLE', N'ErrorLog', 'COLUMN', N'ErrorState'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'ErrorLog', 'COLUMN', N'ErrorTime'))
EXEC sp_addextendedproperty N'MS_Description', N'The date and time at which the error occurred.', 'SCHEMA', N'dbo', 'TABLE', N'ErrorLog', 'COLUMN', N'ErrorTime'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'ErrorLog', 'COLUMN', N'UserName'))
EXEC sp_addextendedproperty N'MS_Description', N'The user who executed the batch in which the error occurred.', 'SCHEMA', N'dbo', 'TABLE', N'ErrorLog', 'COLUMN', N'UserName'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'ErrorLog', 'CONSTRAINT', N'DF_ErrorLog_ErrorTime'))
EXEC sp_addextendedproperty N'MS_Description', N'Default constraint value of GETDATE()', 'SCHEMA', N'dbo', 'TABLE', N'ErrorLog', 'CONSTRAINT', N'DF_ErrorLog_ErrorTime'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'ErrorLog', 'CONSTRAINT', N'PK_ErrorLog_ErrorLogID'))
EXEC sp_addextendedproperty N'MS_Description', N'Primary key (clustered) constraint', 'SCHEMA', N'dbo', 'TABLE', N'ErrorLog', 'CONSTRAINT', N'PK_ErrorLog_ErrorLogID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'ErrorLog', 'INDEX', N'PK_ErrorLog_ErrorLogID'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index created by a primary key constraint.', 'SCHEMA', N'dbo', 'TABLE', N'ErrorLog', 'INDEX', N'PK_ErrorLog_ErrorLogID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeHireInfo', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Updates the Employee table and inserts a new row in the EmployeePayHistory table with the values specified in the input parameters.', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeHireInfo', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeHireInfo', 'PARAMETER', N'@BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the stored procedure uspUpdateEmployeeHireInfo. Enter a valid BusinessEntityID from the Employee table.', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeHireInfo', 'PARAMETER', N'@BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeHireInfo', 'PARAMETER', N'@CurrentFlag'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the stored procedure uspUpdateEmployeeHireInfo. Enter the current flag for the employee.', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeHireInfo', 'PARAMETER', N'@CurrentFlag'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeHireInfo', 'PARAMETER', N'@HireDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the stored procedure uspUpdateEmployeeHireInfo. Enter a hire date for the employee.', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeHireInfo', 'PARAMETER', N'@HireDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeHireInfo', 'PARAMETER', N'@JobTitle'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the stored procedure uspUpdateEmployeeHireInfo. Enter a title for the employee.', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeHireInfo', 'PARAMETER', N'@JobTitle'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeHireInfo', 'PARAMETER', N'@PayFrequency'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the stored procedure uspUpdateEmployeeHireInfo. Enter the pay frequency for the employee.', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeHireInfo', 'PARAMETER', N'@PayFrequency'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeHireInfo', 'PARAMETER', N'@Rate'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the stored procedure uspUpdateEmployeeHireInfo. Enter the new rate for the employee.', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeHireInfo', 'PARAMETER', N'@Rate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeHireInfo', 'PARAMETER', N'@RateChangeDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the stored procedure uspUpdateEmployeeHireInfo. Enter the date the rate changed for the employee.', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeHireInfo', 'PARAMETER', N'@RateChangeDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeLogin', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Updates the Employee table with the values specified in the input parameters for the given BusinessEntityID.', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeLogin', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeLogin', 'PARAMETER', N'@BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the stored procedure uspUpdateEmployeeLogin. Enter a valid EmployeeID from the Employee table.', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeLogin', 'PARAMETER', N'@BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeLogin', 'PARAMETER', N'@CurrentFlag'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the stored procedure uspUpdateEmployeeHireInfo. Enter the current flag for the employee.', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeLogin', 'PARAMETER', N'@CurrentFlag'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeLogin', 'PARAMETER', N'@HireDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the stored procedure uspUpdateEmployeeHireInfo. Enter a hire date for the employee.', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeLogin', 'PARAMETER', N'@HireDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeLogin', 'PARAMETER', N'@JobTitle'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the stored procedure uspUpdateEmployeeHireInfo. Enter a title for the employee.', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeLogin', 'PARAMETER', N'@JobTitle'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeLogin', 'PARAMETER', N'@LoginID'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the stored procedure uspUpdateEmployeeHireInfo. Enter a valid login for the employee.', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeLogin', 'PARAMETER', N'@LoginID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeLogin', 'PARAMETER', N'@OrganizationNode'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the stored procedure uspUpdateEmployeeHireInfo. Enter a valid ManagerID for the employee.', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeeLogin', 'PARAMETER', N'@OrganizationNode'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeePersonalInfo', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Updates the Employee table with the values specified in the input parameters for the given EmployeeID.', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeePersonalInfo', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeePersonalInfo', 'PARAMETER', N'@BirthDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the stored procedure uspUpdateEmployeeHireInfo. Enter a birth date for the employee.', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeePersonalInfo', 'PARAMETER', N'@BirthDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeePersonalInfo', 'PARAMETER', N'@BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the stored procedure uspUpdateEmployeePersonalInfo. Enter a valid BusinessEntityID from the HumanResources.Employee table.', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeePersonalInfo', 'PARAMETER', N'@BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeePersonalInfo', 'PARAMETER', N'@Gender'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the stored procedure uspUpdateEmployeeHireInfo. Enter a gender for the employee.', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeePersonalInfo', 'PARAMETER', N'@Gender'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeePersonalInfo', 'PARAMETER', N'@MaritalStatus'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the stored procedure uspUpdateEmployeeHireInfo. Enter a marital status for the employee.', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeePersonalInfo', 'PARAMETER', N'@MaritalStatus'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeePersonalInfo', 'PARAMETER', N'@NationalIDNumber'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the stored procedure uspUpdateEmployeeHireInfo. Enter a national ID for the employee.', 'SCHEMA', N'HumanResources', 'PROCEDURE', N'uspUpdateEmployeePersonalInfo', 'PARAMETER', N'@NationalIDNumber'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'PROCEDURE', N'uspGetBillOfMaterials', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Stored procedure using a recursive query to return a multi-level bill of material for the specified ProductID.', 'SCHEMA', N'dbo', 'PROCEDURE', N'uspGetBillOfMaterials', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'PROCEDURE', N'uspGetBillOfMaterials', 'PARAMETER', N'@CheckDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the stored procedure uspGetBillOfMaterials used to eliminate components not used after that date. Enter a valid date.', 'SCHEMA', N'dbo', 'PROCEDURE', N'uspGetBillOfMaterials', 'PARAMETER', N'@CheckDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'PROCEDURE', N'uspGetBillOfMaterials', 'PARAMETER', N'@StartProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the stored procedure uspGetBillOfMaterials. Enter a valid ProductID from the Production.Product table.', 'SCHEMA', N'dbo', 'PROCEDURE', N'uspGetBillOfMaterials', 'PARAMETER', N'@StartProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'PROCEDURE', N'uspGetEmployeeManagers', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Stored procedure using a recursive query to return the direct and indirect managers of the specified employee.', 'SCHEMA', N'dbo', 'PROCEDURE', N'uspGetEmployeeManagers', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'PROCEDURE', N'uspGetEmployeeManagers', 'PARAMETER', N'@BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the stored procedure uspGetEmployeeManagers. Enter a valid BusinessEntityID from the HumanResources.Employee table.', 'SCHEMA', N'dbo', 'PROCEDURE', N'uspGetEmployeeManagers', 'PARAMETER', N'@BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'PROCEDURE', N'uspGetManagerEmployees', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Stored procedure using a recursive query to return the direct and indirect employees of the specified manager.', 'SCHEMA', N'dbo', 'PROCEDURE', N'uspGetManagerEmployees', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'PROCEDURE', N'uspGetManagerEmployees', 'PARAMETER', N'@BusinessEntityID'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the stored procedure uspGetManagerEmployees. Enter a valid BusinessEntityID of the manager from the HumanResources.Employee table.', 'SCHEMA', N'dbo', 'PROCEDURE', N'uspGetManagerEmployees', 'PARAMETER', N'@BusinessEntityID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'PROCEDURE', N'uspGetWhereUsedProductID', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Stored procedure using a recursive query to return all components or assemblies that directly or indirectly use the specified ProductID.', 'SCHEMA', N'dbo', 'PROCEDURE', N'uspGetWhereUsedProductID', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'PROCEDURE', N'uspGetWhereUsedProductID', 'PARAMETER', N'@CheckDate'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the stored procedure uspGetWhereUsedProductID used to eliminate components not used after that date. Enter a valid date.', 'SCHEMA', N'dbo', 'PROCEDURE', N'uspGetWhereUsedProductID', 'PARAMETER', N'@CheckDate'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'PROCEDURE', N'uspGetWhereUsedProductID', 'PARAMETER', N'@StartProductID'))
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the stored procedure uspGetWhereUsedProductID. Enter a valid ProductID from the Production.Product table.', 'SCHEMA', N'dbo', 'PROCEDURE', N'uspGetWhereUsedProductID', 'PARAMETER', N'@StartProductID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'PROCEDURE', N'uspLogError', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Logs error information in the ErrorLog table about the error that caused execution to jump to the CATCH block of a TRY...CATCH construct. Should be executed from within the scope of a CATCH block otherwise it will return without inserting error information.', 'SCHEMA', N'dbo', 'PROCEDURE', N'uspLogError', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'PROCEDURE', N'uspLogError', 'PARAMETER', N'@ErrorLogID'))
EXEC sp_addextendedproperty N'MS_Description', N'Output parameter for the stored procedure uspLogError. Contains the ErrorLogID value corresponding to the row inserted by uspLogError in the ErrorLog table.', 'SCHEMA', N'dbo', 'PROCEDURE', N'uspLogError', 'PARAMETER', N'@ErrorLogID'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'dbo', 'PROCEDURE', N'uspPrintError', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Prints error information about the error that caused execution to jump to the CATCH block of a TRY...CATCH construct. Should be executed from within the scope of a CATCH block otherwise it will return without printing any error information.', 'SCHEMA', N'dbo', 'PROCEDURE', N'uspPrintError', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'VIEW', N'vEmployeeDepartmentHistory', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Returns employee name and current and previous departments.', 'SCHEMA', N'HumanResources', 'VIEW', N'vEmployeeDepartmentHistory', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'VIEW', N'vEmployeeDepartment', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Returns employee name, title, and current department.', 'SCHEMA', N'HumanResources', 'VIEW', N'vEmployeeDepartment', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'VIEW', N'vEmployee', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Employee names and addresses.', 'SCHEMA', N'HumanResources', 'VIEW', N'vEmployee', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'VIEW', N'vJobCandidateEducation', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Displays the content from each education related element in the xml column Resume in the HumanResources.JobCandidate table. The content has been localized into French, Simplified Chinese and Thai. Some data may not display correctly unless supplemental language support is installed.', 'SCHEMA', N'HumanResources', 'VIEW', N'vJobCandidateEducation', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'VIEW', N'vJobCandidateEmployment', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Displays the content from each employement history related element in the xml column Resume in the HumanResources.JobCandidate table. The content has been localized into French, Simplified Chinese and Thai. Some data may not display correctly unless supplemental language support is installed.', 'SCHEMA', N'HumanResources', 'VIEW', N'vJobCandidateEmployment', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'VIEW', N'vJobCandidate', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Job candidate names and resumes.', 'SCHEMA', N'HumanResources', 'VIEW', N'vJobCandidate', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'VIEW', N'vAdditionalContactInfo', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Displays the contact name and content from each element in the xml column AdditionalContactInfo for that person.', 'SCHEMA', N'Person', 'VIEW', N'vAdditionalContactInfo', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'VIEW', N'vStateProvinceCountryRegion', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Joins StateProvince table with CountryRegion table.', 'SCHEMA', N'Person', 'VIEW', N'vStateProvinceCountryRegion', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'VIEW', N'vStateProvinceCountryRegion', 'INDEX', N'IX_vStateProvinceCountryRegion'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index on the view vStateProvinceCountryRegion.', 'SCHEMA', N'Person', 'VIEW', N'vStateProvinceCountryRegion', 'INDEX', N'IX_vStateProvinceCountryRegion'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'VIEW', N'vProductAndDescription', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Product names and descriptions. Product descriptions are provided in multiple languages.', 'SCHEMA', N'Production', 'VIEW', N'vProductAndDescription', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'VIEW', N'vProductAndDescription', 'INDEX', N'IX_vProductAndDescription'))
EXEC sp_addextendedproperty N'MS_Description', N'Clustered index on the view vProductAndDescription.', 'SCHEMA', N'Production', 'VIEW', N'vProductAndDescription', 'INDEX', N'IX_vProductAndDescription'
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'VIEW', N'vProductModelCatalogDescription', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Displays the content from each element in the xml column CatalogDescription for each product in the Production.ProductModel table that has catalog data.', 'SCHEMA', N'Production', 'VIEW', N'vProductModelCatalogDescription', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'VIEW', N'vProductModelInstructions', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Displays the content from each element in the xml column Instructions for each product in the Production.ProductModel table that has manufacturing instructions.', 'SCHEMA', N'Production', 'VIEW', N'vProductModelInstructions', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'VIEW', N'vVendorWithAddresses', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Vendor (company) names and addresses .', 'SCHEMA', N'Purchasing', 'VIEW', N'vVendorWithAddresses', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', 'VIEW', N'vVendorWithContacts', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Vendor (company) names  and the names of vendor employees to contact.', 'SCHEMA', N'Purchasing', 'VIEW', N'vVendorWithContacts', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'VIEW', N'vIndividualCustomer', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Individual customers (names and addresses) that purchase Adventure Works Cycles products online.', 'SCHEMA', N'Sales', 'VIEW', N'vIndividualCustomer', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'VIEW', N'vPersonDemographics', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Displays the content from each element in the xml column Demographics for each customer in the Person.Person table.', 'SCHEMA', N'Sales', 'VIEW', N'vPersonDemographics', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'VIEW', N'vSalesPersonSalesByFiscalYears', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Uses PIVOT to return aggregated sales information for each sales representative.', 'SCHEMA', N'Sales', 'VIEW', N'vSalesPersonSalesByFiscalYears', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'VIEW', N'vSalesPerson', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Sales representiatives (names and addresses) and their sales-related information.', 'SCHEMA', N'Sales', 'VIEW', N'vSalesPerson', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'VIEW', N'vStoreWithAddresses', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Stores (including store addresses) that sell Adventure Works Cycles products to consumers.', 'SCHEMA', N'Sales', 'VIEW', N'vStoreWithAddresses', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'VIEW', N'vStoreWithContacts', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Stores (including store contacts) that sell Adventure Works Cycles products to consumers.', 'SCHEMA', N'Sales', 'VIEW', N'vStoreWithContacts', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'VIEW', N'vStoreWithDemographics', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Stores (including demographics) that sell Adventure Works Cycles products to consumers.', 'SCHEMA', N'Sales', 'VIEW', N'vStoreWithDemographics', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'TRIGGER', N'ddlDatabaseTriggerLog', NULL, NULL, NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Database trigger to audit all of the DDL changes made to the AdventureWorks database.', 'TRIGGER', N'ddlDatabaseTriggerLog', NULL, NULL, NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', 'XML SCHEMA COLLECTION', N'HRResumeSchemaCollection', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Collection of XML schemas for the Resume column in the HumanResources.JobCandidate table.', 'SCHEMA', N'HumanResources', 'XML SCHEMA COLLECTION', N'HRResumeSchemaCollection', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'XML SCHEMA COLLECTION', N'AdditionalContactInfoSchemaCollection', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Collection of XML schemas for the AdditionalContactInfo column in the Person.Contact table.', 'SCHEMA', N'Person', 'XML SCHEMA COLLECTION', N'AdditionalContactInfoSchemaCollection', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', 'XML SCHEMA COLLECTION', N'IndividualSurveySchemaCollection', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Collection of XML schemas for the Demographics column in the Person.Person table.', 'SCHEMA', N'Person', 'XML SCHEMA COLLECTION', N'IndividualSurveySchemaCollection', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'XML SCHEMA COLLECTION', N'ManuInstructionsSchemaCollection', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Collection of XML schemas for the Instructions column in the Production.ProductModel table.', 'SCHEMA', N'Production', 'XML SCHEMA COLLECTION', N'ManuInstructionsSchemaCollection', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', 'XML SCHEMA COLLECTION', N'ProductDescriptionSchemaCollection', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Collection of XML schemas for the CatalogDescription column in the Production.ProductModel table.', 'SCHEMA', N'Production', 'XML SCHEMA COLLECTION', N'ProductDescriptionSchemaCollection', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', 'XML SCHEMA COLLECTION', N'StoreSurveySchemaCollection', NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Collection of XML schemas for the Demographics column in the Sales.Store table.', 'SCHEMA', N'Sales', 'XML SCHEMA COLLECTION', N'StoreSurveySchemaCollection', NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'HumanResources', NULL, NULL, NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Contains objects related to employees and departments.', 'SCHEMA', N'HumanResources', NULL, NULL, NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Person', NULL, NULL, NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Contains objects related to names and addresses of customers, vendors, and employees', 'SCHEMA', N'Person', NULL, NULL, NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Production', NULL, NULL, NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Contains objects related to products, inventory, and manufacturing.', 'SCHEMA', N'Production', NULL, NULL, NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Purchasing', NULL, NULL, NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Contains objects related to vendors and purchase orders.', 'SCHEMA', N'Purchasing', NULL, NULL, NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', 'SCHEMA', N'Sales', NULL, NULL, NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'Contains objects related to customers, sales orders, and sales territories.', 'SCHEMA', N'Sales', NULL, NULL, NULL, NULL
IF NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', NULL, NULL, NULL, NULL, NULL, NULL))
EXEC sp_addextendedproperty N'MS_Description', N'AdventureWorks 2014 Sample OLTP Database', NULL, NULL, NULL, NULL, NULL, NULL
