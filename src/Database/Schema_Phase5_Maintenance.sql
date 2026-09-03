-- =============================================
-- itehad - Phase 5: Vehicle maintenance tracking per driver/car
-- Run via: sqlcmd -S Benz -E -C -d itehad -f i:65001 -i Database\Schema_Phase5_Maintenance.sql
-- =============================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

CREATE TABLE dbo.MaintenanceEntries
(
    Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MaintenanceEntries PRIMARY KEY,
    DriverId    INT               NOT NULL CONSTRAINT FK_MaintenanceEntries_Drivers REFERENCES dbo.Drivers(Id),
    Description NVARCHAR(300)     NOT NULL,
    Amount      DECIMAL(10,2)     NOT NULL,
    EntryDate   DATETIME2         NOT NULL CONSTRAINT DF_MaintenanceEntries_EntryDate DEFAULT (SYSDATETIME()),
    Notes       NVARCHAR(500)     NULL
);

CREATE INDEX IX_MaintenanceEntries_DriverId ON dbo.MaintenanceEntries(DriverId);
CREATE INDEX IX_MaintenanceEntries_EntryDate ON dbo.MaintenanceEntries(EntryDate);

COMMIT TRANSACTION;
