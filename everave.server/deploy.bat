@echo off
setlocal

:: Set variables
set RESOURCE_GROUP=everave
set ACR_NAME=everave
set IMAGE_NAME=everaveserver
set IMAGE_TAG=latest

:: Use 'az login' and 'az acr login --name everave' first

:: Build Docker image
echo Building Docker image...
docker build -t %IMAGE_NAME%:latest -f Dockerfile ..
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

:: Tag Docker image for ACR
echo Tagging Docker image...
docker tag %IMAGE_NAME%:latest %ACR_NAME%.azurecr.io/%IMAGE_NAME%:%IMAGE_TAG%
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

:: Push Docker image to ACR
echo Pushing Docker image to ACR...
docker push %ACR_NAME%.azurecr.io/%IMAGE_NAME%:%IMAGE_TAG%
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

:: Done
echo Deployment completed successfully!
pause