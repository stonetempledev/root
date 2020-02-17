/*

INIT-DB-DEV

inizializzazione oggetti database di sviluppo

i certificati vengono solo aggiunti in caso mancassero.

tabelle, funzioni, s.p., etc. vengono inzializzati

*/

---------- USER ACCESS
/*
USE [toyn]
GO

CREATE USER [IIS APPPOOL\toyn] FOR LOGIN [IIS APPPOOL\toyn]
GO
*/

---------- FUNCTIONS BASE

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
--if exists (SELECT top 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'nodes')
--  drop table nodes
--go

--CREATE TABLE [dbo].[nodes]([id_node] [int] IDENTITY(1,1) NOT NULL, [id_parent_node] [int] NULL, [path_node] varchar(max) not null
-- , [title_node] [varchar](50) NOT NULL, [des_node] [varchar](250) NULL
-- , [dt_ins] [datetime] NOT NULL, [dt_upd] [datetime] NULL
-- , [id_utente] [int] NOT NULL, PRIMARY KEY CLUSTERED ([id_node] ASC));
--go

--create unique index idx_n_1 on nodes (id_parent_node, title_node);
--create index idx_n_2 on nodes (id_parent_node);
--create index idx_n_3 on nodes (title_node);
--go


-- nodes functions

--if exists (select * from sysobjects where id = object_id(N'node_path') and xtype in ('fn', 'if', 'tf'))
-- drop function [dbo].[node_path]
--go

--create function [dbo].[node_path](@id_node int)
--returns varchar(max)
--as
--begin      

-- declare @path varchar(max);

-- with cartelle (id_parent_node, id_node, title_node, livello)
-- as
-- (select id_parent_node, id_node, title_node, 0 as livello
--    from nodes 
--     where id_node = @id_node 
--   union all select e.id_parent_node, e.id_node, e.title_node, livello - 1
--    from nodes e
--    inner join cartelle d on d.id_parent_node = e.id_node and d.id_node <> d.id_parent_node)
-- select @path = coalesce(@path + '/', '') + title_node
--  from cartelle order by livello
  
-- return (case when @path is not null then '/' + @path + '/' else @path + '/' end);
 
--end
--go

--if exists (select * from sysobjects where id = object_id(N'node_path_id') and xtype in ('fn', 'if', 'tf'))
-- drop function [dbo].[node_path_id]
--go

--create function [dbo].[node_path_id](@id_node int)
--returns varchar(max)
--as
--begin      

-- declare @path varchar(max);

-- with cartelle (id_parent_node, id_node, title_node, livello)
-- as
-- (select id_parent_node, id_node, title_node, 0 as livello
--    from nodes 
--     where id_node = @id_node 
--   union all select e.id_parent_node, e.id_node, e.title_node, livello - 1
--    from nodes e
--    inner join cartelle d on d.id_parent_node = e.id_node and d.id_node <> d.id_parent_node)
-- select @path = coalesce(@path + '/', '') + cast(id_node as varchar)
--  from cartelle order by livello
  
-- return (case when @path is not null then '/' + @path + '/' else @path  + '/' end);
 
--end
--go

-- elements
if exists (SELECT top 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'elements')
  drop table [elements]
go

create table [elements] (element_id int not null identity(1,1) primary key, element_type varchar(25) not null, element_code varchar(25), element_title varchar(250), element_ref varchar(100));
go

create index idx_elements on elements_contents (id_utente, element_id);
go

-- elements_contents
if exists (SELECT top 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'elements_contents')
  drop table elements_contents
go

-- child_content_type: title, text, value, account, element
create table elements_contents (element_content_id int not null identity(1,1) primary key, element_id int null, child_content_type varchar(10) not null, child_id int not null);
create unique index idx_elements_contents on elements_contents (element_id, child_content_type, child_id);
create index idx_elements_contents_2 on elements_contents (element_content_id, child_content_type, child_id);
create index idx_elements_contents_3 on elements_contents (child_content_type, child_id);
create index idx_elements_contents_4 on elements_contents (element_id);
go

-- titles
if exists (SELECT top 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'titles')
  drop table titles
go

-- title_ref: url, {@cmd=''}
create table titles (title_id int not null identity(1,1) primary key, title_text varchar(250) not null, title_ref varchar(100));
go

-- texts
if exists (SELECT top 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'texts')
  drop table texts
go

-- text_style: [bold], [underline], [italic], [a capo]
create table texts (text_id int not null identity(1,1) primary key, text_style varchar(50), text_content varchar(max) not null);
go

-- values
if exists (SELECT top 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'values')
  drop table [values]
go

create table [values] (value_id int not null identity(1,1) primary key, value_content varchar(50) not null, value_ref varchar(100), value_notes varchar(100));
go

-- accounts
if exists (SELECT top 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'accounts')
  drop table accounts
go

create table accounts (account_id int not null identity(1,1) primary key, account_user varchar(30) not null, account_password varchar(30) not null
 , account_notes varchar(50));
go




-- elements views

if exists (SELECT top 1 1 FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME = N'vw_elements')
  drop view vw_elements
go

-- select * from elements
-- select * from vw_elements
create view vw_elements
as
 select t.*
  from (
   select e.element_id, e.element_type, e.element_code, e.element_title, e.element_ref, ec.element_id as parent_id
     , ec.element_content_id, null as child_id, null as child_text, null as child_value, null as child_notes, null as child_code, null as child_style, null as child_ref
    from [elements] e
    join elements_contents ec on ec.child_id = e.element_id and ec.child_content_type = 'element'
   union select e.element_id, ec.child_content_type as element_type, null as element_code, null as element_title, null as element_ref, ec.element_id as parent_id
     , ec.element_content_id, t.text_id as child_id, t.text_content as child_text, null as child_value, null as child_notes, null as child_code, t.text_style as child_style, null as child_ref
    from [elements] e
    join elements_contents ec on ec.element_id = e.element_id and ec.child_content_type = 'text'
    join texts t on t.text_id = ec.child_id
   union select e.element_id, ec.child_content_type as element_type, null as element_code, null as element_title, null as element_ref, ec.element_id as parent_id
     , ec.element_content_id, t.title_id as child_id, t.title_text as child_text, null as child_value, null as child_notes, null as child_code, null as child_style, t.title_ref as child_ref
    from [elements] e
    join elements_contents ec on ec.element_id = e.element_id and ec.child_content_type = 'title'
    join titles t on t.title_id = ec.child_id
   union select e.element_id, ec.child_content_type as element_type, null as element_code, null as element_title, null as element_ref, ec.element_id as parent_id
     , ec.element_content_id, v.value_id as child_id, null as child_text, v.value_content as child_value, v.value_notes as child_notes, null as child_code, null as child_style, v.value_ref as child_ref
    from [elements] e
    join elements_contents ec on ec.element_id = e.element_id and ec.child_content_type = 'value'
    join [values] v on v.value_id = ec.child_id
   union select e.element_id, ec.child_content_type as element_type, null as element_code, null as element_title, null as element_ref, ec.element_id as parent_id
     , ec.element_content_id, a.account_id as child_id, null as child_text, a.account_user + '/' + isnull(a.account_password, '') as child_value, a.account_notes as child_notes, null as child_code, null as child_style, null as child_ref
    from [elements] e
    join elements_contents ec on ec.element_id = e.element_id and ec.child_content_type = 'account'
    join accounts a on a.account_id = ec.child_id
  ) t
go

if exists (SELECT top 1 1 FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME = N'vw_elements_h')
  drop view vw_elements_h
go

create view vw_elements_h
as
 select pe.element_id as parent_id, e.element_id, e.element_type, e.element_code, e.element_title, e.element_ref 
  from [elements] e
  left join (
   select element_id,child_id from elements_contents where child_content_type = 'element') pe on pe.child_id = e.element_id
go
