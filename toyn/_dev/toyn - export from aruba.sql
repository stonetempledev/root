-- ESPORTAZIONE ARUBA -> TOYN


-- tipo_evento


-- select * from tipieventi
delete from toyn.toyn.dbo.[objects] where object_type = 'tipo_evento' and zz_aruba_id is not null;

delete oa from toyn.toyn.dbo.[objects_attrs_text] oa
 where not exists (select top 1 1 from toyn.toyn.dbo.[objects] where [object_id] = oa.[object_id]);
go

insert into toyn.toyn.dbo.[objects] (object_type, object_title, [user_id], zz_aruba_id)
 select 'tipo_evento' as object_type, ar.tipoevento as object_title, u.[user_id], ar.IDTipoEvento as zz_aruba_id
  from tipieventi ar
  join toyn.toyn.dbo.[users] u on u.nome in ('papino', 'famigliola', 'gi');
go

insert into toyn.toyn.dbo.[objects_attrs_text] ([object_id], attribute_id, [value])
 select o.[object_id], oa.attribute_id, ar.note as [value]
  from toyn.toyn.dbo.[objects] o 
  join tipieventi ar on ar.IDTipoEvento = o.zz_aruba_id
  join toyn.toyn.dbo.[objects_attrs] oa on oa.object_type = o.object_type and oa.user_id = o.user_id and oa.attribute_code = 'notes'
  where o.object_type = 'tipo_evento' and ar.note is not null
go


-- tipo_ricevuta


-- select * from tipispesa
delete from toyn.toyn.dbo.[objects] where object_type = 'tipo_ricevuta' and zz_aruba_id is not null;

delete oa from toyn.toyn.dbo.[objects_attrs_text] oa
 where not exists (select top 1 1 from toyn.toyn.dbo.[objects] where [object_id] = oa.[object_id]);
delete oa from toyn.toyn.dbo.[objects_attrs_flag] oa
 where not exists (select top 1 1 from toyn.toyn.dbo.[objects] where [object_id] = oa.[object_id]);
go

insert into toyn.toyn.dbo.[objects] (object_type, object_code, object_title, [user_id], zz_aruba_id)
 select 'tipo_ricevuta' as object_type, ar.codspesa as object_code
   , ar.tipospesa as object_title, u.[user_id], ar.IDTipoSpesa as zz_aruba_id
  from tipispesa ar
  join toyn.toyn.dbo.[users] u on u.nome in ('papino', 'famigliola', 'gi');
go

insert into toyn.toyn.dbo.[objects_attrs_text] ([object_id], attribute_id, [value])
 select o.[object_id], oa.attribute_id, ar.note as [value]
  from toyn.toyn.dbo.[objects] o
  join tipispesa ar on ar.IDTipoSpesa = o.zz_aruba_id 
  join toyn.toyn.dbo.[objects_attrs] oa on oa.object_type = o.object_type and oa.user_id = o.user_id and oa.attribute_code = 'notes'
  where o.object_type = 'tipo_ricevuta' and ar.note is not null
go

insert into toyn.toyn.dbo.[objects_attrs_flag] ([object_id], attribute_id, [value])
 select o.[object_id], oa.attribute_id, ar.entrata as [value]
  from toyn.toyn.dbo.[objects] o 
  join tipispesa ar on ar.IDTipoSpesa = o.zz_aruba_id
  join toyn.toyn.dbo.[objects_attrs] oa on oa.object_type = o.object_type and oa.user_id = o.user_id and oa.attribute_code = 'entrata'
  where o.object_type = 'tipo_ricevuta' and ar.entrata = 1
go


-- evento


-- select * from eventi
-- select * from utenti
delete from toyn.toyn.dbo.[objects] where object_type = 'evento' and zz_aruba_id is not null;

delete oa from toyn.toyn.dbo.[objects_attrs_text] oa
 where not exists (select top 1 1 from toyn.toyn.dbo.[objects] where [object_id] = oa.[object_id]);
delete oa from toyn.toyn.dbo.[objects_attrs_object] oa
 where not exists (select top 1 1 from toyn.toyn.dbo.[objects] where [object_id] = oa.[object_id]);
delete oa from toyn.toyn.dbo.[objects_attrs_datetime] oa
 where not exists (select top 1 1 from toyn.toyn.dbo.[objects] where [object_id] = oa.[object_id]);
go

insert into toyn.toyn.dbo.[objects] (object_type, object_code, object_title, [user_id], zz_aruba_id)
 select 'evento' as object_type, null as object_code
   , ar.evento as object_title, tu.[user_id], ar.IDEvento as zz_aruba_id
  from eventi ar
  join utenti u on u.idutente = ar.idutente
  join toyn.toyn.dbo.[users] tu on tu.nome collate SQL_Latin1_General_CP1_CI_AS = u.Nome
go

insert into toyn.toyn.dbo.[objects_attrs_text] ([object_id], attribute_id, [value])
 select o.[object_id], oa.attribute_id, ar.note as [value]
  from toyn.toyn.dbo.[objects] o 
  join eventi ar on ar.IDEvento = o.zz_aruba_id
  join toyn.toyn.dbo.[objects_attrs] oa on oa.object_type = o.object_type and oa.user_id = o.user_id and oa.attribute_code = 'notes'
  where o.object_type = 'evento' and ar.note is not null
go

insert into toyn.toyn.dbo.[objects_attrs_datetime] ([object_id], attribute_id, [value])
 select o.[object_id], oa.attribute_id, ar.datada as [value]
  from toyn.toyn.dbo.[objects] o 
  join eventi ar on ar.IDEvento = o.zz_aruba_id
  join toyn.toyn.dbo.[objects_attrs] oa on oa.object_type = o.object_type and oa.user_id = o.user_id and oa.attribute_code = 'dal'
  where o.object_type = 'evento' and ar.datada is not null
go

insert into toyn.toyn.dbo.[objects_attrs_datetime] ([object_id], attribute_id, [value])
 select o.[object_id], oa.attribute_id, ar.dataa as [value]
  from toyn.toyn.dbo.[objects] o 
  join eventi ar on ar.IDEvento = o.zz_aruba_id
  join toyn.toyn.dbo.[objects_attrs] oa on oa.object_type = o.object_type and oa.user_id = o.user_id and oa.attribute_code = 'al'
  where o.object_type = 'evento' and ar.dataa is not null
go

insert into toyn.toyn.dbo.[objects_attrs_object] ([object_id], attribute_id, [value])
 select o.[object_id], oa.attribute_id, ot.[object_id] as [value]
  from toyn.toyn.dbo.[objects] o 
  join eventi ar on ar.IDEvento = o.zz_aruba_id 
  join toyn.toyn.dbo.[objects] ot on ot.object_type = 'tipo_evento' and ot.user_id = o.user_id and ot.zz_aruba_id = ar.IDTipoEvento
  join toyn.toyn.dbo.[objects_attrs] oa on oa.object_type = o.object_type and oa.user_id = o.user_id and oa.attribute_code = 'tipo_evento'
  where o.object_type = 'evento' and ar.idtipoevento is not null
go


-- ricevuta


delete from toyn.toyn.dbo.[objects] where object_type = 'ricevuta' and zz_aruba_id is not null;

delete oa from toyn.toyn.dbo.[objects_attrs_varchar] oa
 where not exists (select top 1 1 from toyn.toyn.dbo.[objects] where [object_id] = oa.[object_id]);
delete oa from toyn.toyn.dbo.[objects_attrs_object] oa
 where not exists (select top 1 1 from toyn.toyn.dbo.[objects] where [object_id] = oa.[object_id]);
delete oa from toyn.toyn.dbo.[objects_attrs_real] oa
 where not exists (select top 1 1 from toyn.toyn.dbo.[objects] where [object_id] = oa.[object_id]);
delete oa from toyn.toyn.dbo.[objects_attrs_datetime] oa
 where not exists (select top 1 1 from toyn.toyn.dbo.[objects] where [object_id] = oa.[object_id]);
delete oa from toyn.toyn.dbo.[objects_attrs_integer] oa
 where not exists (select top 1 1 from toyn.toyn.dbo.[objects] where [object_id] = oa.[object_id]);
go

insert into toyn.toyn.dbo.[objects] (object_type, object_code, object_title, [user_id], zz_aruba_id, dt_ins)
 select 'ricevuta' as object_type, null as object_code
   , ar.pezzo as object_title, tu.[user_id], ar.idspesa as zz_aruba_id, ar.DtIns
  from spese ar
  join utenti u on u.idutente = ar.idutente
  join toyn.toyn.dbo.[users] tu on tu.nome collate SQL_Latin1_General_CP1_CI_AS = u.Nome
go

insert into toyn.toyn.dbo.[objects_attrs_object] ([object_id], attribute_id, [value])
 select o.[object_id], oa.attribute_id, ot.[object_id] as [value]
  from toyn.toyn.dbo.[objects] o 
  join spese ar on ar.idspesa = o.zz_aruba_id 
  join toyn.toyn.dbo.[objects] ot on ot.object_type = 'evento' and ot.user_id = o.user_id and ot.zz_aruba_id = ar.idevento
  join toyn.toyn.dbo.[objects_attrs] oa on oa.object_type = o.object_type and oa.user_id = o.user_id and oa.attribute_code = 'evento'
  where o.object_type = 'ricevuta' and ar.idevento is not null
go

insert into toyn.toyn.dbo.[objects_attrs_object] ([object_id], attribute_id, [value])
 select o.[object_id], oa.attribute_id, ot.[object_id] as [value]
  from toyn.toyn.dbo.[objects] o 
  join spese ar on ar.idspesa = o.zz_aruba_id 
  join toyn.toyn.dbo.[objects] ot on ot.object_type = 'tipo_ricevuta' and ot.user_id = o.user_id and ot.zz_aruba_id = ar.IDTipoSpesa
  join toyn.toyn.dbo.[objects_attrs] oa on oa.object_type = o.object_type and oa.user_id = o.user_id and oa.attribute_code = 'tipo_ricevuta'
  where o.object_type = 'ricevuta' and ar.idtipospesa is not null
go

insert into toyn.toyn.dbo.[objects_attrs_integer] ([object_id], attribute_id, [value])
 select o.[object_id], oa.attribute_id, ar.Quantita as [value]
  from toyn.toyn.dbo.[objects] o 
  join spese ar on ar.idspesa = o.zz_aruba_id
  join toyn.toyn.dbo.[objects_attrs] oa on oa.object_type = o.object_type and oa.user_id = o.user_id and oa.attribute_code = 'qta'
  where o.object_type = 'ricevuta' and ar.quantita is not null
go

insert into toyn.toyn.dbo.[objects_attrs_datetime] ([object_id], attribute_id, [value])
 select o.[object_id], oa.attribute_id, ar.[data] as [value]
  from toyn.toyn.dbo.[objects] o 
  join spese ar on ar.idspesa = o.zz_aruba_id
  join toyn.toyn.dbo.[objects_attrs] oa on oa.object_type = o.object_type and oa.user_id = o.user_id and oa.attribute_code = 'data'
  where o.object_type = 'ricevuta' and ar.data is not null
go

insert into toyn.toyn.dbo.[objects_attrs_real] ([object_id], attribute_id, [value])
 select o.[object_id], oa.attribute_id, ar.prezzo as [value]
  from toyn.toyn.dbo.[objects] o 
  join spese ar on ar.idspesa = o.zz_aruba_id
  join toyn.toyn.dbo.[objects_attrs] oa on oa.object_type = o.object_type and oa.user_id = o.user_id and oa.attribute_code = 'prezzo'
  where o.object_type = 'ricevuta' and ar.prezzo is not null
go
