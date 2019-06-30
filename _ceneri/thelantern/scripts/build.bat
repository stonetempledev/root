set src_folder=C:\_lib\thelantern
set dest_folder=c:\tmp\tltest
C:\Windows\Microsoft.NET\Framework\v4.0.30319\aspnet_compiler.exe -v /thelantern -p %src_folder%\ -f %dest_folder%\
C:\_lib\thecommands\bin\Release\thecommands.exe --crypt_folder:%dest_folder%\configs dir:*.xml add_ext:tlenc pwd:qswdef34!£
rmdir "%dest_folder%\setup" /S /Q
del %dest_folder%\configs\*.xml
del %dest_folder%\thelantern.sln
del %dest_folder%\i.txt
del %dest_folder%\stack.txt
pause

