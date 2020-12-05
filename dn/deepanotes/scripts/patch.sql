-- clients
truncate table clients
alter table clients add machine_name varchar(100) not null
alter table clients add interval_ss int not null

