@echo off
setlocal

set msbuild="C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe"

set ffi=JsSourceMapper.FFI
set proj=Javascript.SourceMapper

echo Building FFI bindings
pushd %ffi%
cargo build
if %errorlevel% neq 0 (
	echo Cargo build failed
	goto :exit
)
cargo build --release
if %errorlevel% neq 0 (
	echo Cargo build failed
	goto :exit
)
popd

mkdir %proj%\costura32 2>nul

echo Publishing FFI bindings to project build directory
for /d %%d in (dll pdb) do (
	REM cp %ffi%\target\debug\JsSourceMapper_FFI.%%d %proj%\costura32\
	cp %ffi%\target\release\JsSourceMapper_FFI.%%d %proj%\costura32\
)

echo NuGet restore
nuget restore %proj%.sln
if %errorlevel% neq 0 (
	echo Could not restore NuGet packages
	goto :exit
)

echo Building solution
%msbuild% %proj%.sln
if %errorlevel% neq 0 (
	echo C# build failed
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
