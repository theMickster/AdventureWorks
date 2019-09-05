IF OBJECT_ID('[dbo].[uspAddOrderTrackingEvent]') IS NOT NULL
	DROP PROCEDURE [dbo].[uspAddOrderTrackingEvent];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

----------------------------------------------------------------
-- Create supporting stored procedures
----------------------------------------------------------------
CREATE PROCEDURE [dbo].[uspAddOrderTrackingEvent]
   @SalesOrderID INT,
   @TrackingEventID INT,
   @EventDetails NVARCHAR(2000)
AS
BEGIN
/* Example:
      exec dbo.uspGetOrderTrackingBySalesOrderID 53498
      exec dbo.uspAddOrderTrackingEvent 53498, 7, 'invalid address, package is undeleverable'
      exec dbo.uspGetOrderTrackingBySalesOrderID 53498
*/
   SET NOCOUNT ON;

   BEGIN TRY
      BEGIN TRANSACTION;

      DECLARE @TrackingNumber NVARCHAR(25);

      SET @TrackingNumber = (
         SELECT TOP 1 ot.CarrierTrackingNumber 
           FROM Sales.OrderTracking ot
          WHERE ot.SalesOrderID = @SalesOrderID);

      IF (@TrackingNumber IS NULL)
      BEGIN
         SET @TrackingNumber = SUBSTRING(CONVERT(CHAR(255), NEWID()),2,25);
      END;

      INSERT INTO Sales.OrderTracking
         (SalesOrderID, CarrierTrackingNumber, TrackingEventID, EventDetails, EventDateTime)
      VALUES
         (@SalesOrderID, @TrackingNumber, @TrackingEventID, @EventDetails, GETDATE());

      COMMIT TRANSACTION;
   END TRY
   BEGIN CATCH
      -- Rollback any active or uncommittable transactions before
      -- inserting information in the ErrorLog
      IF @@TRANCOUNT > 0
      BEGIN
         ROLLBACK TRANSACTION;
      END

      EXECUTE [dbo].[uspLogError];
   END CATCH;
END;
GO
