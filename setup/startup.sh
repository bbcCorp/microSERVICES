#!/bin/bash


if [ ! -d "certs" ]; then
  # Create certificates if cert folder does not exist
  mkdir -p ./certs
  echo "You do not have any certs. For hosted applications, provide certs. We will try to generate one now."
  dotnet dev-certs https -ep ./certs/aspnetapp.pfx -p crypticpassword
  echo "Generated cert aspnetapp.pfx"
fi


# create the mapped directories used by docker the container volumes
mkdir -p ./data/mongo/db
mkdir -p ./data/mongo/logs
mkdir -p ./data/kafka
mkdir -p ./data/api-customers
mkdir -p ./data/webapp-customermgmt
mkdir -p ./data/service-notification
mkdir -p ./data/service-replication

docker-compose up -d