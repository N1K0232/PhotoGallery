CREATE TABLE [dbo].[Images]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [Name] VARCHAR(256) NOT NULL,
    [Path] VARCHAR(512) NOT NULL,
    [Title] NVARCHAR(256) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT getutcdate(),
    [LastModifiedAt] DATETIME NULL,

    PRIMARY KEY([Id]),
    FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers]([Id])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_UserImage]
ON [dbo].[Images]([UserId],[Name],[Path]);