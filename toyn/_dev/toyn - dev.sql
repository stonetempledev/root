select * from users
select ot.object_type_id, ot.object_type, ot.object_des, ot.object_list_des, ot.object_notes, ot.object_cmd
 from objects_types ot 
 where ot.user_id = 7 and ot.object_type = 'ricevuta'

select * from objects_attrs where user_id = 7 and object_type = 'ricevuta'
select * from objects_attrs where user_id = 7 and object_type = 'tipo_ricevuta'
select * from objects_attrs where user_id = 7 and object_type = 'evento'
select * from objects_attrs where user_id = 7 and object_type = 'tipo_evento'

select * from objects_types where user_id = 7

-- CON LE VISTE

select 
 -- 1
 t.*
  -- tipo_ricevuta
  , t_tr.object_code as t_tr_object_code, t_tr.object_title as t_tr_object_title
  , t_tr.dt_ins as t_tr_dt_ins, t_tr.dt_upd as t_tr_dt_upd
  , t_tr.v_entrata as t_tr_v_entrata, t_tr.v_notes as t_tr_v_notes
  -- eventi
  , t_e.object_code as t_e_object_code, t_e.object_title as t_e_object_title
  , t_e.dt_ins as t_e_dt_ins, t_e.dt_upd as t_e_dt_upd, t_e.v_al as t_e_v_al, t_e.v_dal as t_e_v_dal
  , t_e.v_notes as t_e_v_notes, t_e.id_tipo_evento as t_e_id_tipo_evento
   -- tipo_evento
   , t_te.object_code as t_e_t_te_object_code, t_te.object_title as t_e_t_te_object_title
   , t_te.dt_ins as t_e_t_te_dt_ins, t_te.dt_upd as t_e_t_te_dt_upd, t_te.v_notes as t_e_t_te_v_notes
from (
 -- 1
 select *, ROW_NUMBER() OVER (ORDER BY v_data) AS row_num
  from vwo_ricevuta where user_id = 7
  ) t
  left join vwo_tipo_ricevuta t_tr on t_tr.object_id = t.id_tipo_ricevuta
  left join vwo_eventi t_e on t_e.object_id = t.id_evento
  left join vwo_tipo_evento t_te on t_te.object_id = t_e.id_tipo_evento
  WHERE t.row_num >= 600 AND t.row_num < 600 + 400

alter view vwo_ricevuta
as
 select o.object_id, o.object_type, o.object_code, o.object_title, o.dt_ins, o.dt_upd, o.user_id
   , a_23.[value] as v_data, a_29.[value] as v_notes, a_32.[value] as v_prezzo, a_35.[value] as v_qta
   , a_26.[value] as id_evento, a_38.[value] as id_tipo_ricevuta
  from [objects] o
  left join objects_attrs_datetime a_23 on a_23.object_id = o.object_id and a_23.attribute_id = 23
  left join objects_attrs_varchar a_29 on a_29.object_id = o.object_id and a_29.attribute_id = 29
  left join objects_attrs_real a_32 on a_32.object_id = o.object_id and a_32.attribute_id = 32
  left join objects_attrs_integer a_35 on a_35.object_id = o.object_id and a_35.attribute_id = 35
  left join objects_attrs_object a_26 on a_26.object_id = o.object_id and a_26.attribute_id = 26
  left join objects_attrs_object a_38 on a_38.object_id = o.object_id and a_38.attribute_id = 38
  where o.object_type = 'ricevuta'
go

alter view vwo_tipo_ricevuta
as
 select o.object_id, o.object_type, o.object_code, o.object_title, o.dt_ins, o.dt_upd, o.user_id
  , a_8.[value] as v_entrata, a_5.[value] as v_notes
 from [objects] o
 left join objects_attrs_text a_5 on a_5.object_id = o.object_id and a_5.attribute_id = 5
 left join objects_attrs_flag a_8 on a_8.object_id = o.object_id and a_8.attribute_id = 8
 where o.object_type = 'tipo_ricevuta' 
go

alter view vwo_eventi
as
 select o.object_id, o.object_type, o.object_code, o.object_title, o.dt_ins, o.dt_upd, o.user_id
  , a_11.[value] as v_al, a_14.[value] as v_dal, a_17.[value] as v_notes
  , a_20.[value] as id_tipo_evento
 from [objects] o
 left join objects_attrs_datetime a_11 on a_11.object_id = o.object_id and a_11.attribute_id = 11
 left join objects_attrs_datetime a_14 on a_14.object_id = o.object_id and a_14.attribute_id = 14
 left join objects_attrs_text a_17 on a_17.object_id = o.object_id and a_17.attribute_id = 17
 left join objects_attrs_object a_20 on a_20.object_id = o.object_id and a_20.attribute_id = 20
 where o.object_type = 'evento'
go

alter view vwo_tipo_evento
as
 select o.object_id, o.object_type, o.object_code, o.object_title, o.dt_ins, o.dt_upd, o.user_id
  , a_2.[value] as v_notes
 from [objects] o
 left join objects_attrs_text a_2 on a_2.object_id = o.object_id and a_2.attribute_id = 2
 where o.object_type = 'tipo_evento'
go
