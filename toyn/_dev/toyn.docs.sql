-- docs
--  select * from docs
drop table docs;
create table docs (id_doc int not null identity(1,1) primary key, title varchar(100) not null, sub_title varchar(200)
	, dt_ins datetime not null default getdate(), dt_upd datetime);

-- test
truncate table docs;
insert into docs (title, sub_title)
 values ('TOYN', 'i tuoi appunti');


-- tags
drop table tags;
create table tags (id_tag int not null identity(1,1) primary key, id_parent_tag int , title varchar(100) not null, sub_title varchar(200)
	, dt_ins datetime not null default getdate(), dt_upd datetime);
