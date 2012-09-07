#!/bin/bash
#not work on xbuild currently
#sh build.sh web Test
echo 'run on mono-mac'
xsp4 --port 8889 --root $(pwd)/build/test/cooper_web
