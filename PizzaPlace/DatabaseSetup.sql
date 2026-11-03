USE master;
GO

IF DB_ID(N'PizzaPlace') IS NULL
    CREATE DATABASE PizzaPlace;
GO

USE PizzaPlace;
GO

IF OBJECT_ID(N'dbo.Recipes', N'U') IS NULL
BEGIN
    CREATE TABLE Recipes
    (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        RecipeType INT NOT NULL UNIQUE,
        CookingTimeMinutes INT NOT NULL
    );
END

IF OBJECT_ID(N'dbo.Stock', N'U') IS NULL
BEGIN
    CREATE TABLE Stock
    (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        StockType INT NOT NULL UNIQUE,
        Amount INT NOT NULL
    );
END

IF OBJECT_ID(N'dbo.RecipeIngredients', N'U') IS NULL
BEGIN
    CREATE TABLE RecipeIngredients
    (
        RecipeId BIGINT NOT NULL,
        StockType INT NOT NULL,
        Amount INT NOT NULL,

        CONSTRAINT PK_RecipeIngredients
            PRIMARY KEY (RecipeId, StockType),

        CONSTRAINT FK_RecipeIngredients_Recipes
            FOREIGN KEY (RecipeId)
            REFERENCES Recipes(Id)
            ON DELETE CASCADE
    )
END

IF OBJECT_ID(N'dbo.Orders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders
    (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        Status NVARCHAR(20) NOT NULL,
        TotalCost DECIMAL(10, 2) NOT NULL,
        EstimatedMinutes INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CompletedAt DATETIME2 NULL
    );
END