@echo off
setlocal

set msbuild="C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe"

set proj=Javascript.SourceMapper

echo NuGet restore
nuget restore %proj%.sln
if %errorlevel% neq 0 (
	echo Could not restore NuGet packages
	goto :exit
)

%msbuild% %proj%.sln /p:Configuration=Release
if %errorlevel% neq 0 (
	echo C# build failed
	goto :exit
)

exit /b 0

:err
exit /b %errorlevel%
