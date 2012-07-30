echo off
@forfiles /s /m packages.config /c "cmd /c %1\nuget install @file -o %2 -source https://cs-nuget.apphb.com/nuget;https://nuget.org/api/v2/"
echo on