-- =============================================
-- itehad - Phase 7: optional driver/car link on expenses (for fuel/maintenance categories)
-- Run via: sqlcmd -S Benz -E -C -d itehad -f i:65001 -i Database\Schema_Phase7_ExpenseDriver.sql
-- =============================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

ALTER TABLE dbo.Expenses ADD DriverId INT NULL CONSTRAINT FK_Expenses_Drivers REFERENCES dbo.Drivers(Id);
CREATE INDEX IX_Expenses_DriverId ON dbo.Expenses(DriverId);

COMMIT TRANSACTION;
