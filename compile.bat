dotnet build
dotnet publish -c Release -r osx-x64
dotnet publish -c Release -r win10-x64 -p:PublishReadyToRun=true