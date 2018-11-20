/* MOLELLO
*/

-- M_NODES
--  select * from M_NODES
if exists (select * from sysobjects where id = object_id('m_nodes') and objectproperty(id, 'isusertable') = 1)
 drop table m_nodes;
go

create table M_NODES (ID_NODE int identity(1,1) not null primary key, NODE_TITLE varchar(50) not null);

create index IDX_M_NODES on M_NODES (node_title);
go


-- M_LINKS
--  select * from m_links
if exists (select * from sysobjects where id = object_id('m_links') and objectproperty(id, 'isusertable') = 1)
 drop table m_links;
go

create table M_LINKS (ID_NODE int not null, ID_NODE_PARENT int not null, PATH_IDS varchar(200) not null);

create unique index IDX_M_LINKS on M_LINKS (id_node, id_node_parent);
create index IDX_M_LINKS_2 on M_LINKS (id_node_parent);
create index IDX_M_LINKS_3 on M_LINKS (id_node);
go


-- M_ITEMS
--  select * from M_ITEMS
if exists (select * from sysobjects where id = object_id('m_items') and objectproperty(id, 'isusertable') = 1)
 drop table m_items;
go

create table M_ITEMS (ID_ITEM int identity(1,1) not null primary key, ID_NODE int not null, ITEM_TITLE varchar(50) not null);

create unique index IDX_M_ITEMS on M_ITEMS (id_node, item_title);
create index IDX_M_ITEMS_2 on M_ITEMS (id_node);
go

---- T_TP_ELEMENTS
----  select * from t_tp_elements
--if exists (select * from sysobjects where id = object_id('t_tp_elements') and objectproperty(id, 'isusertable') = 1)
-- drop table t_tp_elements;
--go

--create table T_TP_ELEMENTS (COD_TP_ELEMENT varchar(10) primary key, TITLE_TP_ELEMENT varchar(50) not null, DES_TP_ELEMENT varchar(250) not null);
--go

--insert into t_tp_elements (cod_tp_element, title_tp_element, des_tp_element)
-- values ('prj', 'progetto', 'progetto da portare a termine')
--  , ('section', 'sezione', 'elemento che compone l''insieme')
--  , ('task', 'attività', 'attività/scadenza da portare a termine')
--  , ('item', 'voce semplice', 'informazioni che vanno a descrivere elementi più complessi');
--go


---- T_STATES
----  select * from T_STATES
--if exists (select * from sysobjects where id = object_id('t_states') and objectproperty(id, 'isusertable') = 1)
-- drop table t_states;
--go

--create table T_STATES (COD_STATE varchar(15) primary key, TITLE_STATE varchar(50) not null);
--go

--insert into t_states (cod_state, title_state)
-- values ('started', 'iniziata'), ('in_progress', 'lavori in corso'), ('suspended', 'sospesa'), ('breaked', 'interrotta')
--  , ('completed', 'finita');
--go


