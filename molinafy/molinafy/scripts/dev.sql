select * from client_ips
select * from client_logged
select * from users

-- test doc
--  select * from docs
truncate table docs;
insert into docs (title_doc, des_doc)
 values ('Molinafy Line Commands', 'elenco dei comandi con relativa descrizione che si possono usare nella molinafy, per poter fare tutto quello che vuoi');
