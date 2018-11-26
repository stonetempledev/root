
---- table: M_NODES
--  select * from m_nodes
if exists (select * from sysobjects where id = object_id('m_nodes') and objectproperty(id, 'isusertable') = 1)
 drop table m_nodes;
go

create table M_NODES (NODE_ID int identity(1,1) not null primary key, NODE_TITLE varchar(50) not null);

create index IDX_M_NODES on M_NODES (node_title);
go


---- table: M_LINKS
--  select * from m_links
if exists (select * from sysobjects where id = object_id('m_links') and objectproperty(id, 'isusertable') = 1)
 drop table m_links;
go

create table M_LINKS (LINK_ID int identity(1,1) not null primary key, PARENT_LINK_ID int, PARENT_NODE_ID int, NODE_ID int not null);

create index IDX_M_LINKS on M_LINKS (parent_link_id);
--create index IDX_M_LINKS on M_LINKS (node_id, parent_node_id);
--create index IDX_M_LINKS_2 on M_LINKS (parent_node_id);
--create index IDX_M_LINKS_3 on M_LINKS (node_id);
--create index IDX_M_LINKS_4 on M_LINKS (node_id);
go


---- table: M_ITEMS
--  select * from m_items
if exists (select * from sysobjects where id = object_id('m_items') and objectproperty(id, 'isusertable') = 1)
 drop table m_items;
go

create table M_ITEMS (ITEM_ID int identity(1,1) not null primary key, NODE_ID int not null, ITEM_TITLE varchar(50) not null);

create unique index IDX_M_ITEMS on M_ITEMS (node_id, item_title);
create index IDX_M_ITEMS_2 on M_ITEMS (node_id);
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
-- link_path 8
if object_id('link_path', 'p') is not null
	drop proc link_path;
go

create procedure link_path(@link_id int)
as
begin
 with cartelle (link_id, parent_link_id, node_id, node_title, livello)
 as
 (select link_id, parent_link_id, node_id, node_title, 0 as livello
    from vw_links 
     where link_id = @link_id
   union all select e.link_id, e.parent_link_id, e.node_id, e.node_title, livello - 1
    from vw_links e
    inner join cartelle d on d.parent_link_id = e.link_id)
 select c.link_id, c.parent_link_id, c.node_id, c.node_title, c.livello
   , (select count(*) from m_links where parent_link_id = c.link_id) as cc_childs
  from cartelle c order by c.livello
end


---- procedure: sub_links
-- sub_links 8
if object_id('sub_links', 'p') is not null
	drop proc sub_links;
go

create procedure sub_links(@link_id int)
as
begin
 with cartelle (link_id, parent_link_id, node_id, node_title, livello)
 as
 (select link_id, parent_link_id, node_id, node_title, 0 as livello
    from vw_links 
     where link_id = @link_id
   union all select e.link_id, e.parent_link_id, e.node_id, e.node_title, livello + 1
    from vw_links e
    inner join cartelle d on d.link_id = e.parent_link_id)
 select c.link_id, c.parent_link_id, c.node_id, c.node_title, c.livello
   , (select count(*) from m_links where parent_link_id = c.link_id) as cc_childs
  from cartelle c order by c.livello
end
