CREATE TABLE [dbo].[Account] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [ExactId]     INT            NOT NULL,
    [IsApproved]  BIT            NOT NULL,
    [CreatedOn]   DATETIME       NOT NULL,
    [Period1]     INT            NOT NULL,
    [Period2]     INT            NOT NULL,
    [Period3]     INT            NOT NULL,
    [BankAccName] NVARCHAR (150) NOT NULL,
    [BIC]         NVARCHAR (150) NOT NULL,
    [IBAN]        NVARCHAR (150) NOT NULL,
    [AuthId]      NVARCHAR (150) NOT NULL,
    [BankId]      INT            NOT NULL,
    CONSTRAINT [PK_Account] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[Allocation] (
    [Id]           INT             IDENTITY (1, 1) NOT NULL,
    [BatchId]      INT             NOT NULL,
    [InvoiceId]    INT             NOT NULL,
    [CustomerId]   INT             NOT NULL,
    [CustomerName] NVARCHAR (150)  NOT NULL,
    [Amount]       DECIMAL (18, 4) NOT NULL,
    CONSTRAINT [PK_Allocation] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[Bank] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [Name]      NVARCHAR (150) NOT NULL,
    [Shorthand] NVARCHAR (10)  NOT NULL,
    CONSTRAINT [PK_Bank] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[Batch] (
    [Id]          INT      IDENTITY (1, 1) NOT NULL,
    [Number]      INT      NOT NULL,
    [AccountId]   INT      NOT NULL,
    [UserId]      INT      NOT NULL,
    [ProcessDate] DATETIME NOT NULL,
    [CreatedOn]   DATETIME NOT NULL,
    CONSTRAINT [PK_Batch] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[User] (
    [Id]        INT      IDENTITY (1, 1) NOT NULL,
    [AccountId] INT      NOT NULL,
    [CreatedOn] DATETIME NOT NULL,
    CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

ALTER TABLE [dbo].[Account]
    ADD DEFAULT 0 FOR [IsApproved];
GO

ALTER TABLE [dbo].[Account]
    ADD DEFAULT 30 FOR [Period1];
GO

ALTER TABLE [dbo].[Account]
    ADD DEFAULT 60 FOR [Period2];
GO

ALTER TABLE [dbo].[Account]
    ADD DEFAULT 90 FOR [Period3];
GO

ALTER TABLE [dbo].[Account] WITH NOCHECK
    ADD CONSTRAINT [FK_Account_Bank] FOREIGN KEY ([BankId]) REFERENCES [dbo].[Bank] ([Id]) ON UPDATE CASCADE;
GO

ALTER TABLE [dbo].[Allocation] WITH NOCHECK
    ADD CONSTRAINT [FK_Allocation_Batch] FOREIGN KEY ([BatchId]) REFERENCES [dbo].[Batch] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE;
GO

ALTER TABLE [dbo].[Batch] WITH NOCHECK
    ADD CONSTRAINT [FK_Batch_Account] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[Account] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE;
GO

ALTER TABLE [dbo].[Batch] WITH NOCHECK
    ADD CONSTRAINT [FK_Batch_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([Id]) ON UPDATE CASCADE;
GO

GO
ALTER TABLE [dbo].[User] WITH NOCHECK
    ADD CONSTRAINT [FK_User_Account] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[Account] ([Id]);
GO

ALTER TABLE [dbo].[Account] WITH CHECK CHECK CONSTRAINT [FK_Account_Bank];
ALTER TABLE [dbo].[Allocation] WITH CHECK CHECK CONSTRAINT [FK_Allocation_Batch];
ALTER TABLE [dbo].[Batch] WITH CHECK CHECK CONSTRAINT [FK_Batch_Account];
ALTER TABLE [dbo].[Batch] WITH CHECK CHECK CONSTRAINT [FK_Batch_User];
ALTER TABLE [dbo].[User] WITH CHECK CHECK CONSTRAINT [FK_User_Account];
GO