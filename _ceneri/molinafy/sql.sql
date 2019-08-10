-- client_ips
--  select * from client_ips
-- drop table client_ips
create table client_ips (id_client int identity(1,1) not null primary key, ip_address varchar(30) not null
 , dt_ins datetime not null default getdate())
create index idx_client_ips on client_ips(ip_address);

-- utenti
--  select * from users
-- drop table users
create table users (id_user int identity(1,1) not null primary key, [user_name] varchar(30) not null, [password] varchar(30) not null
 , dt_ins datetime not null default getdate());
create unique index idx_users on users([user_name]);
insert into users ([user_name], [password]) values ('admin', 'admin32!');

-- client_logged
--  select * from client_logged
-- drop table client_logged
create table client_logged (id_client int not null, id_user int not null, dt_ins datetime not null default getdate());
create unique index idx_client_logged on client_logged(id_client, id_user);

select u.id_user, u.[user_name], cl.dt_ins
 from users u
 join client_logged cl on cl.id_client = 0 and cl.id_user = u.id_user