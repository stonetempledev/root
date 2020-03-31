
/* azzeramento totale

 truncate table [elements]
 truncate table [elements_contents]
 truncate table [elements_attrs_datetime]
 truncate table [elements_attrs_integer]
 truncate table [elements_attrs_link]
 truncate table [elements_attrs_text]
 truncate table [elements_attrs_real]
 truncate table [elements_attrs_varchar]

*/

/* via i deleted

 delete from elements_contents where element_id in (select element_id from [elements] where deleted > 0)
  or child_element_id in (select element_id from [elements] where deleted > 0)
 delete from elements_attrs_datetime where element_id in (select element_id from [elements] where deleted > 0)
 delete from elements_attrs_integer  where element_id in (select element_id from [elements] where deleted > 0)
 delete from elements_attrs_link where element_id in (select element_id from [elements] where deleted > 0)
 delete from elements_attrs_text where element_id in (select element_id from [elements] where deleted > 0)
 delete from elements_attrs_real where element_id in (select element_id from [elements] where deleted > 0)
 delete from elements_attrs_varchar where element_id in (select element_id from [elements] where deleted > 0)
 delete from [elements] where deleted > 0

*/

select * from [elements] where deleted > 0
select * from elements_contents where --element_id between 56 and 65 or
 child_element_id between 56 and 65
select * from [elements] order by element_id
select * from elements_contents where element_id between 13 and 27
 or child_element_id between 13 and 27 order by 2, 3

select * from vw_elements_attrs
select * from vw_elements_h where ref_element_id = -1 order by livello, [order]
select * from vw_elements_h where ref_element_id = 1 order by livello, [order]
select * from vw_elements_h where ref_element_id = 28 order by livello, [order]
select * from vw_elements_h where ref_element_id = 47 order by livello, [order]

select * from elements_contents where child_element_id = 13

select a.element_type, a.attribute_id
  , a.attribute_code, a.attribute_type
 from vw_attributes a 
 order by a.element_type, a.attribute_code

-- integrita: righe scollegate
select ec.* from [elements_contents] ec 
 where ec.element_id <> -1 and not exists (select top 1 1 from elements e where e.element_id = ec.element_id)
  or not exists (select top 1 1 from elements e where e.element_id = ec.child_element_id)
select e.* from elements e 
 where not exists (select top 1 1 from vw_elements_h where element_id = e.element_id)
select t.*
 from (select element_id, attribute_id from elements_attrs_datetime
  union select element_id, attribute_id from elements_attrs_integer
  union select element_id, attribute_id from elements_attrs_link
  union select element_id, attribute_id from elements_attrs_real
  union select element_id, attribute_id from elements_attrs_text
  union select element_id, attribute_id from elements_attrs_varchar) t
  where not exists (select top 1 1 from elements e where e.element_id = t.element_id)
   or not exists (select top 1 1 from attributes a where a.attribute_id = t.attribute_id)

select * from [elements] order by dt_del desc
select * from vw_elements_attrs order by element_id, attribute_code
select * from attributes order by 1

select * from utenti