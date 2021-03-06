# Cooper

OpenSource Task-based Multiplayer Online Collaboration System.

## ICON

![logo](https://cooper.apphb.com/favicon.ico)

```shell
oooooooooooooooooooooooooooooooooooooooooooooooooo
oooooooooooooooooooooooooooooooooooooooooooooooooo
oooooooooooooooooooooooooooooooooooooooooooooooooo
oooooooooooooooooooooooooooooooooooooooooooooooooo
oooooooooooooooooooooooooooooooooooooooooooooooooo
oooooooooooooooooooooooooooooooooooooooooooooooooo
oooooooooooooooooooooooooooooooooooooooooooooooooo
oooooooooooooooooo/-.`   ``.:+ooooooosssoooooooooo
ooooooooooooooo+:`            ./oooossoooooooooooo
oooooooooooooo+.              ./ooo/-/oooooooooooo
ooooooooooooo+.            `:+ooo/.   ./oooooooooo
oooooooooo:`  .::`       .:oooo+-      `/ooooooooo
ooooooooo:   .+ooo/-`  .:oooo+-`        -ooooooooo
ooooooooo-`````-/ooo+/:+oooo/`````````` -ooooooooo
ooooooo/.````````./+++++++/-`````````````./ooooooo
oooooo+``.........`.:/++/:...............``+oooooo
oooooo+...............-:-..................+oooooo
ooooooo+-................................-+ooooooo
oooooooooo++++++++++++++++++++++++++++++oooooooooo
oooooooooooooooooooooooooooooooooooooooooooooooooo
oooooooooooooooooooooooooooooooooooooooooooooooooo
oooooooooooooooooooooooooooooooooooooooooooooooooo
oooooooooooooooooooooooooooooooooooooooooooooooooo
oooooooooooooooooooooooooooooooooooooooooooooooooo
oooooooooooooooooooooooooooooooooooooooooooooooooo
```

## Showcase

[Demo On Appharbor](https://cooper.apphb.com)

[Demo On Azure](https://cooper.websites.com)

[Public Service Coming soon](https://incooper.net)

======

## Build

Depends on git@github.com:codesharp/work-tool.git for building tools.

.NET
```shell
build nuget
build test_model Debug
build web [Debug/Test/Release]
```

MONO (build via xbuild)
```shell
sh build.sh nuget
or
sh nuget.insatll.sh
sh build.sh test_model Debug
sh build.sh web [Debug/Test/Release]
```

## Run

.NET
```shell
run.ps1
run_local.ps1 #for dev
```

MONO
```shell
sh run.sh
sh run_local.sh #for dev
```

## Style Reference

- 字体 = 黑体
- 蓝色主色调 = #2e9bd6
- 灰色普通字 = #9a9a9a 
- 灰色标题字 = #747474 加粗
- 灰色背景色 = #ebebeb
- 黑色文字 = #000
- 蓝色文字 = #1c5daf 加粗
- 图标风格 = 白色 + 投影1
- ios topbar 中间白色字体间距放小 

## Browser Support

- Good
	- Chrome
	- Safari
	- FireFox
	- IE9+
- Low
	- IE7/8
- Not
	- IE6

## License

![GPL](http://www.gnu.org/graphics/gplv3-127x51.png)

	[GPL](http://www.gnu.org/copyleft/gpl.html)
	

	Copyright (C) 2012  CodeSharp

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <http://www.gnu.org/licenses/>.
