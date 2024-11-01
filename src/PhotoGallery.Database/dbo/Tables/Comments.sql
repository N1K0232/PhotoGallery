CREATE TABLE [dbo].[Comments]
(
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [ImageId] UNIQUEIDENTIFIER NOT NULL,
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [Title] NVARCHAR(256) NOT NULL,
    [Text] NVARCHAR(MAX) NOT NULL,
    [Score] INTEGER NOT NULL,
    [SentimentScore] FLOAT NOT NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT getutcdate(),
    [LastModifiedAt] DATETIME NULL,

    PRIMARY KEY([Id]),
    FOREIGN KEY([ImageId]) REFERENCES [dbo].[Images]([Id]),
    FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers]([Id])
)