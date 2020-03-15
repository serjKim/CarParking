# CarParking
A hybrid "real" web app written on F#.

## Description
The goal is to demonstrate an interaction between F#, SQL database and frontend part.

## Requirements:
- .NET Core 3.1 or higher
- Docker

## Getting Started
1. Build the database
```cmd
docker-compose build
docker-compose up database
```
2. Build the Web api
```cmd
cd CarParking.WebApi
dotnet run ASPNETCORE_ENVIRONMENT=Development
```
3. Web api is ready for work. That's enough to be able to use Postman or other client to play with the api. `postman_calls.json` - a collection of api calls intended to be imported to Postman.

## Build UI
4. Run the Web server
```cmd
cd CarParking.WebUI
dotnet run ASPNETCORE_ENVIRONMENT=Development
```
5. Build angular app
```cmd
cd CarParking.WebUI\frontend
npm i
npm run build
```
6. Open https://localhost:5051/
7. Enjoy

## Run Integration Tests
```cmd
docker-compose up database_tests
dotnet test -v n
```
