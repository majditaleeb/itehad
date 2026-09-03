-- =============================================
-- itehad - Phase 3: Fuel (diesel) tracking per driver/car
-- Run via: sqlcmd -S Benz -E -C -d itehad -f i:65001 -i Database\Schema_Phase3_Fuel.sql
-- =============================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

CREATE TABLE dbo.FuelEntries
(
    Id        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FuelEntries PRIMARY KEY,
    DriverId  INT               NOT NULL CONSTRAINT FK_FuelEntries_Drivers REFERENCES dbo.Drivers(Id),
    Amount    DECIMAL(10,2)     NOT NULL,
    EntryDate DATETIME2         NOT NULL CONSTRAINT DF_FuelEntries_EntryDate DEFAULT (SYSDATETIME()),
    Notes     NVARCHAR(500)     NULL
);

CREATE INDEX IX_FuelEntries_DriverId ON dbo.FuelEntries(DriverId);
CREATE INDEX IX_FuelEntries_EntryDate ON dbo.FuelEntries(EntryDate);

COMMIT TRANSACTION;
