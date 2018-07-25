#!/bin/sh

# Release Timestamp
RDATE=`date +%Y%m%d`
RVERSION=1.0.0
RENV=Development

CURRENT_DIR=`pwd`
PUBLISH_FOLDER="$CURRENT_DIR/../build/microSERVICES-$RVERSION-$RDATE/api-customers"
mkdir -p $PUBLISH_FOLDER

echo "Publishing to $PUBLISH_FOLDER"
echo "[`date +%Y%m%d_%H:%M:%S`] Building Release files for api-customers:$RVERSION"
dotnet publish "../src/app.api.customers/app.api.customers.csproj"  --output $PUBLISH_FOLDER --framework netcoreapp2.0 --configuration $RENV

# Remove old docker container images
docker rm $(docker ps -a -q)

# Remove dangling images - one with <none>
docker rmi $(docker images -f "dangling=true" -q)

cd $PUBLISH_FOLDER
# Use the docker file to create a build a docker image 
docker build -t microservices-api-customers:$RVERSION . 
echo "[`date +%Y%m%d_%H:%M:%S`] Created Docker image microservices-api-customers:$RVERSION"
