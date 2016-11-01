SET NUGET_PATH=C:\nuget.exe
SET MSBUILD_PATH="C:\Program Files (x86)\MSBuild\14.0\Bin\MsBuild.exe"

%MSBUILD_PATH% JungleBus.sln /t:Clean /p:Configuration=Release
%MSBUILD_PATH% JungleBus.sln /t:Build /p:Configuration=Release

%NUGET_PATH% pack JungleBus.Interfaces.nuspec
%NUGET_PATH% pack JungleBus.nuspec
%NUGET_PATH% pack JungleBus.StructureMap.nuspec