$source='build\'+$args[0]+'\cooper_web'
$target='..\apphb-cooper'
get-childitem $target | remove-item -recurse -force -confirm:$false
xcopy $source $target  /Y /E /I /R 