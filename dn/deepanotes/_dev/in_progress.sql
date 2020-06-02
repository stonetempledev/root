-- synch_machines
drop table synch_machines;
create table synch_machines (synch_machine_id int not null identity(1,1) primary key 
 , pc_name varchar(50) not null, pc_des varchar(250) not null, ip_address varchar(15)
 , seconds int not null, active bit not null, [state] varchar(10), dt_start datetime, dt_stop datetime, dt_lastsynch datetime
 , c_folders int, c_files int, c_deleted int, s_synch int);
create unique index idx_synch_machines on synch_machines (pc_name);
go

-- synch_folders
drop table synch_folders
create table synch_folders (synch_folder_id int not null identity(1,1) primary key
 , pc_name varchar(50) not null, [title] varchar(50) not null, [des] varchar(250)
 , [local_path] varchar(250) not null, [http_path] varchar(250) not null
 , [user] varchar(30), [password] varchar(30));

truncate table synch_folders;
insert into synch_folders ([pc_name], [title], [des], [local_path], http_path, [user], [password]) 
 values ('dell-pc', 'Documenti Aziendali', 'contenuti aziendali', 'C:\tmp\dn\deepa', 'http://192.168.1.11/dndocs/', null, null)
  , ('dell-pc', 'Documentazione Deepa-Notes', 'manutenzione, funzionamento deepa-notes', 'C:\_todd\root\dn\deepanotes\_dev', 'http://192.168.1.11/deepanotes/_dev', null, null); 

-- synch_settings
/*
drop table synch_settings
create table synch_settings ([name] varchar(50) not null, [des] varchar(250), [value] varchar(250));
create index idx_synch_settings on synch_settings ([name]);

truncate table synch_settings
--insert into synch_settings ([name], [des], [value])
-- values ('url_deepanotes', 'indirizzo deepanotes', 'http://localhost/deepanotes');
*/

-- dn_folders
drop table dn_folders;
create table dn_folders (synch_folder_id int not null, folder_id bigint identity(1,1) primary key not null
  , parent_id bigint null, folder_name varchar(150) not null, dt_ins datetime not null default getdate(), readed bit null);
create index idx_fs_folders on dn_folders (synch_folder_id, parent_id);

-- dn_files
drop table dn_files;
create table dn_files (synch_folder_id int not null, [file_id] bigint identity(1,1) primary key not null
  , folder_id bigint null, [file_name] varchar(150) not null, dt_ins datetime not null default getdate(), readed bit null);
create index idx_fs_files on dn_files (synch_folder_id, folder_id);

-- dn_tasks
drop table dn_tasks;
create table dn_tasks ([task_id] bigint identity(1,1) primary key not null
 , [folder_id] bigint, [file_id] bigint, title varchar(150) not null
 , [user] varchar(50), [stato] varchar(50), dt_upd datetime not null default getdate(), readed bit null);
create unique index idx_dn_tasks on dn_tasks (folder_id, [file_id]);
go

-- dn_task_states
--  in_corso, sospeso, fatto, da_fare, incompleto, urgente
drop table dn_task_states
create table dn_task_states (free_txt varchar(25) not null, task_state varchar(25) not null);
create unique index idx_ts on dn_task_states (free_txt);
create unique index idx_ts_2 on dn_task_states (free_txt, task_state);
go

truncate table dn_task_states
insert into dn_task_states (free_txt, task_state)
 values ('in_corso', 'in_corso'), ('in corso', 'in_corso'), ('in_progress', 'in_corso'), ('in progress', 'in_corso'), ('progress', 'in_corso'), ('iniziato', 'in_corso'), ('iniziata', 'in_corso')
  , ('sospeso', 'sospeso'), ('fermo', 'sospeso'), ('bloccato', 'sospeso'), ('lock', 'sospeso')
  , ('fatto', 'fatto'), ('ok', 'fatto'), ('finito', 'fatto')
  , ('da_fare', 'da_fare'), ('todo', 'da_fare'), ('da fare', 'da_fare')
  , ('urgente', 'urgente'), ('il prossimo', 'da_fare'), ('il_prossimo', 'da_fare'), ('prossimo', 'da_fare')
  , ('incompleto', 'incompleto'), ('ko', 'incompleto'), ('problemi', 'incompleto'), ('con problemi', 'incompleto'), ('non riesco', 'incompleto');
go


/*
drop function [dbo].[folder_path](@folder_id int)
returns varchar(max)
as
begin      

 declare @path varchar(max);

 with dn_folders_tree (synch_folder_id, folder_id, parent_id, folder_name, lvl)
 as
 (
  select synch_folder_id, folder_id, parent_id, folder_name, 0 as lvl 
  from dn_folders where folder_id = @folder_id
  union all select dn_folders.synch_folder_id, dn_folders.folder_id, dn_folders.parent_id, dn_folders.folder_name, dn_folders_tree.lvl + 1
   from dn_folders 
   inner join dn_folders_tree on dn_folders.folder_id = dn_folders_tree.parent_id
  )
 select @path = coalesce(@path + '/', '') + ft.folder_name
  from dn_folders_tree ft
  order by ft.lvl
  
 return (case when @path is not null then @path + '/' else null end);
 
end
GO
*/

/*
synch_machines
--insert into synch_machines (pc_name, pc_des, seconds, active)
-- values ('dell-pc', 'portatile casalingo', 10, 1);
update synch_machines set state = null where synch_machine_id = 1
update synch_machines set active = 0 where synch_machine_id = 1
update synch_machines set active = 1 where synch_machine_id = 1
*/

