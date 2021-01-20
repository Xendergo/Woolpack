dotnet build
dotnet publish -c Release -r osx-x64
dotnet publish -c Release -r win10-x64 -p:PublishReadyToRun=true
del mac-os.zip
del win64.zip
7z a -tzip win64.zip .\bin\Release\net5.0\win10-x64\publish\*
7z a -tzip mac-os.zip .\bin\Release\net5.0\osx-x64\publish\*