-- restore database
ALTER DATABASE [toyn_bck] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

RESTORE DATABASE [toyn_bck] FROM DISK = N'c:\tmp\toyn_bck.bak' WITH NOUNLOAD,
 MOVE 'toyn' TO
  'c:\Program Files\Microsoft SQL Server\MSSQL10.SQLEXPRESS\MSSQL\DATA\toyn_bck.mdf',
      MOVE 'toyn_Log'
  TO 'c:\Program Files\Microsoft SQL Server\MSSQL10.SQLEXPRESS\MSSQL\DATA\toyn_bck.ldf'
 , REPLACE, STATS = 10;

ALTER DATABASE [toyn_bck] SET MULTI_USER;