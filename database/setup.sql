/* =====================================================================
   E-Commerce Microservices Platform - database setup
   Creates one database per service plus the Hangfire job store.
   Safe to re-run: every object is created only if it does not exist.

   Run in SSMS (or sqlcmd) against your local SQL Server instance.
   ===================================================================== */

SET NOCOUNT ON;
GO

/* ---------------------------------------------------------------------
   Identity Service
   --------------------------------------------------------------------- */
IF DB_ID(N'IdentityServiceDb') IS NULL CREATE DATABASE IdentityServiceDb;
GO
USE IdentityServiceDb;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        Id            UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Email         NVARCHAR(256)    NOT NULL,
        PasswordHash  NVARCHAR(500)    NOT NULL,
        FullName      NVARCHAR(200)    NOT NULL,
        Role          NVARCHAR(50)     NOT NULL DEFAULT 'Customer',
        CreatedAt     DATETIME2        NOT NULL DEFAULT GETUTCDATE()
    );
    CREATE UNIQUE INDEX IX_Users_Email ON dbo.Users(Email);
END
GO

IF OBJECT_ID(N'dbo.RefreshTokens', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RefreshTokens (
        Id         UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        UserId     UNIQUEIDENTIFIER NOT NULL,
        Token      NVARCHAR(500)    NOT NULL,
        ExpiresAt  DATETIME2        NOT NULL,
        CreatedAt  DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        RevokedAt  DATETIME2        NULL,
        CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
    );
    CREATE INDEX IX_RefreshTokens_Token ON dbo.RefreshTokens(Token);
END
GO

IF OBJECT_ID(N'dbo.RevokedAccessTokens', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RevokedAccessTokens (
        Jti        NVARCHAR(100) NOT NULL PRIMARY KEY,
        RevokedAt  DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        ExpiresAt  DATETIME2     NOT NULL
    );
END
GO

/* ---------------------------------------------------------------------
   Catalog Service
   --------------------------------------------------------------------- */
IF DB_ID(N'CatalogServiceDb') IS NULL CREATE DATABASE CatalogServiceDb;
GO
USE CatalogServiceDb;
GO

IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories (
        Id           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Name         NVARCHAR(200)    NOT NULL,
        Description  NVARCHAR(1000)   NULL,
        IsActive     BIT              NOT NULL DEFAULT 1,
        CreatedAt    DATETIME2        NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products (
        Id           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Name         NVARCHAR(300)    NOT NULL,
        Description  NVARCHAR(2000)   NULL,
        Price        DECIMAL(18,2)    NOT NULL,
        Sku          NVARCHAR(100)    NOT NULL,
        CategoryId   UNIQUEIDENTIFIER NOT NULL,
        IsActive     BIT              NOT NULL DEFAULT 1,
        CreatedAt    DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt    DATETIME2        NULL,
        CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id)
    );
    CREATE UNIQUE INDEX IX_Products_Sku ON dbo.Products(Sku);
    CREATE INDEX IX_Products_CategoryId ON dbo.Products(CategoryId);
END
GO

/* Generic file table: EntityType + EntityId lets any entity own files. */
IF OBJECT_ID(N'dbo.Files', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Files (
        Id           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        EntityType   NVARCHAR(50)     NOT NULL,
        EntityId     UNIQUEIDENTIFIER NOT NULL,
        FileUrl      NVARCHAR(500)    NOT NULL,
        FileName     NVARCHAR(300)    NOT NULL,
        ContentType  NVARCHAR(100)    NOT NULL,
        IsPrimary    BIT              NOT NULL DEFAULT 0,
        CreatedAt    DATETIME2        NOT NULL DEFAULT GETUTCDATE()
    );
    CREATE INDEX IX_Files_EntityType_EntityId ON dbo.Files(EntityType, EntityId);
END
GO

/* Optional starter category so you can create a product straight away. */
IF NOT EXISTS (SELECT 1 FROM dbo.Categories)
    INSERT INTO dbo.Categories (Id, Name, Description)
    VALUES (NEWID(), N'Electronics', N'Phones, laptops, gadgets, and accessories');
GO

/* ---------------------------------------------------------------------
   Order Service
   --------------------------------------------------------------------- */
IF DB_ID(N'OrderServiceDb') IS NULL CREATE DATABASE OrderServiceDb;
GO
USE OrderServiceDb;
GO

IF OBJECT_ID(N'dbo.Orders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders (
        Id               UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        UserId           UNIQUEIDENTIFIER NOT NULL,
        Status           NVARCHAR(50)     NOT NULL DEFAULT 'Pending',
        TotalAmount      DECIMAL(18,2)    NOT NULL,
        ShippingAddress  NVARCHAR(500)    NOT NULL,
        CreatedAt        DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt        DATETIME2        NULL
    );
    CREATE INDEX IX_Orders_UserId ON dbo.Orders(UserId);
END
GO

/* ProductName and UnitPrice are snapshots taken at checkout. */
IF OBJECT_ID(N'dbo.OrderItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems (
        Id           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        OrderId      UNIQUEIDENTIFIER NOT NULL,
        ProductId    UNIQUEIDENTIFIER NOT NULL,
        ProductName  NVARCHAR(300)    NOT NULL,
        UnitPrice    DECIMAL(18,2)    NOT NULL,
        Quantity     INT              NOT NULL,
        CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id)
    );
    CREATE INDEX IX_OrderItems_OrderId ON dbo.OrderItems(OrderId);
END
GO

/* One cart per user. Cart items hold no price: it is looked up live. */
IF OBJECT_ID(N'dbo.Carts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Carts (
        Id         UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        UserId     UNIQUEIDENTIFIER NOT NULL,
        CreatedAt  DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt  DATETIME2        NULL
    );
    CREATE UNIQUE INDEX IX_Carts_UserId ON dbo.Carts(UserId);
END
GO

IF OBJECT_ID(N'dbo.CartItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CartItems (
        Id         UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        CartId     UNIQUEIDENTIFIER NOT NULL,
        ProductId  UNIQUEIDENTIFIER NOT NULL,
        Quantity   INT              NOT NULL,
        CreatedAt  DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_CartItems_Carts FOREIGN KEY (CartId) REFERENCES dbo.Carts(Id)
    );
    CREATE UNIQUE INDEX IX_CartItems_CartId_ProductId ON dbo.CartItems(CartId, ProductId);
END
GO

/* ---------------------------------------------------------------------
   Inventory Service
   --------------------------------------------------------------------- */
IF DB_ID(N'InventoryServiceDb') IS NULL CREATE DATABASE InventoryServiceDb;
GO
USE InventoryServiceDb;
GO

IF OBJECT_ID(N'dbo.Stock', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Stock (
        Id                 UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ProductId          UNIQUEIDENTIFIER NOT NULL,
        QuantityAvailable  INT              NOT NULL DEFAULT 0,
        QuantityReserved   INT              NOT NULL DEFAULT 0,
        UpdatedAt          DATETIME2        NOT NULL DEFAULT GETUTCDATE()
    );
    CREATE UNIQUE INDEX IX_Stock_ProductId ON dbo.Stock(ProductId);
END
GO

IF OBJECT_ID(N'dbo.StockReservations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockReservations (
        Id         UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        OrderId    UNIQUEIDENTIFIER NOT NULL,
        ProductId  UNIQUEIDENTIFIER NOT NULL,
        Quantity   INT              NOT NULL,
        Status     NVARCHAR(50)     NOT NULL DEFAULT 'Reserved',
        CreatedAt  DATETIME2        NOT NULL DEFAULT GETUTCDATE()
    );
    CREATE INDEX IX_StockReservations_OrderId ON dbo.StockReservations(OrderId);
END
GO

/* ---------------------------------------------------------------------
   Payment Service
   --------------------------------------------------------------------- */
IF DB_ID(N'PaymentServiceDb') IS NULL CREATE DATABASE PaymentServiceDb;
GO
USE PaymentServiceDb;
GO

IF OBJECT_ID(N'dbo.Payments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Payments (
        Id                   UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        OrderId              UNIQUEIDENTIFIER NOT NULL,
        UserId               UNIQUEIDENTIFIER NOT NULL,
        Amount               DECIMAL(18,2)    NOT NULL,
        Currency             NVARCHAR(10)     NOT NULL DEFAULT 'BDT',
        Status               NVARCHAR(50)     NOT NULL DEFAULT 'Initiated',
        TransactionId        NVARCHAR(200)    NOT NULL,
        GatewaySessionKey    NVARCHAR(500)    NULL,
        GatewayValidationId  NVARCHAR(200)    NULL,
        FailureReason        NVARCHAR(500)    NULL,
        GatewayPageUrl       NVARCHAR(1000)   NULL,
        CreatedAt            DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt            DATETIME2        NULL
    );
    CREATE UNIQUE INDEX IX_Payments_TransactionId ON dbo.Payments(TransactionId);
    CREATE INDEX IX_Payments_OrderId ON dbo.Payments(OrderId);
    CREATE INDEX IX_Payments_UserId ON dbo.Payments(UserId);
END
GO

/* Guards against the gateway delivering the same IPN more than once. */
IF OBJECT_ID(N'dbo.ProcessedIpnCallbacks', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProcessedIpnCallbacks (
        TransactionId  NVARCHAR(200) NOT NULL PRIMARY KEY,
        ProcessedAt    DATETIME2     NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

/* ---------------------------------------------------------------------
   Notification Service
   --------------------------------------------------------------------- */
IF DB_ID(N'NotificationServiceDb') IS NULL CREATE DATABASE NotificationServiceDb;
GO
USE NotificationServiceDb;
GO

IF OBJECT_ID(N'dbo.NotificationLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NotificationLogs (
        Id                UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        OrderId           UNIQUEIDENTIFIER NOT NULL,
        RecipientEmail    NVARCHAR(256)    NOT NULL,
        NotificationType  NVARCHAR(50)     NOT NULL,
        Subject           NVARCHAR(300)    NOT NULL,
        Status            NVARCHAR(50)     NOT NULL DEFAULT 'Pending',
        ErrorMessage      NVARCHAR(1000)   NULL,
        CreatedAt         DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        SentAt            DATETIME2        NULL
    );
    CREATE INDEX IX_NotificationLogs_OrderId ON dbo.NotificationLogs(OrderId);
END
GO

/* ---------------------------------------------------------------------
   Hangfire (Order Service background jobs)
   Hangfire creates its own tables on first run; the database must exist.
   --------------------------------------------------------------------- */
IF DB_ID(N'HangfireDb') IS NULL CREATE DATABASE HangfireDb;
GO

PRINT 'Database setup complete.';
GO
