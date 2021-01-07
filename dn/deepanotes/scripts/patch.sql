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

drop table client_cmds;
go

create function [dbo].[folder_parents](@folder_id integer)
returns table
as
return
	with dn_folders_tree (synch_folder_id, folder_id, parent_id, folder_name, lvl)
	 as
	  (select synch_folder_id, folder_id, parent_id, folder_name, 1 as lvl 
	   from dn_folders with(nolock) where folder_id = @folder_id
	   union all select dn_folders.synch_folder_id, dn_folders.folder_id, dn_folders.parent_id, dn_folders.folder_name, dn_folders_tree.lvl + 1
		from dn_folders with(nolock) 
		inner join dn_folders_tree on dn_folders.folder_id = dn_folders_tree.parent_id)
	select synch_folder_id, folder_id, parent_id, folder_name, ((select max(lvl) from dn_folders_tree) - lvl) + 1 as lvl
	 from dn_folders_tree ft with(nolock)
	union select synch_folder_id, null as folder_id, null as parent_id, title as folder_name, 0 as lvl
	 from synch_folders
	  where synch_folder_id = (select top 1 synch_folder_id from dn_folders_tree);
go


--drop table dn_search_text
create table dn_search_text (search_id int not null identity(1,1) primary key, session_id varchar(100) not null, search_text varchar(max), dt_ins datetime not null default getdate());
create unique index idx_dn_search_text on dn_search_text (session_id);
go

--drop table dn_search_tasks
create table dn_search_tasks (search_id int not null, task_id int not null, synch_folder_id int not null, folder_id int, file_id int);
create unique index idx_dn_search_tasks on dn_search_tasks (search_id, task_id);
create index idx_dn_search_tasks_2 on dn_search_tasks (search_id);
create index idx_dn_search_tasks_3 on dn_search_tasks (file_id);
create index idx_dn_search_tasks_4 on dn_search_tasks (search_id, file_id);
go

--drop table dn_search_folders
create table dn_search_folders (search_id int not null, synch_folder_id int not null, folder_id int null);
create unique index idx_dn_search_folders on dn_search_folders (search_id, synch_folder_id, folder_id);
create index idx_dn_search_folders_2 on dn_search_folders (search_id);
go