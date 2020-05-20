select * from users
select ot.object_type_id, ot.object_type, ot.object_des, ot.object_list_des, ot.object_notes, ot.object_cmd
 from objects_types ot 
 where ot.user_id = 7 and ot.object_type = 'ricevuta'

select * from objects_attrs where object_type = 'evento' and user_id = 7

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
