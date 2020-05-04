@echo off
set "origCD=%cd%"
echo AsmGen Dir: %1
echo Assembly Name: %2
echo CsProj Title: %3
echo CsProj File: %origCD%\%3.csproj
echo AssemblyConfig File: %origCD%\%2.assemblyconfig
cd %1
dotnet run -c Release %origCD%\%2.assemblyconfig --create
dotnet run -c Release %origCD%\%2.assemblyconfig -a %origCD%\%3.csproj
dotnet run -c Release %origCD%\%2.assemblyconfig -sname %2
cd %origCD%