-- ESPORTAZIONE ARUBA -> TOYN
/*
select * from toyn.toyn.dbo.[objects_types]
select * from toyn.toyn.dbo.[objects_attrs]
select * from toyn.toyn.dbo.[objects]
*/
select * from tipieventi
select * from tipispesa
select * from eventi
select * from spese order by dtins desc

-- TOYN
select * from objects_attrs
select * from users
