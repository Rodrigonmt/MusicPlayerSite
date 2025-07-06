@echo off
echo Iniciando o projeto ASP.NET Core...
cd /d %~dp0
dotnet build
dotnet run
pause