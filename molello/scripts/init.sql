
---- table: M_NODES
--  select * from m_nodes
if exists (select * from sysobjects where id = object_id('m_nodes') and objectproperty(id, 'isusertable') = 1)
 drop table m_nodes;
go

create table M_NODES (NODE_ID int identity(1,1) not null primary key, NODE_TITLE varchar(50) not null
 , DT_INS datetime not null, DT_UPD datetime);

create index IDX_M_NODES on M_NODES (node_title);
go


---- table: M_LINKS
--  select * from m_links
if exists (select * from sysobjects where id = object_id('m_links') and objectproperty(id, 'isusertable') = 1)
 drop table m_links;
go

create table M_LINKS (LINK_ID int identity(1,1) not null primary key, PARENT_LINK_ID int, PARENT_NODE_ID int, NODE_ID int not null
 , LAST_NAV datetime, CC_NAV int, DT_INS datetime not null);

create index IDX_M_LINKS on M_LINKS (parent_link_id);
--create index IDX_M_LINKS on M_LINKS (node_id, parent_node_id);
--create index IDX_M_LINKS_2 on M_LINKS (parent_node_id);
--create index IDX_M_LINKS_3 on M_LINKS (node_id);
--create index IDX_M_LINKS_4 on M_LINKS (node_id);
go


---- table: M_ITEM_TYPES
--  select * from m_item_types
if exists (select * from sysobjects where id = object_id('m_item_types') and objectproperty(id, 'isusertable') = 1)
 drop table m_item_types;
go

create table M_ITEM_TYPES (ITEM_TYPE varchar(15) not null primary key, ITEM_TYPE_TITLE varchar(30) not null);
go

delete from m_item_types;
insert into m_item_types (item_type, item_type_title)
 values ('title', 'TITLE'), ('text', 'PARAGRAFO'), ('label', 'ETICHETTA');
go


---- table: M_ITEMS
--  select * from m_items
if exists (select * from sysobjects where id = object_id('m_items') and objectproperty(id, 'isusertable') = 1)
 drop table m_items;
go

create table M_ITEMS (ITEM_ID int identity(1,1) not null primary key, NODE_ID int not null, ITEM_TYPE varchar(15) not null);

create index IDX_M_ITEMS on M_ITEMS (node_id);
go


---- table: M_ITEMS_TEXT
--  select * from m_items_text
if exists (select * from sysobjects where id = object_id('m_items_text') and objectproperty(id, 'isusertable') = 1)
 drop table m_items_text;
go

create table M_ITEMS_TEXT (ITEM_ID int not null primary key, ITEM_TEXT varchar(max) not null);
go


---- table: M_ITEMS_TITLE
--  select * from m_items_title
if exists (select * from sysobjects where id = object_id('m_items_title') and objectproperty(id, 'isusertable') = 1)
 drop table m_items_title;
go

create table M_ITEMS_TITLE (ITEM_ID int not null primary key, ITEM_TITLE varchar(100) not null);
go


---- table: M_ITEMS_LABEL
--  select * from m_items_label
if exists (select * from sysobjects where id = object_id('m_items_label') and objectproperty(id, 'isusertable') = 1)
 drop table m_items_label;
go

create table M_ITEMS_LABEL (ITEM_ID int not null primary key, ITEM_LABEL varchar(50) not null);
go


---- table: M_ITEMS_TODO
--  select * from m_items_todo
if exists (select * from sysobjects where id = object_id('m_items_todo') and objectproperty(id, 'isusertable') = 1)
 drop table m_items_todo;
go

create table M_ITEMS_TODO (ITEM_ID int not null primary key, ITEM_STATO varchar(50) not null, ITEM_COSA varchar(500) not null);
go


---- view: vw_links
-- select * from vw_links
if exists(select 1 from sys.views where name='vw_links' and type='v')
  drop view vw_links;
go

create view vw_links
as
 select l.link_id, l.parent_link_id, l.parent_node_id, pn.node_title as parent_node_title
   , l.node_id, n.node_title
  from m_links l
  left join m_nodes pn on pn.node_id = l.parent_node_id
  left join m_nodes n on n.node_id = l.node_id
go

---- procedure: link_path
-- link_path '8'
-- link_path '8,10', '5,10'
if object_id('link_path', 'p') is not null
	drop proc link_path;
go

create procedure link_path(@link_ids varchar(100), @filter_nodes varchar(100) = null)
as
begin
 with cartelle (link_id, parent_link_id, node_id, node_title, livello)
 as
 (select link_id, parent_link_id, node_id, node_title, 0 as livello
    from vw_links 
    join dbo.split(@link_ids, ',') ids on ids.item = link_id
   union all select e.link_id, e.parent_link_id, e.node_id, e.node_title, livello - 1
    from vw_links e
    inner join cartelle d on d.parent_link_id = e.link_id)
 select distinct c.link_id, c.parent_link_id, c.node_id, c.node_title, c.livello
   , (select count(*) from m_links where parent_link_id = c.link_id) as cc_childs
   , (select count(*) from m_links where node_id = c.node_id and link_id <> c.link_id) as cc_links
  from cartelle c 
  where @filter_nodes is null or (@filter_nodes is not null and (c.node_id in (select item from dbo.split(@filter_nodes, ','))))
  order by c.livello
end
go

---- procedure: sub_links
-- sub_links '4,2', '7,8'
if object_id('sub_links', 'p') is not null
	drop procedure sub_links;
go

create procedure sub_links(@link_ids varchar(100), @filter_nodes varchar(100) = null)
as
begin
 with cartelle (link_id, parent_link_id, node_id, node_title, livello)
 as
 (select link_id, parent_link_id, node_id, node_title, 0 as livello
    from vw_links 
	 join dbo.split(@link_ids, ',') ids on ids.item = link_id
   union all select e.link_id, e.parent_link_id, e.node_id, e.node_title, livello + 1
    from vw_links e
    inner join cartelle d on d.link_id = e.parent_link_id)
 select distinct c.link_id, c.parent_link_id, c.node_id, c.node_title, c.livello
   , (select count(*) from m_links where parent_link_id = c.link_id) as cc_childs
   , (select count(*) from m_links where node_id = c.node_id and link_id <> c.link_id) as cc_links
  from cartelle c 
  where @filter_nodes is null or (@filter_nodes is not null and (c.node_id in (select item from dbo.split(@filter_nodes, ','))))
  order by c.livello
end
go

---- function: split
if object_id('split', 'tf') is not null
	drop function dbo.split;
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
          set @slice = left(@str, @idx - 1);
      else     
          set @slice = @str;

      if(len(@slice) > 0)
          insert into @tbl(Item) values(ltrim(rtrim(@slice)));

      set @str = right(@str, len(@str) - @idx);
      if len(@str) = 0 break;
   end 
 
   return;

  end
GO


