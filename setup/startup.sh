#!/bin/bash

# create the mapped directories used by docker the container volumes
mkdir -p ./data/mongo
mkdir -p ./data/api-customers

docker-compose up -d