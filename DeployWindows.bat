del /s /f /q MasterServer\bin\Release\*.*
for /f %%f in ('dir /ad /b .\MasterServer\bin\Release\') do rd /s /q .\MasterServer\bin\Release\%%f

dotnet build --runtime win-x64 -c Release
dotnet publish --runtime win-x64 -c Release

del MasterServer_Windows.zip
copy SettingsWindows.xml MasterServer\bin\Release\netcoreapp2.1\win-x64\publish\Settings.xml
powershell .\Zip.ps1 -in MasterServer\bin\Release\netcoreapp2.1\win-x64\publish -out MasterServer_Windows.zip
