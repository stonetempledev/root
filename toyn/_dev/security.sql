---------- SECURITY

use toyn
go

-- mk 

--  DROP MASTER KEY
--  SELECT * FROM SYS.SYMMETRIC_KEYS 
if not exists (SELECT top 1 1 FROM SYS.SYMMETRIC_KEYS WHERE NAME LIKE '%DatabaseMasterKey%')
 CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'Homme33!'
go

-- BACKUP
use toyn
go

OPEN MASTER KEY DECRYPTION BY PASSWORD = 'Homme33!';  
BACKUP MASTER KEY TO FILE = 'c:\tmp\master.key'   
  ENCRYPTION BY PASSWORD = 'Homme33!'  
go

-- RESTORE
USE toyn;
go
  
RESTORE MASTER KEY   
    FROM FILE = 'c:\tmp\master.key'   
    DECRYPTION BY PASSWORD = 'Homme33!'   
    ENCRYPTION BY PASSWORD = 'Homme33!';  
GO  

-- cer 
--  DROP CERTIFICATE toyn191005 
--  SELECT * FROM SYS.CERTIFICATES 

OPEN MASTER KEY DECRYPTION BY PASSWORD = 'Homme33!';  
if not exists (SELECT top 1 1 FROM SYS.CERTIFICATES WHERE NAME = 'toyn191005')
 CREATE CERTIFICATE toyn191005 WITH SUBJECT = 'toyn'
go

-- sk 
--  DROP SYMMETRIC KEY toyn01 
--  SELECT * FROM SYS.SYMMETRIC_KEYS

if not exists (SELECT top 1 1 FROM SYS.SYMMETRIC_KEYS WHERE NAME = 'toyn01')
  CREATE SYMMETRIC KEY toyn01 WITH ALGORITHM = AES_256 ENCRYPTION BY CERTIFICATE toyn191005
go

/* TEST

OPEN SYMMETRIC KEY toyn01 DECRYPTION BY CERTIFICATE toyn191005;
truncate table utenti
insert into utenti (enc_nome, enc_email, pwd, dt_ins, tmp_key, activate_key, activated)
 values (EncryptByKey(Key_GUID('toyn01'), 'molinero'), EncryptByKey(Key_GUID('toyn01'), 'fabio.molinaroli@mail.com'), 'QQCybj9TBJP8iCgCZg1GyXORoBo=', getdate(), 'VW9DV9X43GWJ89S4Y416XZALXVVGX2WF', '5202UICRVRPMTBELTJTP43F6VXPAORHA', 2);

select * from utenti

OPEN SYMMETRIC KEY toyn01 DECRYPTION BY CERTIFICATE toyn191005;
select id_utente, CONVERT(varchar(100), DecryptByKey(enc_nome)) as nome, pwd
  , CONVERT(varchar(100), DecryptByKey(enc_email)) as email, isnull(activated, 0) as activated 
 from utenti 

*/