IF OBJECT_ID('[dbo].[uspGetOrderTrackingBySalesOrderID]') IS NOT NULL
	DROP PROCEDURE [dbo].[uspGetOrderTrackingBySalesOrderID];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

CREATE PROCEDURE [dbo].[uspGetOrderTrackingBySalesOrderID]
   @SalesOrderID [int] NULL
AS
BEGIN
/* Example:
      exec dbo.uspGetOrderTrackingBySalesOrderID 53498
*/
   SET NOCOUNT ON;

   SELECT 
      ot.SalesOrderID,
      ot.CarrierTrackingNumber,
      ot.OrderTrackingID,
      ot.TrackingEventID,
      te.EventName,
      ot.EventDetails,
      ot.EventDateTime
   FROM 
      Sales.OrderTracking ot, 
      Sales.TrackingEvent te
   WHERE
      ot.SalesOrderID = @SalesOrderID AND
      ot.TrackingEventID = te.TrackingEventID
   ORDER BY
      ot.SalesOrderID,
      ot.TrackingEventID;
END;
GO
