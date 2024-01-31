@echo off
@dotnet publish -r win-x64 -p:PublishSingleFile=true -c Release 
@pause
