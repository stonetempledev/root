---------- FUNCTIONS

if exists (select * from sysobjects where id = object_id(N'split') and xtype in ('fn', 'if', 'tf'))
 drop function [dbo].[split]
go

CREATE FUNCTION [dbo].[split] (@str varchar(8000), @sep char(1) = ',')     
   returns @tbl TABLE (item varchar(8000))     
  as     
  begin     

   if len(@str) < 1 or @str is null  
    return;

   declare @slice varchar(8000), @idx int;
   set @idx = 1;
   while @idx != 0
   begin
      set @idx = charindex(@sep, @str)
      if @idx!=0     
          set @slice = left(@str, @idx - 1)     
      else     
          set @slice = @str     

      if(len(@slice) > 0)
          insert into @tbl(Item) values(ltrim(rtrim(@slice)))     

      set @str = right(@str, len(@str) - @idx)     
      if len(@str) = 0 break     
   end 
 
   return

  end
go


---------- SECURITY

-- mk 

--  DROP MASTER KEY
--  SELECT * FROM SYS.SYMMETRIC_KEYS 
if not exists (SELECT top 1 1 FROM SYS.SYMMETRIC_KEYS WHERE NAME LIKE '%DatabaseMasterKey%')
 CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'Homme33!'
go

-- cer 
--  DROP CERTIFICATE tL180204 
--  SELECT * FROM SYS.CERTIFICATES 

if not exists (SELECT top 1 1 FROM SYS.CERTIFICATES WHERE NAME = 'toyn191005')
 CREATE CERTIFICATE toyn191005 WITH SUBJECT = 'toyn'
go

-- sk 
--  DROP SYMMETRIC KEY toyn01 
--  SELECT * FROM SYS.SYMMETRIC_KEYS

if not exists (SELECT top 1 1 FROM SYS.SYMMETRIC_KEYS WHERE NAME = 'toyn01')
  CREATE SYMMETRIC KEY toyn01 WITH ALGORITHM = AES_256 ENCRYPTION BY CERTIFICATE toyn191005
go


---------- TABELLE

-- utenti
--  select * from utenti
if exists (SELECT top 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'utenti')
  drop table utenti
go

CREATE TABLE [dbo].[utenti]([id_utente] [int] IDENTITY(1,1) NOT NULL
  , [enc_nome] [varbinary](128) NOT NULL,[enc_email] [varbinary](128) NOT NULL, [pwd] [varchar](100) NOT NULL
  , [dt_ins] [datetime] NOT NULL, [dt_upd] [datetime] NULL, [dt_activate] [datetime] NULL, [tmp_key] [varchar](32) NULL
  , [activate_key] [varchar](32) NULL, [activated] [int] NULL, PRIMARY KEY CLUSTERED ([id_utente] ASC));
go

-- nodes
--  select * from nodes
if exists (SELECT top 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'nodes')
  drop table nodes
go

CREATE TABLE [dbo].[nodes]([id_node] [int] IDENTITY(1,1) NOT NULL, [id_parent_node] [int] NULL
 , [title_node] [varchar](50) NOT NULL, [des_node] [varchar](250) NULL
 , [dt_ins] [datetime] NOT NULL, [dt_upd] [datetime] NULL
 , [id_utente] [int] NOT NULL, PRIMARY KEY CLUSTERED ([id_node] ASC));
go

create unique index idx_n_1 on nodes (id_parent_node, title_node);
create index idx_n_2 on nodes (id_parent_node);
create index idx_n_3 on nodes (title_node);
go
