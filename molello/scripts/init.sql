-- DEV_NODES
--  select * from dev_nodes
if exists (select * from sysobjects where id = object_id('dev_nodes') and objectproperty(id, 'isusertable') = 1)
begin
 drop table dev_nodes;
end
go

create table DEV_NODES (ID_NODE int identity(1,1) not null primary key
  , COD_TP_NODE varchar(10), COD_STATE_NODE varchar(10), TITLE_NODE varchar(50) not null);

create index IDX_DEV_NODES_2 on DEV_NODES (title_node);
go


-- DEV_NODES_LINK
--  select * from dev_nodes_link
if exists (select * from sysobjects where id = object_id('dev_nodes_link') and objectproperty(id, 'isusertable') = 1)
begin
 drop table dev_nodes_link;
end
go

create table DEV_NODES_LINK (ID_NODE int not null, ID_NODE_PARENT int not null);

create unique index IDX_DEV_NODES_LINK on DEV_NODES_LINK (id_node, id_node_parent);
go


-- DEV_TP_NODES
--  select * from dev_tp_nodes
if exists (select * from sysobjects where id = object_id('dev_tp_nodes') and objectproperty(id, 'isusertable') = 1)
begin
 drop table dev_tp_nodes;
end
go

create table DEV_TP_NODES (COD_TP_NODE varchar(10) primary key, TITLE_TP_NODE varchar(50) not null, DES_TP_NODE varchar(250) not null);
go

insert into dev_tp_nodes (cod_tp_node, title_tp_node, des_tp_node)
 values ('prj', 'progetto', 'progetto da portare a termine'), ('mission', 'missione', 'missione da portare a termine')
  , ('task', 'attività', 'attività da portare a termine'), ('expiry', 'scadenza', 'scadenza che verrà')
  , ('tile', 'piastrella', 'elemento che compone l''insieme'), ('infos', 'informazioni', 'insieme di informazioni');
go


-- DEV_STATE_NODES
--  select * from dev_state_nodes
if exists (select * from sysobjects where id = object_id('dev_state_nodes') and objectproperty(id, 'isusertable') = 1)
begin
 drop table dev_state_nodes;
end
go

create table DEV_STATE_NODES (COD_STATE_NODE varchar(10) primary key, TITLE_STATE_NODE varchar(50) not null);
go

insert into dev_state_nodes (cod_state_node, title_state_node)
 values ('begin', 'iniziata'), ('work', 'lavori in corso'), ('susp', 'sospesa'), ('broken', 'interrotta')
  , ('ok', 'finita');
go


-- test
--  select * from dev_nodes
--  select * from dev_nodes_link
insert into dev_nodes (cod_tp_node, cod_state_node, title_node)
 values ('mission', null, 'Mercedes Classe A'), ('mission', null, 'Alfa 156'), ('mission', null, 'Jippone');
insert into dev_nodes (cod_tp_node, cod_state_node, title_node)
 values ('task', null, 'prima vista')
  , ('expiry', null, 'valutazione a quarto')
  , ('tile', null, 'annunci');

insert into dev_nodes_link(id_node, id_node_parent)
 select n.id_node, (select id_node from dev_nodes where title_node = 'Mercedes Classe A')
  from dev_nodes n where n.title_node = 'prima vista';
insert into dev_nodes_link(id_node, id_node_parent)
 select n.id_node, (select id_node from dev_nodes where title_node = 'Alfa 156')
  from dev_nodes n where n.title_node = 'valutazione a quarto';
insert into dev_nodes_link(id_node, id_node_parent)
 select n.id_node, (select id_node from dev_nodes where title_node = 'Jippone')
  from dev_nodes n where n.title_node = 'annunci';
go