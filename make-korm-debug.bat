@echo off
SET SDK="c:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\"
SET PATH=%PATH%;%SDK%

msbuild kosson.korm.sln /m /t:Clean /p:Configuration=Debug
msbuild kosson.korm.sln /m /t:Build /p:Configuration=Debug
msbuild kosson.korm.sln /m /t:Pack /p:Configuration=Debug
pause