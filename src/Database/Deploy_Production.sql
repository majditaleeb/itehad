-- =============================================
-- itehad - سكربت نشر كامل على سيرفر جديد
-- ينشئ كل الجداول + يزرع بيانات الإعداد الحالية (مصادر الحجز، المواقع،
-- الزبائن، السائقين، تصنيفات المصاريف، إعدادات التنبيه)
-- بدون أي بيانات رحلات أو مصاريف أو حضور/انصراف (فاضية عن قصد)
-- المستخدم الافتراضي (admin) سيُنشأ تلقائيًا أول ما يشتغل الموقع على السيرفر الجديد
--
-- الاستخدام: sqlcmd -S <اسم السيرفر> -E -C -d <اسم قاعدة البيانات> -f i:65001 -i Deploy_Production.sql
-- (لازم تكون قاعدة البيانات نفسها منشأة مسبقًا وفارغة تمامًا قبل التشغيل)
-- =============================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

-- =============================================
-- الجداول
-- =============================================

CREATE TABLE dbo.BookingSources
(
    Id      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BookingSources PRIMARY KEY,
    Name    NVARCHAR(100)     NOT NULL CONSTRAINT UQ_BookingSources_Name UNIQUE
);

CREATE TABLE dbo.Locations
(
    Id      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Locations PRIMARY KEY,
    Name    NVARCHAR(200)     NOT NULL CONSTRAINT UQ_Locations_Name UNIQUE
);

CREATE TABLE dbo.Customers
(
    Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Customers PRIMARY KEY,
    Name        NVARCHAR(200)     NOT NULL,
    Phone       NVARCHAR(30)      NULL,
    CreatedDate DATETIME2         NOT NULL CONSTRAINT DF_Customers_CreatedDate DEFAULT (SYSDATETIME())
);

CREATE TABLE dbo.Drivers
(
    Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Drivers PRIMARY KEY,
    Name        NVARCHAR(200)     NOT NULL,
    Phone       NVARCHAR(30)      NULL,
    CarNumber   NVARCHAR(50)      NULL,
    IsActive    BIT               NOT NULL CONSTRAINT DF_Drivers_IsActive DEFAULT (1)
);

CREATE TABLE dbo.Trips
(
    Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Trips PRIMARY KEY,
    TripDate        DATETIME2         NOT NULL,
    BookingSourceId INT               NOT NULL CONSTRAINT FK_Trips_BookingSources REFERENCES dbo.BookingSources(Id),
    CustomerId      INT               NOT NULL CONSTRAINT FK_Trips_Customers REFERENCES dbo.Customers(Id),
    RequestType     TINYINT           NOT NULL,
    DaysCount       INT               NULL,
    FromLocationId  INT               NOT NULL CONSTRAINT FK_Trips_FromLocation REFERENCES dbo.Locations(Id),
    ToLocationId    INT               NOT NULL CONSTRAINT FK_Trips_ToLocation REFERENCES dbo.Locations(Id),
    Fare            DECIMAL(10,2)     NOT NULL,
    Currency        TINYINT           NOT NULL,
    PaymentMethod   TINYINT           NOT NULL,
    IsSettled       BIT               NOT NULL CONSTRAINT DF_Trips_IsSettled DEFAULT (0),
    SettledDate     DATETIME2         NULL,
    Notes           NVARCHAR(500)     NULL,
    CreatedAt       DATETIME2         NOT NULL CONSTRAINT DF_Trips_CreatedAt DEFAULT (SYSDATETIME())
);

CREATE INDEX IX_Trips_TripDate ON dbo.Trips(TripDate);
CREATE INDEX IX_Trips_CustomerId ON dbo.Trips(CustomerId);

CREATE TABLE dbo.TripDrivers
(
    TripId      INT NOT NULL CONSTRAINT FK_TripDrivers_Trips REFERENCES dbo.Trips(Id) ON DELETE CASCADE,
    DriverId    INT NOT NULL CONSTRAINT FK_TripDrivers_Drivers REFERENCES dbo.Drivers(Id),
    CONSTRAINT PK_TripDrivers PRIMARY KEY (TripId, DriverId)
);

CREATE INDEX IX_TripDrivers_DriverId ON dbo.TripDrivers(DriverId);

CREATE TABLE dbo.DriverAttendance
(
    Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DriverAttendance PRIMARY KEY,
    DriverId        INT               NOT NULL CONSTRAINT FK_DriverAttendance_Drivers REFERENCES dbo.Drivers(Id),
    CheckInTime     DATETIME2         NOT NULL,
    CheckOutTime    DATETIME2         NULL
);

CREATE INDEX IX_DriverAttendance_DriverId ON dbo.DriverAttendance(DriverId);

CREATE TABLE dbo.AppUsers
(
    Id           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AppUsers PRIMARY KEY,
    Username     NVARCHAR(50)      NOT NULL CONSTRAINT UQ_AppUsers_Username UNIQUE,
    PasswordHash NVARCHAR(200)     NOT NULL,
    DisplayName  NVARCHAR(200)     NOT NULL,
    IsAdmin      BIT               NOT NULL CONSTRAINT DF_AppUsers_IsAdmin DEFAULT (0),
    IsActive     BIT               NOT NULL CONSTRAINT DF_AppUsers_IsActive DEFAULT (1),
    CreatedDate  DATETIME2         NOT NULL CONSTRAINT DF_AppUsers_CreatedDate DEFAULT (SYSDATETIME())
);

CREATE TABLE dbo.AppUserModules
(
    UserId    INT          NOT NULL CONSTRAINT FK_AppUserModules_AppUsers REFERENCES dbo.AppUsers(Id) ON DELETE CASCADE,
    ModuleKey NVARCHAR(50) NOT NULL,
    CONSTRAINT PK_AppUserModules PRIMARY KEY (UserId, ModuleKey)
);

CREATE TABLE dbo.AppSettings
(
    Id                    INT           NOT NULL CONSTRAINT PK_AppSettings PRIMARY KEY,
    DebtAlertThresholdILS DECIMAL(10,2) NOT NULL CONSTRAINT DF_AppSettings_ILS DEFAULT (0),
    DebtAlertThresholdUSD DECIMAL(10,2) NOT NULL CONSTRAINT DF_AppSettings_USD DEFAULT (0)
);

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
    CreatedAt           DATETIME2         NOT NULL CONSTRAINT DF_Expenses_CreatedAt DEFAULT (SYSDATETIME()),
    DriverId            INT               NULL CONSTRAINT FK_Expenses_Drivers REFERENCES dbo.Drivers(Id)
);

CREATE INDEX IX_Expenses_CategoryId ON dbo.Expenses(CategoryId);
CREATE INDEX IX_Expenses_InvoiceDate ON dbo.Expenses(InvoiceDate);
CREATE INDEX IX_Expenses_DriverId ON dbo.Expenses(DriverId);

-- =============================================
-- بيانات الإعداد الحالية (مواقع، زبائن، سائقين... الخ)
-- ملاحظة: بدون أي رحلات / مصاريف / حضور وانصراف - فاضية عن قصد
-- حساب المدير الافتراضي (admin) سيُنشأ تلقائيًا أول ما يفتح الموقع على السيرفر الجديد
-- =============================================

SET IDENTITY_INSERT dbo.BookingSources ON;
INSERT INTO dbo.BookingSources (Id, Name) VALUES (4, N'حجوزات المجد');
INSERT INTO dbo.BookingSources (Id, Name) VALUES (5, N'حجوزات الاتحاد');
INSERT INTO dbo.BookingSources (Id, Name) VALUES (6, N'مباشر');
SET IDENTITY_INSERT dbo.BookingSources OFF;

SET IDENTITY_INSERT dbo.Locations ON;
INSERT INTO dbo.Locations (Id, Name) VALUES (6, N'باب العمود');
INSERT INTO dbo.Locations (Id, Name) VALUES (7, N'اريحا');
INSERT INTO dbo.Locations (Id, Name) VALUES (9, N'المعاير');
SET IDENTITY_INSERT dbo.Locations OFF;

SET IDENTITY_INSERT dbo.Customers ON;
INSERT INTO dbo.Customers (Id, Name, Phone, CreatedDate) VALUES (3, N'زبون نقدي', N'', '2026-07-06T22:19:57.6638397');
INSERT INTO dbo.Customers (Id, Name, Phone, CreatedDate) VALUES (4, N'زبون نقدي', N'', '2026-07-06T22:21:53.1281264');
INSERT INTO dbo.Customers (Id, Name, Phone, CreatedDate) VALUES (5, N'شركة', N'', '2026-07-06T22:29:09.2923507');
INSERT INTO dbo.Customers (Id, Name, Phone, CreatedDate) VALUES (6, N'المجد للسباحة والسفر', N'', '2026-07-07T10:31:11.1467647');
SET IDENTITY_INSERT dbo.Customers OFF;

SET IDENTITY_INSERT dbo.Drivers ON;
INSERT INTO dbo.Drivers (Id, Name, Phone, CarNumber, IsActive) VALUES (5, N'لافي', NULL, N'258369', 1);
INSERT INTO dbo.Drivers (Id, Name, Phone, CarNumber, IsActive) VALUES (6, N'عادل', NULL, NULL, 1);
SET IDENTITY_INSERT dbo.Drivers OFF;

SET IDENTITY_INSERT dbo.ExpenseCategories ON;
INSERT INTO dbo.ExpenseCategories (Id, Name) VALUES (1, N'سولار');
INSERT INTO dbo.ExpenseCategories (Id, Name) VALUES (2, N'صيانة');
INSERT INTO dbo.ExpenseCategories (Id, Name) VALUES (3, N'أكل');
SET IDENTITY_INSERT dbo.ExpenseCategories OFF;

INSERT INTO dbo.AppSettings (Id, DebtAlertThresholdILS, DebtAlertThresholdUSD) VALUES (1, 0.00, 0.00);

COMMIT TRANSACTION;
