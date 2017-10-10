CREATE TABLE [dbo].[MobileTable] (
    [Id]            INT             IDENTITY (1, 1) NOT NULL,
    [phone_number]  NCHAR (11)      NOT NULL,
    [date_event]    DATETIME        NOT NULL,
    [time_event]    NCHAR (8)       NOT NULL,
    [service]       NVARCHAR (100)   NOT NULL,
    [target_area]   NVARCHAR (70)   NULL,
    [callnumber]    NVARCHAR (50)   NULL,
    [call_area]     NVARCHAR (50)   NULL,
    [year_event]    NUMERIC (18)    NOT NULL,
    [month_event]   NUMERIC (18)    NOT NULL,
    [day_event]     NUMERIC (18)    NOT NULL,
    [clientaccount] NVARCHAR (50)   NOT NULL,
    [duration]      NUMERIC (18, 2) NOT NULL,
    [cost]          NUMERIC (18, 4) NOT NULL,
    [mess]          NVARCHAR (20)   NULL,
    [input_call]    INT             NULL, 
    [operator] NVARCHAR(20) NULL
);

