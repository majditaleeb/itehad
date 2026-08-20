SET NOCOUNT ON;

IF OBJECT_ID('dbo.TripAuditLog','U') IS NULL
BEGIN
    CREATE TABLE dbo.TripAuditLog(
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TripAuditLog PRIMARY KEY,
        TripId          INT NOT NULL,
        Action          CHAR(1) NOT NULL,          -- 'U' = update, 'D' = delete
        ChangedAt       DATETIME2 NOT NULL CONSTRAINT DF_TripAuditLog_ChangedAt DEFAULT SYSDATETIME(),
        AppUser         NVARCHAR(128) NULL,
        DbLogin         NVARCHAR(128) NULL,
        HostName        NVARCHAR(128) NULL,
        OldTripDate     DATETIME2 NULL,  NewTripDate     DATETIME2 NULL,
        OldSource       NVARCHAR(200) NULL, NewSource    NVARCHAR(200) NULL,
        OldCustomer     NVARCHAR(200) NULL, NewCustomer  NVARCHAR(200) NULL,
        OldFromLoc      NVARCHAR(200) NULL, NewFromLoc   NVARCHAR(200) NULL,
        OldToLoc        NVARCHAR(200) NULL, NewToLoc     NVARCHAR(200) NULL,
        OldFare         DECIMAL(18,2) NULL, NewFare      DECIMAL(18,2) NULL,
        OldCurrency     TINYINT NULL, NewCurrency        TINYINT NULL,
        OldPayment      TINYINT NULL, NewPayment         TINYINT NULL,
        OldRequestType  TINYINT NULL, NewRequestType     TINYINT NULL,
        OldDaysCount    INT NULL, NewDaysCount           INT NULL,
        OldIsSettled    BIT NULL, NewIsSettled           BIT NULL,
        OldNotes        NVARCHAR(500) NULL, NewNotes     NVARCHAR(500) NULL,
        DriversSnapshot NVARCHAR(1000) NULL
    );
END
GO

CREATE OR ALTER TRIGGER dbo.TR_Trips_Audit_Update
ON dbo.Trips
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.TripAuditLog
        (TripId, Action, AppUser, DbLogin, HostName,
         OldTripDate, NewTripDate, OldSource, NewSource, OldCustomer, NewCustomer,
         OldFromLoc, NewFromLoc, OldToLoc, NewToLoc, OldFare, NewFare,
         OldCurrency, NewCurrency, OldPayment, NewPayment, OldRequestType, NewRequestType,
         OldDaysCount, NewDaysCount, OldIsSettled, NewIsSettled, OldNotes, NewNotes)
    SELECT
        i.Id, 'U',
        CONVERT(NVARCHAR(128), SESSION_CONTEXT(N'app_user')),
        SUSER_SNAME(), HOST_NAME(),
        d.TripDate, i.TripDate,
        bs_d.Name, bs_i.Name,
        c_d.Name,  c_i.Name,
        fl_d.Name, fl_i.Name,
        tl_d.Name, tl_i.Name,
        d.Fare, i.Fare,
        d.Currency, i.Currency,
        d.PaymentMethod, i.PaymentMethod,
        d.RequestType, i.RequestType,
        d.DaysCount, i.DaysCount,
        d.IsSettled, i.IsSettled,
        d.Notes, i.Notes
    FROM inserted i
        JOIN deleted d ON i.Id = d.Id
        LEFT JOIN dbo.BookingSources bs_d ON bs_d.Id = d.BookingSourceId
        LEFT JOIN dbo.BookingSources bs_i ON bs_i.Id = i.BookingSourceId
        LEFT JOIN dbo.Customers c_d ON c_d.Id = d.CustomerId
        LEFT JOIN dbo.Customers c_i ON c_i.Id = i.CustomerId
        LEFT JOIN dbo.Locations fl_d ON fl_d.Id = d.FromLocationId
        LEFT JOIN dbo.Locations fl_i ON fl_i.Id = i.FromLocationId
        LEFT JOIN dbo.Locations tl_d ON tl_d.Id = d.ToLocationId
        LEFT JOIN dbo.Locations tl_i ON tl_i.Id = i.ToLocationId
    WHERE
           d.TripDate       <> i.TripDate
        OR d.BookingSourceId <> i.BookingSourceId
        OR d.CustomerId      <> i.CustomerId
        OR d.FromLocationId  <> i.FromLocationId
        OR d.ToLocationId    <> i.ToLocationId
        OR d.Fare            <> i.Fare
        OR d.Currency        <> i.Currency
        OR d.PaymentMethod   <> i.PaymentMethod
        OR d.RequestType     <> i.RequestType
        OR ISNULL(d.DaysCount, -1) <> ISNULL(i.DaysCount, -1)
        OR d.IsSettled       <> i.IsSettled
        OR ISNULL(d.Notes, N'') <> ISNULL(i.Notes, N'');
END
GO

CREATE OR ALTER TRIGGER dbo.TR_Trips_Audit_Delete
ON dbo.Trips
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.TripAuditLog
        (TripId, Action, AppUser, DbLogin, HostName,
         OldTripDate, OldSource, OldCustomer, OldFromLoc, OldToLoc, OldFare,
         OldCurrency, OldPayment, OldRequestType, OldDaysCount, OldIsSettled, OldNotes,
         DriversSnapshot)
    SELECT
        d.Id, 'D',
        CONVERT(NVARCHAR(128), SESSION_CONTEXT(N'app_user')),
        SUSER_SNAME(), HOST_NAME(),
        d.TripDate, bs.Name, c.Name, fl.Name, tl.Name, d.Fare,
        d.Currency, d.PaymentMethod, d.RequestType, d.DaysCount, d.IsSettled, d.Notes,
        CONVERT(NVARCHAR(1000), SESSION_CONTEXT(N'trip_drivers'))
    FROM deleted d
        LEFT JOIN dbo.BookingSources bs ON bs.Id = d.BookingSourceId
        LEFT JOIN dbo.Customers c ON c.Id = d.CustomerId
        LEFT JOIN dbo.Locations fl ON fl.Id = d.FromLocationId
        LEFT JOIN dbo.Locations tl ON tl.Id = d.ToLocationId;
END
GO
