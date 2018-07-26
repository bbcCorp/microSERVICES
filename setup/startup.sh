#!/bin/bash

# create the mapped directories used by docker the container volumes
mkdir -p ./data/mongo
mkdir -p ./data/kafka
mkdir -p ./data/api-customers
mkdir -p ./data/service-notification
mkdir -p ./data/service-replication

docker-compose up -d