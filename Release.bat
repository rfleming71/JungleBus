SET NUGET_PATH=C:\nuget.exe
SET MSBUILD_PATH="C:\Program Files (x86)\MSBuild\14.0\Bin\MsBuild.exe"

%MSBUILD_PATH% JungleBus.sln /t:Clean /p:Configuration=Release
%MSBUILD_PATH% JungleBus.sln /t:Build /p:Configuration=Release

%NUGET_PATH% pack Interfaces\JungleBus.Interfaces.nuspec
%NUGET_PATH% pack JungleBus\JungleBus.nuspec
%NUGET_PATH% pack JungleBus.Testing\JungleBus.Testing.nuspec
%NUGET_PATH% pack JungleBus\JungleBus.StructureMap.nuspec