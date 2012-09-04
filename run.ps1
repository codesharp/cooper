.\build web Debug

$dir=Get-Location
$dir=''+$dir

$p=$dir+'\build\debug\cooper_web'
echo 'Run on IISExpress'
$env:Path=$env:Path+';C:\PROGRA~2\IIS Express'
start iexplore "http://localhost:8889"
iisexpress.exe /path:$p  /port:8889 /clr:V4.0

if($false){
$p_mono=$dir+'\build\debug\cooper_web_mono'
echo 'Run on MONO'
$env:Path=$env:Path+';C:\PROGRA~2\MONO-2~1.8\bin'
xsp4 --port 8889 --root $p_mono
}

