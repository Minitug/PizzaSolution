


IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Recipes]') AND type in (N'U'))
BEGIN
    CREATE TABLE Recipes
    (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        RecipeType INT NOT NULL,
        CookingTimeMinutes INT NOT NULL
    );

    
    ALTER TABLE Recipes
    ADD CONSTRAINT UQ_Recipes_RecipeType UNIQUE (RecipeType);
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Stock]') AND type in (N'U'))
BEGIN
    CREATE TABLE Stock
    (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        StockType INT NOT NULL UNIQUE,
        Amount INT NOT NULL
    );
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RecipeIngredients]') AND type in (N'U'))
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