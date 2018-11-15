-- drop table __keys
if not exists (select * from sysobjects where name='__keys' and xtype='U')
 CREATE TABLE __Keys ( IDKey INT IDENTITY(1,1), Name varchar(50) NOT NULL, Val varchar(100), ValInt int, ValDbl float, valDate datetime
  ,  CONSTRAINT [pkKeys] PRIMARY KEY CLUSTERED ( [IDKey] ASC ));
  
-- FUNCTIONS
CREATE FUNCTION [dbo].[getKeyDate](@name varchar(50))
RETURNS datetime
AS 
BEGIN     
 return (SELECT valdate FROM __keys where name = @name);
END
GO

CREATE FUNCTION [dbo].[getKeyDbl](@name varchar(50))
RETURNS float
AS 
BEGIN     
 return (SELECT valdbl FROM __keys where name = @name);
END
GO

CREATE FUNCTION [dbo].[getKeyInt](@name varchar(50))
RETURNS int
AS 
BEGIN     
 return (SELECT valint FROM __keys where name = @name);
END
GO

CREATE FUNCTION [dbo].[getKeyVal](@name varchar(50))
RETURNS varchar(100)
AS 
BEGIN     
 return (SELECT val FROM __keys where name = @name);
END
GO
