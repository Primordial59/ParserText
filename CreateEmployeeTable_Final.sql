USE [MobileBase]
GO

/****** Объект: Table [dbo].[employee] Дата скрипта: 31.08.2017 16:26:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[employee] (
    [employee]     NVARCHAR (100) NOT NULL,
    [phone_number] NCHAR (11)     NOT NULL,
    [company]      NVARCHAR (50)  NULL
);
