@echo off
setlocal

set msbuild="C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe"

set ffi=JsSourceMapper.FFI
set proj=Javascript.SourceMapper

echo Building FFI bindings
pushd %ffi%
cargo build --release --target=i686-pc-windows-msvc
if %errorlevel% neq 0 (
	echo Cargo build failed
	goto :exit
)
popd

echo Publishing FFI bindings to project build directory
mkdir %proj%\costura32 2>nul
cp %ffi%\target\i686-pc-windows-msvc\release\JsSourceMapper_FFI.dll %proj%\costura32\
if %errorlevel% neq 0 (
	echo Could not copy DLL to .NET project
	goto :exit
)
cp %ffi%\target\i686-pc-windows-msvc\release\JsSourceMapper_FFI.pdb %proj%\costura32\
if %errorlevel% neq 0 (
	echo Could not copy PDB to .NET project
	goto :exit
)

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
