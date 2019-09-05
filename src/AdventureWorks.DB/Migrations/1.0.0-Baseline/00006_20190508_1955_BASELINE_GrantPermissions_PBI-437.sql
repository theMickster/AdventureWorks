-- <Migration ID="bb84865e-c5d9-4d79-b755-d0e33a38a55b" />
GO
/****************************************************************************************************************
** CREATED BY:   Mick Letofsky
** CREATED DATE: 2019.05.08
** CREATED FOR:  PBI 437
** CREATED:      Baseline code for AdventureWorks SQL Change Automation Project.
****************************************************************************************************************/


GRANT DELETE ON  [Sales].[CustomerPII] TO [SalesManagers]
GRANT INSERT ON  [Sales].[CustomerPII] TO [SalesManagers]
GRANT SELECT ON  [Sales].[CustomerPII] TO [SalesManagers]
GRANT UPDATE ON  [Sales].[CustomerPII] TO [SalesManagers]
GRANT DELETE ON  [Sales].[CustomerPII] TO [SalesPersons]
GRANT INSERT ON  [Sales].[CustomerPII] TO [SalesPersons]
GRANT SELECT ON  [Sales].[CustomerPII] TO [SalesPersons]
GRANT UPDATE ON  [Sales].[CustomerPII] TO [SalesPersons]
