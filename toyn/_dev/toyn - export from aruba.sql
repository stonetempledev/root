-- ESPORTAZIONE ARUBA -> TOYN


-- tipo_evento


-- select * from tipieventi
delete from toyn.toyn.dbo.[objects] where object_type_id = (select top 1 object_type_id from toyn.toyn.dbo.[objects_types] where object_type = 'tipo_evento' )
 and zz_aruba_id is not null;

delete oa from toyn.toyn.dbo.[objects_attrs_text] oa
 where not exists (select top 1 1 from toyn.toyn.dbo.[objects] where [object_id] = oa.[object_id]);
go

insert into toyn.toyn.dbo.[objects] (object_type_id, object_title, [user_id], zz_aruba_id)
 select ot.object_type_id, ar.tipoevento as object_title, u.[user_id], ar.IDTipoEvento as zz_aruba_id
  from tipieventi ar
  join toyn.toyn.dbo.[objects_types] ot on ot.object_type = 'tipo_evento'
  join toyn.toyn.dbo.[users] u on u.nome in ('papino', 'famigliola', 'gi');
go

insert into toyn.toyn.dbo.[objects_attrs_text] ([object_id], attribute_id, [value])
 select o.[object_id], oa.attribute_id, ar.note as [value]
  from toyn.toyn.dbo.[objects] o 
  join toyn.toyn.dbo.[objects_types] ot on ot.object_type_id = o.object_type_id
  join tipieventi ar on ar.IDTipoEvento = o.zz_aruba_id
  join toyn.toyn.dbo.[objects_attrs] oa on oa.object_type_id = ot.object_type_id and oa.attribute_code = 'notes'
  where ot.object_type = 'tipo_evento' and ar.note is not null
go


-- tipo_ricevuta


-- select * from tipispesa
delete from toyn.toyn.dbo.[objects] where object_type_id = (select top 1 object_type_id from toyn.toyn.dbo.[objects_types] where object_type = 'tipo_ricevuta' ) 
 and zz_aruba_id is not null;

delete oa from toyn.toyn.dbo.[objects_attrs_text] oa
 where not exists (select top 1 1 from toyn.toyn.dbo.[objects] where [object_id] = oa.[object_id]);
delete oa from toyn.toyn.dbo.[objects_attrs_flag] oa
 where not exists (select top 1 1 from toyn.toyn.dbo.[objects] where [object_id] = oa.[object_id]);
go

insert into toyn.toyn.dbo.[objects] (object_type_id, object_code, object_title, [user_id], zz_aruba_id)
 select ot.object_type_id, ar.codspesa as object_code
   , ar.tipospesa as object_title, u.[user_id], ar.IDTipoSpesa as zz_aruba_id
  from tipispesa ar
  join toyn.toyn.dbo.[objects_types] ot on ot.object_type = 'tipo_ricevuta'
  join toyn.toyn.dbo.[users] u on u.nome in ('papino', 'famigliola', 'gi');
go

insert into toyn.toyn.dbo.[objects_attrs_text] ([object_id], attribute_id, [value])
 select o.[object_id], oa.attribute_id, ar.note as [value]
  from toyn.toyn.dbo.[objects] o
  join toyn.toyn.dbo.[objects_types] ot on ot.object_type_id = o.object_type_id
  join tipispesa ar on ar.IDTipoSpesa = o.zz_aruba_id 
  join toyn.toyn.dbo.[objects_attrs] oa on oa.object_type_id = ot.object_type_id and oa.attribute_code = 'notes'
  where ot.object_type = 'tipo_ricevuta' and ar.note is not null
go

insert into toyn.toyn.dbo.[objects_attrs_flag] ([object_id], attribute_id, [value])
 select o.[object_id], oa.attribute_id, ar.entrata as [value]
  from toyn.toyn.dbo.[objects] o 
  join toyn.toyn.dbo.[objects_types] ot on ot.object_type_id = o.object_type_id
  join tipispesa ar on ar.IDTipoSpesa = o.zz_aruba_id
  join toyn.toyn.dbo.[objects_attrs] oa on oa.object_type_id = ot.object_type_id and oa.attribute_code = 'entrata'
  where ot.object_type = 'tipo_ricevuta' and ar.entrata = 1
go


-- evento


-- select * from eventi
-- select * from utenti
delete from toyn.toyn.dbo.[objects] where object_type_id = (select top 1 object_type_id from toyn.toyn.dbo.[objects_types] where object_type = 'evento' ) 
 and zz_aruba_id is not null;

delete oa from toyn.toyn.dbo.[objects_attrs_text] oa
 where not exists (select top 1 1 from toyn.toyn.dbo.[objects] where [object_id] = oa.[object_id]);
delete oa from toyn.toyn.dbo.[objects_attrs_object] oa
 where not exists (select top 1 1 from toyn.toyn.dbo.[objects] where [object_id] = oa.[object_id]);
delete oa from toyn.toyn.dbo.[objects_attrs_datetime] oa
 where not exists (select top 1 1 from toyn.toyn.dbo.[objects] where [object_id] = oa.[object_id]);
go

insert into toyn.toyn.dbo.[objects] (object_type_id, object_code, object_title, [user_id], zz_aruba_id)
 select ot.object_type_id, null as object_code
   , ar.evento as object_title, tu.[user_id], ar.IDEvento as zz_aruba_id
  from eventi ar
  join toyn.toyn.dbo.[objects_types] ot on ot.object_type = 'evento'
  join utenti u on u.idutente = ar.idutente
  join toyn.toyn.dbo.[users] tu on tu.nome collate SQL_Latin1_General_CP1_CI_AS = u.Nome
go

insert into toyn.toyn.dbo.[objects_attrs_text] ([object_id], attribute_id, [value])
 select o.[object_id], oa.attribute_id, ar.note as [value]
  from toyn.toyn.dbo.[objects] o 
  join toyn.toyn.dbo.[objects_types] ot on ot.object_type_id = o.object_type_id
  join eventi ar on ar.IDEvento = o.zz_aruba_id
  join toyn.toyn.dbo.[objects_attrs] oa on oa.object_type_id = ot.object_type_id and oa.attribute_code = 'notes'
  where ot.object_type = 'evento' and ar.note is not null
go

insert into toyn.toyn.dbo.[objects_attrs_datetime] ([object_id], attribute_id, [value])
 select o.[object_id], oa.attribute_id, ar.datada as [value]
  from toyn.toyn.dbo.[objects] o 
  join toyn.toyn.dbo.[objects_types] ot on ot.object_type_id = o.object_type_id
  join eventi ar on ar.IDEvento = o.zz_aruba_id
  join toyn.toyn.dbo.[objects_attrs] oa on oa.object_type_id = ot.object_type_id and oa.attribute_code = 'dal'
  where ot.object_type = 'evento' and ar.datada is not null
go

insert into toyn.toyn.dbo.[objects_attrs_datetime] ([object_id], attribute_id, [value])
 select o.[object_id], oa.attribute_id, ar.dataa as [value]
  from toyn.toyn.dbo.[objects] o 
  join toyn.toyn.dbo.[objects_types] ot on ot.object_type_id = o.object_type_id
  join eventi ar on ar.IDEvento = o.zz_aruba_id
  join toyn.toyn.dbo.[objects_attrs] oa on oa.object_type_id = ot.object_type_id and oa.attribute_code = 'al'
  where ot.object_type = 'evento' and ar.dataa is not null
go

insert into toyn.toyn.dbo.[objects_attrs_object] ([object_id], attribute_id, [value])
 select o.[object_id], oa.attribute_id, ot.[object_id] as [value]
  from toyn.toyn.dbo.[objects] o 
  join toyn.toyn.dbo.[objects_types] ott on ott.object_type_id = o.object_type_id
  join eventi ar on ar.IDEvento = o.zz_aruba_id 
  join toyn.toyn.dbo.[objects] ot on ot.object_type_id = (select top 1 object_type_id from toyn.toyn.dbo.[objects_types] where object_type = 'tipo_evento' )
   and ot.user_id = o.user_id and ot.zz_aruba_id = ar.IDTipoEvento
  join toyn.toyn.dbo.[objects_attrs] oa on oa.object_type_id = ott.object_type_id and oa.attribute_code = 'tipo_evento'
  where ott.object_type = 'evento' and ar.idtipoevento is not null
go


-- ricevuta


delete from toyn.toyn.dbo.[objects] where object_type_id = (select top 1 object_type_id from toyn.toyn.dbo.[objects_types] where object_type = 'ricevuta' )  
 and zz_aruba_id is not null;

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

insert into toyn.toyn.dbo.[objects] (object_type_id, object_code, object_title, [user_id], zz_aruba_id, dt_ins)
 select ot.object_type_id, null as object_code
   , ar.pezzo as object_title, tu.[user_id], ar.idspesa as zz_aruba_id, ar.DtIns
  from spese ar
  join toyn.toyn.dbo.[objects_types] ot on ot.object_type = 'ricevuta'
  join utenti u on u.idutente = ar.idutente
  join toyn.toyn.dbo.[users] tu on tu.nome collate SQL_Latin1_General_CP1_CI_AS = u.Nome
go

insert into toyn.toyn.dbo.[objects_attrs_object] ([object_id], attribute_id, [value])
 select o.[object_id], oa.attribute_id, ot.[object_id] as [value]
  from toyn.toyn.dbo.[objects] o 
  join toyn.toyn.dbo.[objects_types] ott on ott.object_type_id = o.object_type_id
  join spese ar on ar.idspesa = o.zz_aruba_id 
  join toyn.toyn.dbo.[objects] ot on ot.object_type_id = (select top 1 object_type_id from toyn.toyn.dbo.[objects_types] where object_type = 'evento' )
   and ot.user_id = o.user_id and ot.zz_aruba_id = ar.idevento
  join toyn.toyn.dbo.[objects_attrs] oa on oa.object_type_id = ott.object_type_id and oa.attribute_code = 'evento'
  where ott.object_type = 'ricevuta' and ar.idevento is not null
go

insert into toyn.toyn.dbo.[objects_attrs_object] ([object_id], attribute_id, [value])
 select o.[object_id], oa.attribute_id, ot.[object_id] as [value]
  from toyn.toyn.dbo.[objects] o 
  join toyn.toyn.dbo.[objects_types] ott on ott.object_type_id = o.object_type_id
  join spese ar on ar.idspesa = o.zz_aruba_id 
  join toyn.toyn.dbo.[objects] ot on ot.object_type_id = (select top 1 object_type_id from toyn.toyn.dbo.[objects_types] where object_type = 'tipo_ricevuta' )
   and ot.user_id = o.user_id and ot.zz_aruba_id = ar.IDTipoSpesa
  join toyn.toyn.dbo.[objects_attrs] oa on oa.object_type_id = ott.object_type_id and oa.attribute_code = 'tipo_ricevuta'
  where ott.object_type = 'ricevuta' and ar.idtipospesa is not null
go

insert into toyn.toyn.dbo.[objects_attrs_integer] ([object_id], attribute_id, [value])
 select o.[object_id], oa.attribute_id, ar.Quantita as [value]
  from toyn.toyn.dbo.[objects] o 
  join toyn.toyn.dbo.[objects_types] ott on ott.object_type_id = o.object_type_id
  join spese ar on ar.idspesa = o.zz_aruba_id
  join toyn.toyn.dbo.[objects_attrs] oa on oa.object_type_id = ott.object_type_id and oa.attribute_code = 'qta'
  where ott.object_type = 'ricevuta' and ar.quantita is not null
go

insert into toyn.toyn.dbo.[objects_attrs_datetime] ([object_id], attribute_id, [value])
 select o.[object_id], oa.attribute_id, ar.[data] as [value]
  from toyn.toyn.dbo.[objects] o 
  join toyn.toyn.dbo.[objects_types] ott on ott.object_type_id = o.object_type_id
  join spese ar on ar.idspesa = o.zz_aruba_id
  join toyn.toyn.dbo.[objects_attrs] oa on oa.object_type_id = ott.object_type_id and oa.attribute_code = 'data'
  where ott.object_type = 'ricevuta' and ar.data is not null
go

insert into toyn.toyn.dbo.[objects_attrs_real] ([object_id], attribute_id, [value])
 select o.[object_id], oa.attribute_id, ar.prezzo as [value]
  from toyn.toyn.dbo.[objects] o 
  join toyn.toyn.dbo.[objects_types] ott on ott.object_type_id = o.object_type_id
  join spese ar on ar.idspesa = o.zz_aruba_id
  join toyn.toyn.dbo.[objects_attrs] oa on oa.object_type_id = ott.object_type_id and oa.attribute_code = 'prezzo'
  where ott.object_type = 'ricevuta' and ar.prezzo is not null
go
