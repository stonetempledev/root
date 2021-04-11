/* tabelle
*/

-- thelantern.dbo.tipieventi
drop table tipi_eventi;
create table tipi_eventi (tipo_evento_id int identity(1,1) primary key, tipo_evento varchar(100) not null, note varchar(500)
 , user_id int not null, dt_ins datetime not null default getdate(), dt_upd datetime);

set identity_insert tipi_eventi on

insert into tipi_eventi (tipo_evento_id, tipo_evento, note, user_id, dt_ins, dt_upd)
 select idtipoevento, tipoevento, note
   , (select user_id from users where nome = 'famigliola') as user_id, dtins, dtupd
  from thelantern.dbo.tipieventi

set identity_insert tipi_eventi off

-- thelantern.dbo.eventi
drop table eventi;
create table eventi (evento_id int identity(1,1) primary key, tipo_evento_id int, evento varchar(100) not null, dt_da date not null, dt_a date not null, note varchar(500)
 , user_id int not null, dt_ins datetime not null default getdate(), dt_upd datetime);

set identity_insert eventi on

insert into eventi (evento_id, tipo_evento_id, evento, dt_da, dt_a, note, user_id, dt_ins, dt_upd)
 select idevento, idtipoevento, evento, datada, dataa, note
   , (select user_id from users where nome = 'famigliola') as user_id, dtins, dtupd
  from thelantern.dbo.Eventi

set identity_insert eventi off

-- thelantern.dbo.tipispesa
drop table tipi_spesa;
create table tipi_spesa (tipo_spesa_id int identity(1,1) primary key, tipo_spesa varchar(100) not null, note varchar(500), entrata bit
 , user_id int not null, dt_ins datetime not null default getdate(), dt_upd datetime);

set identity_insert tipi_spesa on

insert into tipi_spesa (tipo_spesa_id, tipo_Spesa, note, entrata, user_id, dt_ins, dt_upd)
 select idtipospesa, tipospesa, note, entrata
   , (select user_id from users where nome = 'famigliola') as user_id, dtins, dtupd
  from thelantern.dbo.TipiSpesa

set identity_insert tipi_spesa off

-- thelantern.dbo.spese
drop table spese;
create table spese (spesa_id int identity(1,1) primary key, evento_id int, tipo_spesa_id int not null, cosa varchar(100), qta smallint, dt_spesa date not null
 , prezzo money not null, totale money not null, user_id int not null, dt_ins datetime not null default getdate(), dt_upd datetime);

set identity_insert spese on

insert into spese (spesa_id, evento_id, tipo_spesa_id, cosa, qta, dt_spesa, prezzo, totale, user_id, dt_ins, dt_upd)
 select idspesa, idevento, idtipospesa, pezzo, quantita, data, prezzo, prezzo * cast(isnull(quantita, 1) as decimal) as totale
   , (select user_id from users where nome = 'famigliola') as user_id, dtins, dtupd
  from thelantern.dbo.spese

set identity_insert spese off
