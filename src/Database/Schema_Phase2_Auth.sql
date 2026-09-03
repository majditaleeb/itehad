-- =============================================
-- itehad - Phase 2: Authentication, permissions, settings
-- Run via: sqlcmd -S Benz -E -C -d itehad -f i:65001 -i Database\Schema_Phase2_Auth.sql
-- =============================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

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

-- الوحدات (الشاشات) الممنوحة لكل مستخدم غير مدير
CREATE TABLE dbo.AppUserModules
(
    UserId    INT          NOT NULL CONSTRAINT FK_AppUserModules_AppUsers REFERENCES dbo.AppUsers(Id) ON DELETE CASCADE,
    ModuleKey NVARCHAR(50) NOT NULL,
    CONSTRAINT PK_AppUserModules PRIMARY KEY (UserId, ModuleKey)
);

-- إعدادات عامة للنظام (صف واحد ثابت)
CREATE TABLE dbo.AppSettings
(
    Id                    INT           NOT NULL CONSTRAINT PK_AppSettings PRIMARY KEY,
    DebtAlertThresholdILS DECIMAL(10,2) NOT NULL CONSTRAINT DF_AppSettings_ILS DEFAULT (0),
    DebtAlertThresholdUSD DECIMAL(10,2) NOT NULL CONSTRAINT DF_AppSettings_USD DEFAULT (0)
);

INSERT INTO dbo.AppSettings (Id, DebtAlertThresholdILS, DebtAlertThresholdUSD) VALUES (1, 0, 0);

COMMIT TRANSACTION;
