#!/bin/bash


if [ ! -d "certs" ]; then
  # Create certificates if cert folder does not exist
  mkdir -p ./certs
  echo "You do not have any certs. For hosted applications, provide certs. We will try to generate one now."
  dotnet dev-certs https -ep ./certs/aspnetapp.pfx -p crypticpassword
  echo "Generated cert aspnetapp.pfx"
fi


# create the mapped directories used by docker the container volumes
mkdir -p ./data/postgres/db
mkdir -p ./data/postgres/logs
mkdir -p ./data/mongo/db
mkdir -p ./data/mongo/logs
mkdir -p ./data/kafka
mkdir -p ./data/api-customers
mkdir -p ./data/webapp-customermgmt
mkdir -p ./data/service-notification
mkdir -p ./data/service-replication
mkdir -p ./data/identity-sts

while test $# -gt 0; do
  case "$1" in 
      -d|--initdb)
    echo "Setting up microSERVICE database"
    cd ./db
    # Run the initdb script to setup the database
    bash ./initdb.sh
    cd ..
    break
    ;;
  esac
done

docker-compose up -d
