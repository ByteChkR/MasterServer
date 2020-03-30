del /s /f /q MasterServer\bin\Release\*.*
for /f %%f in ('dir /ad /b .\MasterServer\bin\Release\') do rd /s /q .\MasterServer\bin\Release\%%f

dotnet build --runtime ubuntu.18.04-x64 -c Release
dotnet publish --runtime ubuntu.18.04-x64 -c Release

copy .\Settings.xml MasterServer\bin\Release\netcoreapp2.1\ubuntu.18.04-x64\publish

del MasterServerUpdate.zip
powershell .\Zip.ps1 -in MasterServer\bin\Release\netcoreapp2.1\ubuntu.18.04-x64\publish -out MasterServerUpdate.zip
