echo off
@forfiles /s /m packages.config /c "cmd /c %1\nuget install @file -o %2 -source http://nuget.icodesharp.com/nuget"
echo on