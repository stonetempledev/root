-- copy dn_task_free_labels
select 'insert into dn_task_free_labels (free_txt, stato, priorita, tipo, stima, [default]) values ', 0 as [order]
union 
 select '(''' + free_txt + ''''
   + ', ' + (case when stato is null then 'null' else '''' + stato + '''' end) 
   + ', ' + (case when priorita is null then 'null' else '''' + priorita + '''' end) 
   + ', ' + (case when tipo is null then 'null' else '''' + tipo + '''' end) 
   + ', ' + (case when stima is null then 'null' else '''' + stima + '''' end) 
   + ', ' + (case when [default] is null then 'null' 
    when [default] = 1 then 'true' else 'false' end) + '),', 1 as [order]
  from dn_task_free_labels order by 2