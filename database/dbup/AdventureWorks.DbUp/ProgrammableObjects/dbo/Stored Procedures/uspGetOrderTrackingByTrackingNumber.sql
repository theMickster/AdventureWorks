IF OBJECT_ID('[dbo].[uspGetOrderTrackingByTrackingNumber]') IS NOT NULL
	DROP PROCEDURE [dbo].[uspGetOrderTrackingByTrackingNumber];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

CREATE PROCEDURE [dbo].[uspGetOrderTrackingByTrackingNumber]
   @CarrierTrackingNumber [nvarchar](25) NULL
AS
BEGIN
/* Example:
      EXEC dbo.uspGetOrderTrackingByTrackingNumber 'EE33-45E8-9F'
      EXEC dbo.uspAddOrderTrackingEvent 53498, 7, 'invalid address, package is undeleverable'
      EXEC dbo.uspGetOrderTrackingByTrackingNumber 'EE33-45E8-9F'
*/
   SET NOCOUNT ON;

   IF (@CarrierTrackingNumber IS NULL)
   BEGIN
      RETURN;
   END;

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
      ot.CarrierTrackingNumber = @CarrierTrackingNumber AND
      ot.TrackingEventID = te.TrackingEventID
   ORDER BY
      ot.SalesOrderID,
      ot.TrackingEventID;
END;

GO
