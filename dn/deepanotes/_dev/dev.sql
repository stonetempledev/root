select sf.* from synch_folders sf 

-- synch_folders
drop table synch_folders
create table synch_folders (synch_folder_id int not null identity(1,1) primary key
 , [title] varchar(50) not null, [code] varchar(30) not null, [des] varchar(250), [synch_path] varchar(250) not null
 , [client_path] varchar(250) not null, [user] varchar(30), [password] varchar(30));
create index idx_synch_folders on synch_folders ([code]);

truncate table synch_folders;
insert into synch_folders ([title], [code], [des], [synch_path], [client_path], [user], [password]) 
 values ('Documenti Aziendali', 'deepa_docs', 'contenuti aziendali', 'C:\tmp\dn\deepa', 'C:\tmp\dn\deepa', null, null)
  , ('Deepa Notes', 'deepa_notes', 'regole, documentazione utilizzo deepa notes', 'C:\_todd\root\dn\deepanotes\_docs', 'C:\_todd\root\dn\deepanotes\_docs', null, null)
  , ('My Notes', 'my_notes', 'contenuti personali', 'C:\tmp\dn\my_notes', 'C:\tmp\dn\my_notes', null, null); 

-- synch_settings
drop table synch_settings
create table synch_settings ([name] varchar(50) not null, [des] varchar(250), [value] varchar(250));
create index idx_synch_settings on synch_settings ([name]);

truncate table synch_settings
--insert into synch_settings ([name], [des], [value])
-- values ('url_deepanotes', 'indirizzo deepanotes', 'http://localhost/deepanotes');

-- fs_folders
drop table fs_folders;
create table fs_folders (synch_folder_id int not null, folder_id bigint identity(1,1) primary key not null
 , parent_id bigint null, folder_name varchar(150) not null
 , dt_ins datetime not null default getdate(), readed bit null);
create index idx_fs_folders on fs_folders (parent_id);

-- fs_files
drop table fs_files;
create table fs_files ([file_id] bigint identity(1,1) primary key not null
 , folder_id bigint not null, [file_name] varchar(150) not null
 , dt_ins datetime not null default getdate(), readed bit null);
create index idx_fs_files on fs_files (folder_id);
	
