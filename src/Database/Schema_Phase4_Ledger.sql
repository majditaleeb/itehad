-- =============================================
-- itehad - Phase 4: Customer ledger (free-form payments, debit/credit/balance)
-- Run via: sqlcmd -S Benz -E -C -d itehad -f i:65001 -i Database\Schema_Phase4_Ledger.sql
-- =============================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

CREATE TABLE dbo.CustomerPayments
(
    Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerPayments PRIMARY KEY,
    CustomerId  INT               NOT NULL CONSTRAINT FK_CustomerPayments_Customers REFERENCES dbo.Customers(Id),
    Amount      DECIMAL(10,2)     NOT NULL,
    Currency    TINYINT           NOT NULL,
    PaymentDate DATETIME2         NOT NULL,
    Notes       NVARCHAR(500)     NULL,
    CreatedAt   DATETIME2         NOT NULL CONSTRAINT DF_CustomerPayments_CreatedAt DEFAULT (SYSDATETIME())
);

CREATE INDEX IX_CustomerPayments_CustomerId ON dbo.CustomerPayments(CustomerId);

-- ترحيل الرحلات الآجلة المحصَّلة مسبقًا (بالنظام القديم) كدفعات، حتى لا يتغيّر الرصيد الفعلي
INSERT INTO dbo.CustomerPayments (CustomerId, Amount, Currency, PaymentDate, Notes)
SELECT CustomerId, Fare, Currency, SettledDate, N'ترحيل تلقائي: تحصيل رحلة سابقة (رقم ' + CAST(Id AS NVARCHAR(20)) + N')'
FROM dbo.Trips
WHERE PaymentMethod = 1 AND IsSettled = 1 AND SettledDate IS NOT NULL;

COMMIT TRANSACTION;
