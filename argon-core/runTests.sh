#!/bin/zsh
echo 'Argon Test Helper'
echo 'Creating required docker test support images (MSSQL)'

# Stop the images if they already exist first
echo 'Stopping any existing test containers...'
docker stop argon
docker stop argon-test

# Blat the images 
echo 'Blatting the containers...'
docker rm argon
docker rm argon-test

# Re-create the images
echo 'Re-creating containers with parameters aligned to appsettings.Test.json'
docker run --name argon-test -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=8rG0n8dm!n' -e 'MSSQL_PID=Express' -p 1434:1433 -d mcr.microsoft.com/mssql/server:latest
docker run --name argon -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=8rG0n8dm!n' -e 'MSSQL_PID=Express' -p 1433:1433 -d mcr.microsoft.com/mssql/server:latest

echo 'Napping for 10 seconds whilst the containers sort themselves out'
sleep 10

# Build and then run the default tests
echo 'Starting the project tests'
dotnet test --logger "console;verbosity=detailed"
