drop table clients;
create table clients (id_client int not null identity(1,1) primary key, client_key varchar(20) not null, interval_ss int not null
	, machine_ip varchar(16), machine_name varchar(50), session_id varchar(100), dt_session_id datetime
	, dt_ins datetime not null default getdate(), dt_refresh datetime not null);
create unique index idx_clients on clients (client_key);
create index idx_clients_2 on clients (session_id);

drop table clients_cmds;
create table clients_cmds (id_client_cmd int not null identity(1,1) primary key, client_key varchar(20) not null
	, cmd varchar(max) not null, dt_ins datetime not null default getdate());
