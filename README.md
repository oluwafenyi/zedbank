# Zedbank

### Requirements
* Docker

### Services in docker-compose.yml
* database: an MSSQL server interacted with by the server and worker services.
* rabbit: a RabbitMQ server.
* server: JSON API application, served at port :8000 and :8001 on your localhost. Swagger at :8000/swagger/index.html.
* worker: a masstransit-based worker application.

## Steps to Run
* Follow the instructions at this link to setup certificates: https://github.com/dotnet/dotnet-docker/blob/main/samples/run-aspnetcore-https-development.md
* Cd to this directory and run `docker-compose up database` to startup db. Recommended to perform this step first as initial DB setup may take some time.
* Rabbit: `docker-compose up rabbit`
* Server: `docker-compose up server`
* Worker: `docker-compose up worker`
