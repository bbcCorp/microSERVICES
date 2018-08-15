#!/bin/bash
# --------------------------------------------------------------#
#                       initdb.sh                                #
# --------------------------------------------------------------#

mkdir -p ./../data/postgres/db
mkdir -p ./../data/postgres/logs

docker-compose up -d

APP_DB_HOST=localhost
APP_DB_PORT=5433
APP_DB_USER=postgres
APP_DB=microSERVICE
PGPASSWORD=postgres123 

docker-compose up -d

# Wait for postgres to start
until PGPASSWORD=$POSTGRES_PASSWORD psql -h $APP_DB_HOST -p $APP_DB_PORT -U $APP_DB_USER -c '\q'; do
  >&2 echo "Postgres is unavailable - sleeping"
  sleep 1
done

psql -h $APP_DB_HOST -p $APP_DB_PORT -U $APP_DB_USER -f ./setup-identity.sql

docker-compose down