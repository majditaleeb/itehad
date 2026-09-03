-- =============================================
-- itehad - Taxi Office Management System
-- Schema for database: itehad (server: Benz)
-- Run once via: sqlcmd -S Benz -E -C -d itehad -f i:65001 -i Database\Schema.sql
-- (-f i:65001 forces UTF-8 input so the Arabic seed literals below are read correctly)
-- =============================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

-- مصادر الحجز
CREATE TABLE dbo.BookingSources
(
    Id      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BookingSources PRIMARY KEY,
    Name    NVARCHAR(100)     NOT NULL CONSTRAINT UQ_BookingSources_Name UNIQUE
);

-- المواقع (تُستخدم لكل من "من" و"إلى")
CREATE TABLE dbo.Locations
(
    Id      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Locations PRIMARY KEY,
    Name    NVARCHAR(200)     NOT NULL CONSTRAINT UQ_Locations_Name UNIQUE
);

-- الزبائن
CREATE TABLE dbo.Customers
(
    Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Customers PRIMARY KEY,
    Name        NVARCHAR(200)     NOT NULL,
    Phone       NVARCHAR(30)      NULL,
    CreatedDate DATETIME2         NOT NULL CONSTRAINT DF_Customers_CreatedDate DEFAULT (SYSDATETIME())
);

-- السائقون
CREATE TABLE dbo.Drivers
(
    Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Drivers PRIMARY KEY,
    Name        NVARCHAR(200)     NOT NULL,
    Phone       NVARCHAR(30)      NULL,
    CarNumber   NVARCHAR(50)      NULL,
    IsActive    BIT               NOT NULL CONSTRAINT DF_Drivers_IsActive DEFAULT (1)
);

-- الرحلات
CREATE TABLE dbo.Trips
(
    Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Trips PRIMARY KEY,
    TripDate        DATETIME2         NOT NULL,
    BookingSourceId INT               NOT NULL CONSTRAINT FK_Trips_BookingSources REFERENCES dbo.BookingSources(Id),
    CustomerId      INT               NOT NULL CONSTRAINT FK_Trips_Customers REFERENCES dbo.Customers(Id),
    RequestType     TINYINT           NOT NULL, -- 0 = ترانسفير, 1 = حجز لعدة أيام
    DaysCount       INT               NULL,
    FromLocationId  INT               NOT NULL CONSTRAINT FK_Trips_FromLocation REFERENCES dbo.Locations(Id),
    ToLocationId    INT               NOT NULL CONSTRAINT FK_Trips_ToLocation REFERENCES dbo.Locations(Id),
    Fare            DECIMAL(10,2)     NOT NULL,
    Currency        TINYINT           NOT NULL, -- 0 = شيقل, 1 = دولار
    PaymentMethod   TINYINT           NOT NULL, -- 0 = نقدي, 1 = ذمم
    IsSettled       BIT               NOT NULL CONSTRAINT DF_Trips_IsSettled DEFAULT (0),
    SettledDate     DATETIME2         NULL,
    Notes           NVARCHAR(500)     NULL,
    CreatedAt       DATETIME2         NOT NULL CONSTRAINT DF_Trips_CreatedAt DEFAULT (SYSDATETIME())
);

CREATE INDEX IX_Trips_TripDate ON dbo.Trips(TripDate);
CREATE INDEX IX_Trips_CustomerId ON dbo.Trips(CustomerId);

-- ربط الرحلة بأكثر من سائق/سيارة
CREATE TABLE dbo.TripDrivers
(
    TripId      INT NOT NULL CONSTRAINT FK_TripDrivers_Trips REFERENCES dbo.Trips(Id) ON DELETE CASCADE,
    DriverId    INT NOT NULL CONSTRAINT FK_TripDrivers_Drivers REFERENCES dbo.Drivers(Id),
    CONSTRAINT PK_TripDrivers PRIMARY KEY (TripId, DriverId)
);

CREATE INDEX IX_TripDrivers_DriverId ON dbo.TripDrivers(DriverId);

-- حضور وانصراف السائقين
CREATE TABLE dbo.DriverAttendance
(
    Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DriverAttendance PRIMARY KEY,
    DriverId        INT               NOT NULL CONSTRAINT FK_DriverAttendance_Drivers REFERENCES dbo.Drivers(Id),
    CheckInTime     DATETIME2         NOT NULL,
    CheckOutTime    DATETIME2         NULL
);

CREATE INDEX IX_DriverAttendance_DriverId ON dbo.DriverAttendance(DriverId);

-- بيانات أولية: مصادر الحجز المذكورة في خطة العمل
INSERT INTO dbo.BookingSources (Name) VALUES (N'حجوزات المجد'), (N'حجوزات الاتحاد'), (N'مباشر');

COMMIT TRANSACTION;
