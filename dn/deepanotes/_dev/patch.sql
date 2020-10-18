-- client_cmds
CREATE TABLE [dbo].[client_cmds](
	[id_cmd] [int] IDENTITY(1,1) NOT NULL,
	[machine_ip] [varchar](16) NOT NULL,
	[command] [varchar](50) NOT NULL,
	[args] [varchar](500) NULL,
	[state] [varchar](50) NULL,
	[dt_ins] [datetime] NOT NULL default getdate(),
  PRIMARY KEY CLUSTERED ( [id_cmd] ASC )
) ON [PRIMARY]
GO

create index idx_client_cmds on client_cmds (machine_ip, [state]);
go

-- client_db_conns
create table client_db_conns (id_conn int identity(1,1) not null primary key
 , conn_name varchar(50) not null, conn_type varchar(25) not null
 , conn_string varchar(500) not null, [provider] varchar(200) not null, conn_des varchar(1500));
go

create index idx_client_db_conns on client_db_conns (conn_name, conn_type);
go

-- clients
CREATE TABLE [dbo].[clients](
	[id_client] [int] IDENTITY(1,1) NOT NULL,
	[machine_ip] [varchar](16) NOT NULL,
	[state] [varchar](50) NULL,
	[dt_ins] [datetime] NOT NULL default getdate(),
	[dt_upd] [datetime]
  PRIMARY KEY CLUSTERED ( [id_client] ASC )
) 
GO

create unique index idx_clients on clients (machine_ip);
go














alter table dn_tasks_filters add filter_order int
go

update dn_tasks_filters set filter_order = null
update dn_tasks_filters set filter_order = 10 where task_filter_id = 6
update dn_tasks_filters set filter_order = 20 where task_filter_id = 2
update dn_tasks_filters set filter_order = 30 where task_filter_id = 11
update dn_tasks_filters set filter_order = 40 where task_filter_id = 12
update dn_tasks_filters set filter_order = 50 where task_filter_id = 1
update dn_tasks_filters set filter_order = 60 where task_filter_id = 3
update dn_tasks_filters set filter_order = 70 where task_filter_id = 5
update dn_tasks_filters set filter_order = 80 where task_filter_id = 10
update dn_tasks_filters set filter_order = 90 where task_filter_id = 4
update dn_tasks_filters set filter_order = 100 where task_filter_id = 7
update dn_tasks_filters set filter_order = 110 where task_filter_id = 8
update dn_tasks_filters set filter_order = 120 where task_filter_id = 9
go


alter table dn_tasks_filters add filter_class varchar(25);
go

update dn_tasks_filters set filter_class = 'success' where task_filter_id in (4,7,8,9);
update dn_tasks_filters set filter_class = 'primary' where task_filter_id in (10);
update dn_tasks_filters set filter_class = 'secondary' where task_filter_id in (5);
go


insert into dn_tasks_filters
 values (13, 'elenco attività degli ultimi 3 giorni', 'elenco delle attività in corso più quelle aggiornate degli ultimi 3 giorni', '{[stato] = ''in_corso''} or {[diff_days] <= 2}', getdate(), 35, null)
go


update dn_tasks_filters set filter_title = 'elenco attività sospese e incomplete'
  , filter_notes = 'elenco completo di tutte le attività sospese e incomplete'
  , filter_def = '{[stato] = ''sospeso''} or {[stato] = ''incompleto''}'
 where task_filter_id = 5;
go

update dn_task_stato set [order] = [order] * 10;
go

insert into dn_task_stato 
 values ('check', 35, 'danger', 'da monitorare', 'da monitorare')
  , ('release', 5, 'danger', 'da rilasciare', 'da rilasciare');
go


insert into dn_task_free_labels 
 values ('release', 'release', null, null, null, null)
  , ('rilascia', 'release', null, null, null, 1)
  , ('monitora', 'check', null, null, null, 1);
go

insert into dn_tasks_filters 
 values (14, 'elenco attività da rilasciare', 'elenco completo di tutte le attività da rilasciare'
   , '{[stato] = ''release''}', getdate(), 65, 'danger')
  , (15, 'elenco attività da monitorare', 'elenco completo di tutte le attività da monitorare'
   , '{[stato] = ''check''}', getdate(), 67, 'danger');
go


------------------------ 6 agosto

create view vw_task_allegati
as
 select t.task_id, f.folder_id, f.file_id, f.file_name, f.extension
   , sf.http_path + dbo.folder_path(f.folder_id) + f.file_name as http_path
  from dn_files f 
  join dn_tasks t on f.folder_id = t.folder_id
  join synch_folders sf on sf.synch_folder_id = f.synch_folder_id
  where f.file_name <> 'thumbs.db' and not exists (
   select top 1 1 from dn_tasks_notes n2 
    join dn_files f2 on f2.file_id = n2.file_id
    join dn_files_info fi2 on fi2.file_name = f2.file_name
    where n2.file_id = f.file_id and n2.task_id = t.task_id)
