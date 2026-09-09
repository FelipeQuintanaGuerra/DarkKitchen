IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Categories] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [CommercialLines] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_CommercialLines] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Users] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Surname] nvarchar(max) NOT NULL,
    [Email] nvarchar(450) NOT NULL,
    [Phone] nvarchar(max) NOT NULL,
    [Password] nvarchar(max) NOT NULL,
    [Role] int NOT NULL,
    [IsInternal] bit NOT NULL,
    [Salt] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Promotions] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [Discount] int NOT NULL,
    [CommercialLineId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_Promotions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Promotions_CommercialLines_CommercialLineId] FOREIGN KEY ([CommercialLineId]) REFERENCES [CommercialLines] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Orders] (
    [OrderNumber] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [Status] int NOT NULL,
    [CustomerId] uniqueidentifier NOT NULL,
    [ShippingCost] decimal(18,2) NOT NULL,
    [Street] nvarchar(max) NOT NULL,
    [DoorNumber] nvarchar(max) NOT NULL,
    [Apartment] nvarchar(max) NOT NULL,
    [Subtotal] decimal(18,2) NOT NULL,
    [Total] decimal(18,2) NOT NULL,
    [ShippingTypeName] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY ([OrderNumber]),
    CONSTRAINT [FK_Orders_Users_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Products] (
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(max) NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [CommercialLineId] uniqueidentifier NOT NULL,
    [CategoryId] uniqueidentifier NOT NULL,
    [IsActive] bit NOT NULL,
    [PromotionId] uniqueidentifier NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Products_CommercialLines_CommercialLineId] FOREIGN KEY ([CommercialLineId]) REFERENCES [CommercialLines] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Products_Promotions_PromotionId] FOREIGN KEY ([PromotionId]) REFERENCES [Promotions] ([Id])
);
GO

CREATE TABLE [ProductPictures] (
    [Id] uniqueidentifier NOT NULL,
    [FileName] nvarchar(max) NOT NULL,
    [SizeInKb] int NOT NULL,
    [ProductId] uniqueidentifier NULL,
    CONSTRAINT [PK_ProductPictures] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductPictures_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id])
);
GO

CREATE INDEX [IX_Orders_CustomerId] ON [Orders] ([CustomerId]);
GO

CREATE INDEX [IX_ProductPictures_ProductId] ON [ProductPictures] ([ProductId]);
GO

CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId]);
GO

CREATE INDEX [IX_Products_CommercialLineId] ON [Products] ([CommercialLineId]);
GO

CREATE INDEX [IX_Products_PromotionId] ON [Products] ([PromotionId]);
GO

CREATE INDEX [IX_Promotions_CommercialLineId] ON [Promotions] ([CommercialLineId]);
GO

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260423231354_InitialMigration', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [OrderLines] (
    [Id] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [Discount] decimal(18,2) NOT NULL,
    [OrderNumber] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_OrderLines] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_OrderLines_Orders_OrderNumber] FOREIGN KEY ([OrderNumber]) REFERENCES [Orders] ([OrderNumber]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_OrderLines_OrderNumber] ON [OrderLines] ([OrderNumber]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260424183614_AddOrderLines', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260424184334_AddProductPictures', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [ProductPictures] DROP CONSTRAINT [FK_ProductPictures_Products_ProductId];
GO

ALTER TABLE [ProductPictures] ADD CONSTRAINT [FK_ProductPictures_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260424184558_AddProductPicturesRelation', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260424184828_AddCategories', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260424185108_AddCommercialLines', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Promotions] DROP CONSTRAINT [FK_Promotions_CommercialLines_CommercialLineId];
GO

ALTER TABLE [Promotions] ADD CONSTRAINT [FK_Promotions_CommercialLines_CommercialLineId] FOREIGN KEY ([CommercialLineId]) REFERENCES [CommercialLines] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260424185311_AddPromotions', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Products] DROP CONSTRAINT [FK_Products_Promotions_PromotionId];
GO

DROP INDEX [IX_Products_PromotionId] ON [Products];
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'PromotionId');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Products] DROP COLUMN [PromotionId];
GO

CREATE TABLE [PromotionProducts] (
    [ProductsId] uniqueidentifier NOT NULL,
    [PromotionId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_PromotionProducts] PRIMARY KEY ([ProductsId], [PromotionId]),
    CONSTRAINT [FK_PromotionProducts_Products_ProductsId] FOREIGN KEY ([ProductsId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PromotionProducts_Promotions_PromotionId] FOREIGN KEY ([PromotionId]) REFERENCES [Promotions] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_PromotionProducts_PromotionId] ON [PromotionProducts] ([PromotionId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260424185615_AddPromotionProductsRelation', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260424191314_AddOrderCustomerNavigation', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE INDEX [IX_OrderLines_ProductId] ON [OrderLines] ([ProductId]);
GO

ALTER TABLE [OrderLines] ADD CONSTRAINT [FK_OrderLines_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260424221058_AddOrderLineProductRelation', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Sessions] (
    [Token] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_Sessions] PRIMARY KEY ([Token]),
    CONSTRAINT [FK_Sessions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Sessions_UserId] ON [Sessions] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260520185456_AddSessionTable', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Orders]') AND [c].[name] = N'ShippingTypeName');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Orders] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Orders] DROP COLUMN [ShippingTypeName];
GO

ALTER TABLE [Orders] ADD [ShippingTypeId] uniqueidentifier NOT NULL DEFAULT '11111111-1111-1111-1111-111111111111';
GO

CREATE TABLE [ShippingTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Cost] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_ShippingTypes] PRIMARY KEY ([Id])
);
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Cost', N'Name') AND [object_id] = OBJECT_ID(N'[ShippingTypes]'))
    SET IDENTITY_INSERT [ShippingTypes] ON;
INSERT INTO [ShippingTypes] ([Id], [Cost], [Name])
VALUES ('11111111-1111-1111-1111-111111111111', 250.0, N'Express'),
('22222222-2222-2222-2222-222222222222', 180.0, N'24Hours');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Cost', N'Name') AND [object_id] = OBJECT_ID(N'[ShippingTypes]'))
    SET IDENTITY_INSERT [ShippingTypes] OFF;
GO

CREATE INDEX [IX_Orders_ShippingTypeId] ON [Orders] ([ShippingTypeId]);
GO

CREATE UNIQUE INDEX [IX_ShippingTypes_Name] ON [ShippingTypes] ([Name]);
GO

ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_ShippingTypes_ShippingTypeId] FOREIGN KEY ([ShippingTypeId]) REFERENCES [ShippingTypes] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260528012609_AddShippingTypes', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AuditLoggers] (
    [Id] uniqueidentifier NOT NULL,
    [Username] nvarchar(max) NOT NULL,
    [Action] nvarchar(max) NOT NULL,
    [Time] datetime2 NOT NULL,
    [EntityName] nvarchar(max) NULL,
    [EntityId] uniqueidentifier NULL,
    [Description] nvarchar(max) NULL,
    CONSTRAINT [PK_AuditLoggers] PRIMARY KEY ([Id])
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260601011133_AddAuditLoggerEntityFields', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [ShippingTypes] ADD [IsDefault] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

UPDATE [ShippingTypes] SET [IsDefault] = CAST(1 AS bit)
WHERE [Id] = '11111111-1111-1111-1111-111111111111';
SELECT @@ROWCOUNT;

GO

UPDATE [ShippingTypes] SET [IsDefault] = CAST(1 AS bit)
WHERE [Id] = '22222222-2222-2222-2222-222222222222';
SELECT @@ROWCOUNT;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260606150013_AddShippingTypeIsDefault', N'8.0.0');
GO

COMMIT;
GO

