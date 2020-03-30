set src_folder=C:\_todd\root\toyn
set dest_folder=c:\tmp\toyn_setup
C:\Windows\Microsoft.NET\Framework\v4.0.30319\aspnet_compiler.exe -v /toyn -p %src_folder%\ -f %dest_folder%\
rem C:\_lib\thecommands\bin\Release\thecommands.exe --crypt_folder:%dest_folder%\configs dir:*.xml add_ext:tlenc pwd:qswdef34!£
rem rmdir "%dest_folder%\setup" /S /Q
rem del %dest_folder%\configs\*.xml
del %dest_folder%\toyn.sln
pause

