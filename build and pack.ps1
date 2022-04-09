dotnet build Trex.sln -p:Version=45.0.0 -c release
dotnet pack .\t-rex\t-rex.csproj -c:release --no-build -p:Version=45.0.0
dotnet pack .\TRexLib.nuget\TRexLib.nuget.csproj -c:release --no-build -p:Version=45.0.0