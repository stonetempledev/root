drop table dn_tasks_contents
go

create table dn_files_contents (file_content_id bigint not null identity(1,1) primary key, file_id bigint
	, extension varchar(25), content varchar(max), dt_ins datetime not null default getdate(), dt_upd datetime);
go

create unique index idx_dn_files_contents on dn_files_contents (file_id);
go



drop table dn_tasks_notes
go

create table dn_tasks_notes (task_id int not null primary key, file_id int not null, content varchar(max), dt_ins datetime not null, dt_upd datetime)
go

create index dn_tasks_notes on dn_tasks_notes (file_id)
go