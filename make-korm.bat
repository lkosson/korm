@echo off
SET SDK="c:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\"
SET PATH=%PATH%;%SDK%

msbuild kosson.korm.sln /m /t:Clean /p:Configuration=Release
msbuild kosson.korm.sln /m /t:Build /p:Configuration=Release
msbuild kosson.korm.sln /m /t:Pack /p:Configuration=Release
pause