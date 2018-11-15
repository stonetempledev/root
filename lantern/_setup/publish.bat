@echo off
echo linea di comando:
echo  drive locale del progetto: %1
echo  percorso locale del progetto: %2
echo  percorso locale di pubblicazione: %3
echo  titolo del sito: %4
echo  nome della connessione di default: %5
echo  percorso assoluto del sito sul web server: %6
echo  titolo menu: %7
echo  titolo home page: %8
set /p confirmDeploy=Vuoi continuare? [y/n]:
if %confirmDeploy%==n goto :end
 

@echo on

rem cartella di lavoro
%1
cd %2

rem variabili
set tmpfile=%TEMP%\__tmppassing.txt
set local_path=%1%2
set site_path=%3
set js_folder=libjs

rem pubblicazione
set msBuildDir=%WINDIR%\Microsoft.NET\Framework\v4.0.30319
call %msBuildDir%\msbuild.exe  ..\lantern.sln "/p:Configuration=Release;DeployOnBuild=true" /l:FileLogger,Microsoft.Build.Engine;logfile=publish.log
IF EXIST %site_path% (RD /S /Q "%site_path%")
mkdir %site_path%
xcopy ..\PrecompiledWeb\lantern\*.* %site_path% /e /exclude:exclude_list
RD /S /Q "..\PrecompiledWeb"

rem minify - build libjs
IF EXIST %site_path%\%js_folder% (RD /S /Q "%site_path%\%js_folder%")
mkdir %site_path%\%js_folder%\modules

call :cNodesXml c_nodes "%local_path%\..\cfg.xml" "/root/grp_modules/grp_module"
set /a c_nodes = c_nodes - 1
:begin_nodes
 call :getAttrXml name_grp "%local_path%\..\cfg.xml" "/root/grp_modules/grp_module[%c_nodes%]" "name"

 call :getAttrXmlNoErr ref_css "%local_path%\..\cfg.xml" "/root/grp_modules/grp_module[@name='%name_grp%']/module[@type='css']" "href" "{null}"

 if "%ref_css%" == "{null}" goto :no_css 
 
 call :genMinifyXml "%local_path%\..\" "%local_path%\..\cfg.xml" "%local_path%\minify_css.xml" "/root/grp_modules/grp_module[@name='%name_grp%']/module[@type='css']" "%site_path%\%js_folder%\modules\%name_grp%.css"
 call "C:\Program Files (x86)\Microsoft\Microsoft Ajax Minifier\ajaxmin.exe" -css -comments:none -xml "%local_path%\minify_css.xml" -clobber
 call :removeNodes "%site_path%\cfg.xml" "/root/grp_modules/grp_module[@name='%name_grp%']/module[@type='css']"
 call :addNode "%site_path%\cfg.xml" "/root/grp_modules/grp_module[@name='%name_grp%']" "module" "type:css;href:{@siteurl}%js_folder%/modules/%name_grp%.css;"

:no_css
 call :genMinifyXml "%local_path%\..\" "%local_path%\..\cfg.xml" "%local_path%\minify_js.xml" "/root/grp_modules/grp_module[@name='%name_grp%']/module[@type='script']" "%site_path%\%js_folder%\modules\%name_grp%.js"
 call "C:\Program Files (x86)\Microsoft\Microsoft Ajax Minifier\ajaxmin.exe" -js -comments:none -xml "minify_js.xml" -clobber
 call :removeNodes "%site_path%\cfg.xml" "/root/grp_modules/grp_module[@name='%name_grp%']/module[@type='script']"
 call :addNode "%site_path%\cfg.xml" "/root/grp_modules/grp_module[@name='%name_grp%']" "module" "type:script;href:{@siteurl}%js_folder%/modules/%name_grp%.js;"

 set /a c_nodes = c_nodes - 1
 if %c_nodes% == -1 goto :end_nodes 
goto :begin_nodes
:end_nodes

call :removeNodes "%site_path%\cfg.xml" "/root/grp_modules/grp_module[@name='base']/comment()"

mkdir %site_path%\%js_folder%\fonts
xcopy %local_path%\..\%js_folder%\metro-3.0\fonts\*.* %site_path%\%js_folder%\fonts /e 

rem configurazione
call :setNodeXml "%site_path%\cfg.xml" "/root/vars/var[@name='sitetitle']" %4
call :setAttrXml "%site_path%\cfg.xml" "/root/dbconns" "default" %5
call :setAttrXml "%site_path%\web.config" "/configuration/appSettings/add[@key='approot']" "value" %6
call :setAttrXml "%site_path%\web.config" "/configuration/appSettings/add[@key='reloadConfig']" "value" "false"
call :setAttrXml "%site_path%\web.config" "/configuration/log4net/root/level" "value" "ERROR"
call :setAttrXml "%site_path%\cfg.xml" "/root/pages/page[@name='home']" "title" %7
call :setNodeXml "%site_path%\cfg.xml" "/root/pages/page[@name='home']/title" %8

echo %time%: batch terminato con successo 
goto end


rem SUBROUNTINES


rem function getLocalIP(name var)
:getLocalIP
call c:\windows\system32\wscript "js.wsf" "getLocalIP" %tmpfile%
set /p %1=<%tmpfile%
if %ERRORLEVEL%==-1 goto errore 
exit /b

rem function setAttrXml(pathxml, pathnode, attr, value)
:setAttrXml
call c:\windows\system32\wscript "js.wsf" "setAttrXml" %1 %2 %3 %4
if %ERRORLEVEL%==-1 goto errore 
exit /b

rem function setAttrXmlObb(pathxml, pathnode, attr, value)
:setAttrXmlObb
call c:\windows\system32\wscript "js.wsf" "setAttrXmlObb" %1 %2 %3 %4
if %ERRORLEVEL%==-1 goto errore 
exit /b

rem function setNodeXml(pathxml, pathnode, value)
:setNodeXml
call c:\windows\system32\wscript "js.wsf" "setNodeXml" %1 %2 %3
if %ERRORLEVEL%==-1 goto errore 
exit /b

rem function removeChildsNode(pathxml, pathnode)
:removeChildsNode
call c:\windows\system32\wscript "js.wsf" "removeChildsNode" %1 %2
if %ERRORLEVEL%==-1 goto errore 
exit /b

rem function genMinifyXml(local_path, pathxml, out_xml, path_nodes, out_minify)
:genMinifyXml
call c:\windows\system32\wscript "js.wsf" "genMinifyXml" %1 %2 %3 %4 %5
if %ERRORLEVEL%==-1 goto errore 
exit /b

rem function removeDestModules(pathxml, path_nodes, site_path)
:removeDestModules
call c:\windows\system32\wscript "js.wsf" "removeDestModules" %1 %2 %3
if %ERRORLEVEL%==-1 goto errore 
exit /b

rem function removeNodes(pathxml, path_nodes)
:removeNodes
call c:\windows\system32\wscript "js.wsf" "removeNodes" %1 %2
if %ERRORLEVEL%==-1 goto errore 
exit /b

rem function addNode(pathxml, path_node, element, attrs)
:addNode
call c:\windows\system32\wscript "js.wsf" "addNode" %1 %2 %3 %4
if %ERRORLEVEL%==-1 goto errore 
exit /b

rem function cNodesXml(name var, pathxml, pathnodes)
:cNodesXml
call c:\windows\system32\wscript "js.wsf" "cNodesXml" %2 %3 %tmpfile%
set /p %1=<%tmpfile%
if %ERRORLEVEL%==-1 goto errore 
exit /b

rem function getAttrXml(name var, pathxml, pathnode, attr)
:getAttrXml
call c:\windows\system32\wscript "js.wsf" "getAttrXml" %2 %3 %4 %tmpfile%
set /p %1=<%tmpfile%
if %ERRORLEVEL%==-1 goto errore 
exit /b

rem function getAttrXmlNoErr(name var, pathxml, pathnode, attr, def)
:getAttrXmlNoErr
call c:\windows\system32\wscript "js.wsf" "getAttrXmlNoErr" %2 %3 %4 %5 %tmpfile%
set /p %1=<%tmpfile%
if %ERRORLEVEL%==-1 goto errore 
exit /b

rem END


:errore
echo %time%: errore durante le elaborazioni 

:end
pause
