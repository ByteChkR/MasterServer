set "origCD=%cd%"
cd %1
dotnet run -c Release %origCD%\%2.assemblyconfig -b %3
cd %origCD%