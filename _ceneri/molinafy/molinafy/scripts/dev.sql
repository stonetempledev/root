-- client
select * from users
select * from client_ips
select * from client_logged

-- nodes

/*
drop table nodes
create table nodes (id_node int identity(1,1) primary key not null, id_parent_node int
 , title_node varchar(100) not null, dt_ins datetime not null, dt_upd datetime, user_ins int, user_upd int);
*/

select * from nodes

truncate table nodes;
insert into nodes (title_node, dt_ins) values ('disney', getdate());
insert into nodes (title_node, dt_ins) values ('dev', getdate());
insert into nodes (title_node, dt_ins) values ('viaggi', getdate());

insert into nodes (id_parent_node, title_node, dt_ins) 
 select n.id_node, 'paperopoli', getdate() from nodes n where n.title_node = 'disney'
insert into nodes (id_parent_node, title_node, dt_ins) 
 select n.id_node, 'topolinia', getdate() from nodes n where n.title_node = 'disney'
insert into nodes (id_parent_node, title_node, dt_ins) 
 select n.id_node, 'plutonia', getdate() from nodes n where n.title_node = 'disney'

insert into nodes (id_parent_node, title_node, dt_ins) 
 select n.id_node, 'libraries', getdate() from nodes n where n.title_node = 'dev'
insert into nodes (id_parent_node, title_node, dt_ins) 
 select n.id_node, 'projects', getdate() from nodes n where n.title_node = 'dev'

insert into nodes (id_parent_node, title_node, dt_ins) 
 select n.id_node, 'terra', getdate() from nodes n where n.title_node = 'viaggi'
insert into nodes (id_parent_node, title_node, dt_ins) 
 select n.id_node, 'mare', getdate() from nodes n where n.title_node = 'viaggi'
insert into nodes (id_parent_node, title_node, dt_ins) 
 select n.id_node, 'collinare', getdate() from nodes n where n.title_node = 'viaggi'
insert into nodes (id_parent_node, title_node, dt_ins) 
 select n.id_node, 'on the river', getdate() from nodes n where n.title_node = 'viaggi'


-- docs

/*

drop table docs;
create table docs(id_node int, id_doc int identity(1,1) primary key not null, title_doc varchar(100) not null, des_doc varchar(250) null
 , dt_ins datetime not null, dt_upd datetime null default getdate(), user_ins int null, user_upd int null);

create index idx_docs_node on docs (id_node);

drop table docs_sections;
create table docs_sections (id_doc int not null, id_section int identity(1,1) primary key not null, id_section_parent int 
 , title_section varchar(100) null, dt_ins datetime not null, dt_upd datetime, user_ins int, user_upd int);

drop table docs_pars;
create table docs_pars (id_par int identity(1,1) primary key not null, id_doc int not null, id_section int null 
 , text_paragraph varchar(max) not null, dt_ins datetime not null, dt_upd datetime, user_ins int, user_upd int);
*/

select * from docs
select * from docs_sections
select * from docs_pars

truncate table docs;
insert into docs (title_doc, des_doc, dt_ins)
 values ('Molinafy', 'the beginning', getdate())

