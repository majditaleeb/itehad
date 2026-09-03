-- =============================================
-- itehad - Phase 6: General expense tracking (replaces Fuel + Maintenance)
-- Run via: sqlcmd -S Benz -E -C -d itehad -f i:65001 -i Database\Schema_Phase6_Expenses.sql
-- =============================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

CREATE TABLE dbo.ExpenseCategories
(
    Id   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ExpenseCategories PRIMARY KEY,
    Name NVARCHAR(100)     NOT NULL CONSTRAINT UQ_ExpenseCategories_Name UNIQUE
);

CREATE TABLE dbo.Expenses
(
    Id                  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Expenses PRIMARY KEY,
    CategoryId          INT               NOT NULL CONSTRAINT FK_Expenses_ExpenseCategories REFERENCES dbo.ExpenseCategories(Id),
    InvoiceNumber       NVARCHAR(100)     NULL,
    VendorName          NVARCHAR(200)     NULL,
    VendorLicenseNumber NVARCHAR(100)     NULL,
    InvoiceDate         DATETIME2         NOT NULL,
    Amount              DECIMAL(10,2)     NOT NULL,
    Notes               NVARCHAR(500)     NULL,
    CreatedAt           DATETIME2         NOT NULL CONSTRAINT DF_Expenses_CreatedAt DEFAULT (SYSDATETIME())
);

CREATE INDEX IX_Expenses_CategoryId ON dbo.Expenses(CategoryId);
CREATE INDEX IX_Expenses_InvoiceDate ON dbo.Expenses(InvoiceDate);

INSERT INTO dbo.ExpenseCategories (Name) VALUES (N'سولار'), (N'صيانة'), (N'أكل');

-- ترحيل بيانات تعبئة السولار السابقة (النظام القديم) إلى المصاريف العامة
INSERT INTO dbo.Expenses (CategoryId, InvoiceDate, Amount, Notes)
SELECT (SELECT Id FROM dbo.ExpenseCategories WHERE Name = N'سولار'),
       f.EntryDate, f.Amount,
       N'ترحيل تلقائي من نظام تعبئة السولار السابق - السائق: ' + d.Name + N' - السيارة: ' + ISNULL(d.CarNumber, N'-')
FROM dbo.FuelEntries f
JOIN dbo.Drivers d ON d.Id = f.DriverId;

-- ترحيل بيانات الصيانة السابقة (النظام القديم) إلى المصاريف العامة
INSERT INTO dbo.Expenses (CategoryId, InvoiceDate, Amount, Notes)
SELECT (SELECT Id FROM dbo.ExpenseCategories WHERE Name = N'صيانة'),
       m.EntryDate, m.Amount,
       N'ترحيل تلقائي من نظام الصيانة السابق - السائق: ' + d.Name + N' - السيارة: ' + ISNULL(d.CarNumber, N'-') + N' - ' + m.Description
FROM dbo.MaintenanceEntries m
JOIN dbo.Drivers d ON d.Id = m.DriverId;

DROP TABLE dbo.FuelEntries;
DROP TABLE dbo.MaintenanceEntries;

COMMIT TRANSACTION;
