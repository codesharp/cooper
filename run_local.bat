@echo off
echo Run on IISExpress
echo Prepending 'C:\PROGRA~2\IIS Express' to PATH
PATH=C:\PROGRA~2\IIS Express;%PATH%
start iexplore "http://localhost:8889"
iisexpress.exe /path:%~dp0src\Cooper.Web  /port:8889 /clr:V4.0