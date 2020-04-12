mkdir BuildResult
cd Byt3\Consoles\Byt3.AssemblyGenerator.CLI

call BuildGenerator.bat

cd ..\..\..\MasterServer.Client

call ASMGENBUILD.bat
move /Y .\MasterServer.Client_build ..\BuildResult\

cd ..\MasterServer

call ASMGENBUILD.bat
move /Y .\MasterServer_build ..\BuildResult\

cd ..

pause
