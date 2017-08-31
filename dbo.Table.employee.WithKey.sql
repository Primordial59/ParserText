CREATE TABLE [dbo].[employee]
(
    [employee] NVARCHAR(100) NOT NULL, 
    [phone_number] NCHAR(11) NOT NULL, 
    [operator] NVARCHAR(20) NULL, 
    [company] NVARCHAR(50) NULL, 
    CONSTRAINT [PK_dbo.employee] PRIMARY KEY ([phone_number]) 
)
