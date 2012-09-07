$source='build\'+$args[0]+'\cooper_web'
$target='Z:\Documents\codesharp\cooper\build\'+$args[0]+'\cooper_web'
$dll=$source+'\bin\Microsoft.Web.Infrastructure.dll'
del $dll
get-childitem $target | remove-item -recurse -force -confirm:$false
xcopy $source $target  /Y /E /I /R 