# Zedbank

### Requirements
* Docker

## Steps to Run
* Follow the instructions at this link to setup certificates: https://github.com/dotnet/dotnet-docker/blob/main/samples/run-aspnetcore-https-development.md
* Cd to this directory and run `docker-compose up database` to startup db.
* Run `docker-compose up server` to startup server.
* Server application should be accessible at `http://localhost:8000` or `https:localhost:8001`
