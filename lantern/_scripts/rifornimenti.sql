/*
 MANUTENZIONE DATI RIFORNIMENTI MANCANTI O ECCEDENTI


 select * from auto
 select ra.* from rifornimentoauto ra
  join auto a on a.idauto = ra.idauto and a.marca = 'alfa romeo'
  order by data desc
 select ma.* from manutenzioneauto ma
  join auto a on a.idauto = ma.idauto and a.marca = 'alfa romeo'
  order by data desc

CREATE VIEW ViewRifornimenti
AS  
 select r.IDRifornimento, r.idauto, r.idevento, r.km, r.data, r.prezzo, r.costolitro, r.note, r.dtins, r.dtupd
  , datediff(day, r2.data, r.data) as giorni, r2.data as dataprev, r2.km as kmprev
  , r.km - r2.km as kmpercorsi
  , (cast((r.km - r2.km) as float) / (case datediff(day, r2.data, r.data) when 0 then 1 else datediff(day, r2.data, r.data) end )) as kmgiorno
  , (r.km - r2.km) / (r2.prezzo / r2.costolitro) as kmlitro
 from [rifornimentoauto] r
 left join [rifornimentoauto] r2 on r.idauto = r2.idauto and r2.km = (select max(km) from rifornimentoauto where km < r.km and idauto = r.idauto)

USE [my]
GO

CREATE UNIQUE NONCLUSTERED INDEX [idxRifornimentoAuto] ON [dbo].[RifornimentoAuto]
( [IDAuto] ASC, [Km] ASC )
GO

DROP INDEX [idxRifornimentoAuto] ON [dbo].[RifornimentoAuto]
GO

*/

/* PARAMETRI

delete from __keys where name in ('azione', 'from_date', 'to_date', 'min_kmlitro', 'max_kmlitro', 'max_giorni', 'km_litro', 'euro');
insert into __keys (name, val) values ('azione', ''); -- togli_km, togli_rif, aggiungi
insert into __keys (name, valdate) values ('from_date', Cast('2013-01-01' as datetime));
insert into __keys (name, valdate) values ('to_date', Cast('2016-08-08' as datetime));
insert into __keys (name, valdbl) values ('max_kmlitro', 21);
insert into __keys (name, valdbl) values ('min_kmlitro', 15);
insert into __keys (name, valint) values ('max_giorni', 14);
insert into __keys (name, valdbl) values ('km_litro', 20.5);
insert into __keys (name, valdbl) values ('euro', 30);

*/

-- 474800
-- 464500	2016-01-15 00:00:00.000
-- 457000	2015-08-20 00:00:00.000
-- 448300	2015-04-04 00:00:00.000

-- togliere km al range di date che superano una certa media chilometrica
if dbo.GetKeyVal('azione') = 'togli_km' 
begin

	if exists(select * from ViewRifornimenti r where r.idauto = (select idauto from auto where marca = 'alfa romeo')
	  and r.data >= dbo.getKeyDate('from_date') and r.data <= dbo.getKeyDate('to_date')
	  and kmlitro >= dbo.getKeyDbl('max_kmlitro'))
	begin

	 print 'tolgo dei km'

	 update [rifornimentoauto] set km = km - 10
	  where data >= (select top 1 r.data from viewrifornimenti r
	   where r.idauto = (select idauto from auto where marca = 'alfa romeo') 
		and r.data >= dbo.getKeyDate('from_date') and r.data <= dbo.getKeyDate('to_date')
		and r.kmlitro >= dbo.getKeyDbl('max_kmlitro') order by km) 
	   and data <= dbo.getKeyDate('to_date');

	end
	else print 'tolto nulla'

	select r.IDRifornimento, r.km, r.data, r.prezzo, r.costolitro, r.note
	  , r.giorni, r.kmprev, r.kmpercorsi, r.kmgiorno, r.kmlitro
	 from viewrifornimenti r
	 where r.idauto = (select idauto from auto where marca = 'alfa romeo') 
	   and r.data >= (select top 1 data from ViewRifornimenti r where r.idauto = (select idauto from auto where marca = 'alfa romeo')
	   and r.data >= dbo.getKeyDate('from_date') and r.data <= dbo.getKeyDate('to_date')
	   and kmlitro >= dbo.getKeyDbl('max_kmlitro') order by data)
	  and r.data <= dbo.getKeyDate('to_date')
	 order by r.data 
end

-- aggiungere rifornimenti superata un periodo di non rifornimento

-- insert into __keys (name, valint) values ('max_giorni', 12);
-- insert into __keys (name, valdbl) values ('km_litro', 20.5);
-- insert into __keys (name, valdbl) values ('euro', 30);
-- 1372	435932	2014-07-26 00:00:00.000	40,00	1,55	NULL	12	2014-07-14 00:00:00.000	435515	417	34,75	21,684

if dbo.GetKeyVal('azione') = 'aggiungi' 
begin

	if exists(select r.* from viewrifornimenti r
	 where r.idauto = (select idauto from auto where marca = 'alfa romeo') and r.data >= dbo.getKeyDate('from_date') 
	  and r.data <= dbo.getKeyDate('to_date') and r.giorni >= dbo.getKeyInt('max_giorni'))
	begin

	 print 'aggiungo un rifornimento'

	 declare @id as integer; declare @date datetime; declare @giorni int; declare @costolitro float; declare @prev_km integer;
	 select top 1 @id = r.idrifornimento, @date = r.data, @giorni = r.giorni, @costolitro = r.costolitro, @prev_km = r.kmprev
	  from viewrifornimenti r 
	  where r.idauto = (select idauto from auto where marca = 'alfa romeo') and r.data >= dbo.getKeyDate('from_date') 
	   and r.data <= dbo.getKeyDate('to_date') and r.giorni >= dbo.getKeyInt('max_giorni') order by km

	 -- aggiungo il rifornimento
	 -- select * from rifornimentoauto
	 declare @km as float; set @km = ((dbo.getKeyDbl('euro') / @costolitro) * dbo.getKeyDbl('km_litro'));
	 insert into rifornimentoauto (idauto, km, data, prezzo, costolitro, dtins)
	  select idauto, @prev_km + @km
	   , DATEADD ( day , -(@giorni / 2), @date), dbo.getKeyDbl('euro'), @costolitro, getdate()
	   from auto where marca = 'alfa romeo'

	 -- aggiorno i successivi
	 update rifornimentoauto set km = km + @km
	  where idauto = (select idauto from auto where marca = 'alfa romeo')
	   and data > DATEADD ( day , -(@giorni / 2), @date);

	 /* repair
	 update rifornimentoauto set km = km - 396
	  where idauto = (select idauto from auto where marca = 'alfa romeo')
	   and data >= Cast('2015-19-04' as datetime);
	 */

	end
	else print 'aggiunto nulla'

end

if dbo.GetKeyVal('azione') = 'togli_rif' 
begin
  delete from rifornimentoauto 
   where idrifornimento in (select r.IDRifornimento from viewrifornimenti r
    where r.idauto = (select idauto from auto where marca = 'alfa romeo') and r.data >= dbo.getKeyDate('from_date') 
     and r.data <= dbo.getKeyDate('to_date') and kmlitro <= dbo.getKeyDbl('min_kmlitro'))
end

-- riepilogo
if dbo.GetKeyVal('azione') = '' or dbo.GetKeyVal('azione') = 'togli_rif'
begin 
	select r.IDRifornimento, r.km, r.data, r.prezzo, r.costolitro, r.note
	  , r.giorni, r.kmprev, r.kmpercorsi, r.kmgiorno, r.kmlitro
	 from viewrifornimenti r
	 where r.idauto = (select idauto from auto where marca = 'alfa romeo') and r.data >= dbo.getKeyDate('from_date') 
	  and r.data <= dbo.getKeyDate('to_date')
	 order by r.data desc
end

