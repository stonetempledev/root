@echo off

rem PARAMETRI DI FUNZIONAMENTO


set tmpfile=%TEMP%\__tmppassing.txt
set year=%date:~6,4%
set month=%date:~3,2%
set day=%date:~0,2%
set hours=%time:~0,2%
if "%hours:~0,1%" == " " set hours=0%hours:~1,1%
set minutes=%time:~3,2%
set seconds=%time:~6,2%
set log=_log\%year%%month%%day%_%hours%%minutes%%seconds%
rem percorsi 
set prj_path=C:\ss\edfs\Credit.Management\srcDevel\Credit.Management\pegService
set release=C:\ss\edfs\Credit.Management\srcDevel\Credit.Management\pegService\bin\Release
set release_sched=C:\ss\edfs\Credit.Management\srcDevel\Credit.Management\Scheduler\bin\Release
set release_jobwrp=C:\ss\edfs\Credit.Management\srcDevel\Credit.Management\JobWrapper\bin\Debug
set loader=C:\ss\edfs\Credit.Management\srcDevel\Credit.Management\Loader\Loader
rem percorsi
call :getLocalIP bulkIP
rem set bulkIP=192.168.200.1
set maindrive=C:
set mainpath=\attivita\exprivia\_tools\pegService
set path_output=%maindrive%%mainpath%
rem set path_output=\\%bulkIP%\Backup Utenti\fm\bulk
set path_output_bulk=\\%bulkIP%
rem set path_output_bulk=\\192.168.200.1\Backup Utenti\fm\bulk
rem connessioni: SBA_LOCAL, SBA_EXPR, BPS_EXPR, BCC_EXPR, SBA_PROD, SBA_TEST
set dbname=BPS_EXPR
rem types bulk: BulkInsert, SQLLoader, ExternalTable 
set typebulk=SQLLoader
rem files xml: struttureCSVSBA.xml, struttureCSVPOPSO.xml
set filexml=struttureCSVPOPSO.xml
rem utenza sql loader per oracle e utenza db sempre per nulk di oracle
set usersqlldr=fmolinaroli
set pwdsqlldr=Homme34!
set userdboracle=POPSO_PEG
set pwddboracle=POPSO_PEG
rem cartelle
set folder_ctl=ctl
set folder_bin=bin
set folder_input=input
set folder_output=output
set folder_xmls=xmls
set folder_backup=backup
set folder_rbulk=router_bulk

rem INIZIALIZZAZIONE AMBIENTE
echo inizializzazione ambiente
%maindrive%
cd %mainpath%
if not exist _log mkdir _log
call :getLocalIP localIP

rem COPIA DELLE RISORSE ED INIZIALIZZAZIONE STRUTTURA
echo copia delle risorse
if not exist %folder_bin% mkdir %folder_bin%
copy /y "%release%\*.dll" "%folder_bin%" >%log%_copy.log
copy /y "%release%\*.xml" "%folder_bin%" >>%log%_copy.log
copy /y "%release%\*.exe" "%folder_bin%" >>%log%_copy.log
copy /y "%release%\*.pdb" "%folder_bin%" >>%log%_copy.log
copy /y "%release%\*.config" "%folder_bin%" >>%log%_copy.log
copy /y "%release_sched%\*.xml" "%folder_bin%" >>%log%_copy.log
copy /y "%release_sched%\*.exe" "%folder_bin%" >>%log%_copy.log
copy /y "%release_sched%\*.pdb" "%folder_bin%" >>%log%_copy.log
copy /y "%release_sched%\*.config" "%folder_bin%" >>%log%_copy.log
copy /y "%release_jobwrp%\*.exe" "%folder_bin%" >>%log%_copy.log
copy /y "%release_jobwrp%\*.config" "%folder_bin%" >>%log%_copy.log
if not exist %folder_bin%\html mkdir %folder_bin%\html
copy /y "%release%\html\*.*" "%folder_bin%\html\" >>%log%_copy.log
if not exist %folder_ctl% mkdir %folder_ctl%
copy /y "%loader%\ctl\*.ctl" "%folder_ctl%" >>%log%_copy.log
if not exist %folder_xmls% mkdir %folder_xmls%
copy /y "%loader%\xml\struttureCSVSBA.xml" "%folder_xmls%" >>%log%_copy.log
copy /y "%loader%\xml\struttureCSVPOPSO.xml" "%folder_xmls%" >>%log%_copy.log
if not exist %folder_input% mkdir %folder_input%
if not exist %folder_output% mkdir %folder_output%
if not exist %folder_backup% mkdir %folder_backup%


rem AGGIORNA XML DI CONFIGURAZIONE
echo aggiorna xml di configurazione

call :setNodeXml "%folder_xmls%\struttureCSVSBA.xml" "/pefstat/folders/folder[@key='##PATH_BKP##']/@path" "%maindrive%%mainpath%"
call :setNodeXml "%folder_xmls%\struttureCSVSBA.xml" "/pefstat/folders/folder[@key='##PATH_OUTPUT##']/@path" "%path_output%\%folder_output%\?BANCA?"
call :setNodeXml "%folder_xmls%\struttureCSVSBA.xml" "/pefstat/folders/folder[@key='##PATH_BULK_INSERT##']/@path" "%path_output_bulk%\%folder_output%\?BANCA?"

call :setNodeXml "%folder_xmls%\struttureCSVPOPSO.xml" "/pefstat/folders/folder[@key='##PATH_BKP##']/@path" "%maindrive%%mainpath%"
call :setNodeXml "%folder_xmls%\struttureCSVPOPSO.xml" "/pefstat/folders/folder[@key='##PATH_OUTPUT##']/@path" "%maindrive%%mainpath%\%folder_output%\"
call :setNodeXml "%folder_xmls%\struttureCSVPOPSO.xml" "/pefstat/folders/folder[@key='##PATH_BULK_INSERT##']/@path" "%maindrive%%mainpath%\%folder_output%\"
call :setNodeXml "%folder_xmls%\struttureCSVPOPSO.xml" "/pefstat/folders/folder[@key='##PATH_CTL_SQLLOADER##']/@path" "%maindrive%%mainpath%\%folder_ctl%\"
call :setNodeXml "%folder_xmls%\struttureCSVPOPSO.xml" "/pefstat/folders/folder[@key='##PATH_OUTPUT_EXTTBL##']/@path" ""


rem AGGIORNA CONFIG FILES
echo aggiornamento config file scheduler e peg service
call :setNodeXml "%folder_bin%\pegService.xml" "/peg-service/vars/parsed[@name='path_logs']" "{@appfolder}\..\_log"

call :setAttrXml "%folder_bin%\jobWrapper.exe.config" "/configuration/log4net/appender[@name='LogFileAppender']/file" "value" "C:\attivita\exprivia\_tools\pegService\_log\logWrapper.txt"

call :setAttrXml "%folder_bin%\scheduler.exe.config" "/configuration/log4net/appender[@name='LogFileAppender']/file" "type" "log4net.Util.PatternString"
call :setAttrXml "%folder_bin%\scheduler.exe.config" "/configuration/log4net/appender[@name='LogFileAppender']/file" "value" "../_log/scheduler_@perc;date{yyyyMMdd}_@perc;date{HHmmss}_[@perc;processid].log"
call :setAttrXml "%folder_bin%\scheduler.exe.config" "/configuration/log4net/root/level" "value" "DEBUG"

call :setAttrXml "%folder_bin%\pegService.exe.config" "/configuration/log4net/appender[@name='LogFileAppender']/file" "value" "../_log/service_"
call :setAttrXml "%folder_bin%\pegService.exe.config" "/configuration/log4net/appender[@name='LogConsoleAppender']/file" "value" "../_log/console_"
call :setAttrXml "%folder_bin%\pegService.exe.config" "/configuration/log4net/root/level" "value" "DEBUG"
call :setAttrXml "%folder_bin%\pegService.exe.config" "/configuration/log4net/logger[@name='LogConsoleAppender']/level" "value" "DEBUG"

call :removeChildsNode "%folder_bin%\scheduler.exe.config" "/configuration/appSettings"
call :setAttrXml "%folder_bin%\scheduler.exe.config" "/configuration/appSettings" "file" "settings.config"

call :setAttrXml "%folder_bin%\settings.config" "/appSettings/add[@key='serviceConfigPath']" "value" "C:\attivita\exprivia\_tools\pegService\bin\pegService.xml"
call :setAttrXml "%folder_bin%\settings.config" "/appSettings/add[@key='SendMail']" "value" "true"
call :setAttrXml "%folder_bin%\settings.config" "/appSettings/add[@key='MailTo']" "value" "fabio.molinaroli@exprivia.it"
call :setAttrXml "%folder_bin%\settings.config" "/appSettings/add[@key='BackupFiles']" "value" "true"
call :setAttrXml "%folder_bin%\settings.config" "/appSettings/add[@key='AllFilesOptional']" "value" "true"
call :setAttrXml "%folder_bin%\settings.config" "/appSettings/add[@key='SQLLoaderUser']" "value" "%usersqlldr%"
call :setAttrXml "%folder_bin%\settings.config" "/appSettings/add[@key='SQLLoaderPwd']" "value" "%pwdsqlldr%"
call :setAttrXml "%folder_bin%\settings.config" "/appSettings/add[@key='usr_bulk']" "value" "%userdboracle%"
call :setAttrXml "%folder_bin%\settings.config" "/appSettings/add[@key='pwd_bulk']" "value" "%pwddboracle%"
call :setAttrXml "%folder_bin%\settings.config" "/appSettings/add[@key='BulkInsertType']" "value" "%typebulk%"
call :setAttrXml "%folder_bin%\settings.config" "/appSettings/add[@key='pathStrutturaXml']" "value" "%maindrive%%mainpath%\%folder_xmls%\%filexml%"
call :setAttrXml "%folder_bin%\settings.config" "/appSettings/add[@key='pathInputForced']" "value" "%maindrive%%mainpath%\%folder_input%"
call :setAttrXml "%folder_bin%\settings.config" "/appSettings/add[@key='pathFileTesta']" "value" "%maindrive%%mainpath%\%folder_output%\"
call :setAttrXml "%folder_bin%\settings.config" "/appSettings/add[@key='pathFileCoda']" "value" "%maindrive%%mainpath%\%folder_output%\"
call :setAttrXml "%folder_bin%\settings.config" "/appSettings/add[@key='pathFileCorpo']" "value" "%maindrive%%mainpath%\%folder_output%\"
call :setAttrXml "%folder_bin%\settings.config" "/appSettings/add[@key='DirBackup']" "value" "%maindrive%%mainpath%\%folder_backup%"
call :setAttrXml "%folder_bin%\settings.config" "/appSettings/add[@key='routerNuoveRegole']" "value" "true"
call :setAttrXml "%folder_bin%\settings.config" "/appSettings/add[@key='tipoAutenticazione']" "value" "SBA"
call :setAttrXml "%folder_bin%\settings.config" "/appSettings/add[@key='multiTasking']" "value" "true"
call :setAttrXml "%folder_bin%\settings.config" "/appSettings/add[@key='localPathFilesRouter']" "value" "%path_output%\%folder_rbulk%"
call :setAttrXml "%folder_bin%\settings.config" "/appSettings/add[@key='bulkPathFilesRouter']" "value" "%path_output_bulk%\%folder_rbulk%"
call :setAttrXmlObb "%folder_bin%\settings.config" "/appSettings/add[@key='localPathFilesRouter']" "key" "localPathFilesRouter.bck"
call :setAttrXmlObb "%folder_bin%\settings.config" "/appSettings/add[@key='bulkPathFilesRouter']" "key" "bulkPathFilesRouter.bck"

rem CONNESSIONE AL DATABASE
echo impostazione stringa di connessione: %dbname%
if %dbname%==SBA_LOCAL (
 call :setAttrXml "%folder_bin%\Scheduler.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "connectionString" "Server=(local)\SQLEXPRESS;Database=EXPRV;Trusted_Connection=True;"
 call :setAttrXml "%folder_bin%\Scheduler.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "providerName" "System.Data.SqlClient"
 call :setAttrXml "%folder_bin%\pegService.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "connectionString" "Server=(local)\SQLEXPRESS;Database=EXPRV;Trusted_Connection=True;"
 call :setAttrXml "%folder_bin%\pegService.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "providerName" "System.Data.SqlClient"
 call :setAttrXml "%folder_bin%\jobWrapper.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "connectionString" "Server=(local)\SQLEXPRESS;Database=EXPRV;Trusted_Connection=True;"
 call :setAttrXml "%folder_bin%\jobWrapper.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "providerName" "System.Data.SqlClient"
) else if %dbname%==SBA_EXPR (
 call :setAttrXml "%folder_bin%\Scheduler.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "connectionString" "Data Source=192.168.23.47\sql2005CRED;Persist Security Info=True;Initial Catalog=SBAPEG_TEST;User Id=sa;Password=Apollo13;"
 call :setAttrXml "%folder_bin%\Scheduler.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "providerName" "System.Data.SqlClient"
 call :setAttrXml "%folder_bin%\pegService.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "connectionString" "Data Source=192.168.23.47\sql2005CRED;Persist Security Info=True;Initial Catalog=SBAPEG_TEST;User Id=sa;Password=Apollo13;"
 call :setAttrXml "%folder_bin%\pegService.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "providerName" "System.Data.SqlClient"
 call :setAttrXml "%folder_bin%\jobWrapper.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "connectionString" "Data Source=192.168.23.47\sql2005CRED;Persist Security Info=True;Initial Catalog=SBAPEG_TEST;User Id=sa;Password=Apollo13;"
 call :setAttrXml "%folder_bin%\jobWrapper.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "providerName" "System.Data.SqlClient"
) else if %dbname%==BPS_EXPR (
 call :setAttrXml "%folder_bin%\Scheduler.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "connectionString" "Data Source=ORA10SVIL;User Id=POPSO_PEG;Password=POPSO_PEG;"
 call :setAttrXml "%folder_bin%\Scheduler.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "providerName" "System.Data.OracleClient"
 call :setAttrXml "%folder_bin%\pegService.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "connectionString" "Data Source=ORA10SVIL;User Id=POPSO_PEG;Password=POPSO_PEG;"
 call :setAttrXml "%folder_bin%\pegService.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "providerName" "System.Data.OracleClient"
 call :setAttrXml "%folder_bin%\jobWrapper.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "connectionString" "Data Source=ORA10SVIL;User Id=POPSO_PEG;Password=POPSO_PEG;"
 call :setAttrXml "%folder_bin%\jobWrapper.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "providerName" "System.Data.OracleClient"
) else if %dbname%==BCC_EXPR (
 call :setAttrXml "%folder_bin%\Scheduler.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "connectionString" "Data Source=ORA10SVIL;User Id=BCCPEG;Password=BCCPEG;"
 call :setAttrXml "%folder_bin%\Scheduler.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "providerName" "System.Data.OracleClient"
 call :setAttrXml "%folder_bin%\pegService.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "connectionString" "Data Source=ORA10SVIL;User Id=BCCPEG;Password=BCCPEG;"
 call :setAttrXml "%folder_bin%\pegService.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "providerName" "System.Data.OracleClient"
) else if %dbname%==SBA_TEST (
 call :setAttrXml "%folder_bin%\Scheduler.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "connectionString" "Server=10.4.7.165;Database=pegtest;User ID=pegtest;Password=tOur..16mUZzyyy"
 call :setAttrXml "%folder_bin%\Scheduler.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "providerName" "System.Data.SqlClient"
 call :setAttrXml "%folder_bin%\pegService.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "connectionString" "Server=10.4.7.165;Database=pegtest;User ID=pegtest;Password=tOur..16mUZzyyy"
 call :setAttrXml "%folder_bin%\pegService.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "providerName" "System.Data.SqlClient"
) else if %dbname%==SBA_PROD (
 call :setAttrXml "%folder_bin%\Scheduler.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "connectionString" "Server=10.4.7.165;Database=peg;User ID=dbuser.peg;Password=Zaut.45.mUZxbbb"
 call :setAttrXml "%folder_bin%\Scheduler.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "providerName" "System.Data.SqlClient"
 call :setAttrXml "%folder_bin%\pegService.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "connectionString" "Server=10.4.7.165;Database=peg;User ID=dbuser.peg;Password=Zaut.45.mUZxbbb"
 call :setAttrXml "%folder_bin%\pegService.exe.config" "/configuration/connectionStrings/add[@name='SVI3']" "providerName" "System.Data.SqlClient"
) else (
 echo database %dbname% non supportato
 goto errore
)

rem LINEA DI COMANDO
rem echo esecuzione scheduler: %lc%
rem %lc% >%log%_console.log
rem %lc% 
rem if %ERRORLEVEL%==-1 goto errore 

echo %time%: esecuzione patch terminata con successo 
echo %time%: esecuzione patch terminata con successo >>%log%_ok.log
goto end


rem SUBROUNTINES


rem function getLocalIP(name var)
:getLocalIP
call c:\windows\system32\wscript "_fncs.wsf" "getLocalIP" %tmpfile%
set /p %1=<%tmpfile%
if %ERRORLEVEL%==-1 goto errore 
exit /b

rem function setAttrXml(pathxml, pathnode, attr, value)
:setAttrXml
call c:\windows\system32\wscript "_fncs.wsf" "setAttrXml" %1 %2 %3 %4
if %ERRORLEVEL%==-1 goto errore 
exit /b

rem function setAttrXmlObb(pathxml, pathnode, attr, value)
:setAttrXmlObb
call c:\windows\system32\wscript "_fncs.wsf" "setAttrXmlObb" %1 %2 %3 %4
if %ERRORLEVEL%==-1 goto errore 
exit /b

rem function setNodeXml(pathxml, pathnode, value)
:setNodeXml
call c:\windows\system32\wscript "_fncs.wsf" "setNodeXml" %1 %2 %3
if %ERRORLEVEL%==-1 goto errore 
exit /b

rem function removeChildsNode(pathxml, pathnode)
:removeChildsNode
call c:\windows\system32\wscript "_fncs.wsf" "removeChildsNode" %1 %2
if %ERRORLEVEL%==-1 goto errore 
exit /b



rem END


:errore
echo %time%: errore durante le elaborazioni 
echo %time%: errore durante le elaborazioni >>%log%_ko.log

:end
pause
